using System.Linq;
using Content.Shared.FixedPoint;
using Content.Shared._White.Medical.Pain.Components;
using Content.Shared._White.Medical.Wounds;

namespace Content.Shared._White.Medical.Pain.Systems;

public partial class PainSystem
{
    #region Data

    private readonly Dictionary<WoundSeverity, FixedPoint2> _painMultipliers = new()
    {
        { WoundSeverity.Healed, 1 },
        { WoundSeverity.Minor, 1 },
        { WoundSeverity.Moderate, 1.15 },
        { WoundSeverity.Severe, 1.35 },
        { WoundSeverity.Critical, 1.5 },
        { WoundSeverity.Loss, 1.75} // I think it will be logical, if you lose your arm, your pain WILL BE FUCKING TERRIBLE..
    };

    #endregion

    #region Public API

    /// <summary>
    /// Change pain for specific nerve, if there's any. Adds MORE PAIN to it basically.
    /// </summary>
    /// <param name="uid">Uid of the nerveSystem component owner.</param>
    /// <param name="nerveUid">Nerve uid.</param>
    /// <param name="change">How many pain to add.</param>
    /// <param name="nerveSys">NerveSystemComponent.</param>
    /// <returns>Returns true, if PAIN QUOTA WAS COLLECTED.</returns>
    public bool TryChangePainModifier(EntityUid uid, EntityUid nerveUid, FixedPoint2 change, NerveSystemComponent? nerveSys = null)
    {
        if (!Resolve(uid, ref nerveSys) || _net.IsClient)
            return false;

        if (!nerveSys.Modifiers.TryGetValue(nerveUid, out var modifier))
            return false;

        var modifierToSet =
            modifier with {Change = ApplyModifiersToPain(nerveUid, modifier.Change + change, nerveSys)};
        nerveSys.Modifiers[nerveUid] = modifierToSet;

        var ev = new PainModifierChangedEvent(uid, nerveUid, modifier.Change);
        RaiseLocalEvent(uid, ref ev);

        UpdateNerveSystemPain(uid, nerveSys);
        Dirty(uid, nerveSys);

        return true;
    }

    /// <summary>
    /// Gets copy of a pain modifier.
    /// </summary>
    /// <param name="uid">Uid of the nerveSystem component owner.</param>
    /// <param name="nerveUid">Nerve uid, used to seek for modifier..</param>
    /// <param name="modifier">Modifier copy acquired.</param>
    /// <param name="nerveSys">NerveSystemComponent.</param>
    /// <returns>Returns true, if modifier was acquired.</returns>
    public bool TryGetPainModifier(EntityUid uid, EntityUid nerveUid, out PainModifier? modifier, NerveSystemComponent? nerveSys = null)
    {
        modifier = null;
        if (_net.IsClient)
            return false;

        if (!Resolve(uid, ref nerveSys))
            return false;

        if (!nerveSys.Modifiers.TryGetValue(nerveUid, out var data))
            return false;

        modifier = data;
        return true;
    }

    /// <summary>
    /// Adds pain to needed nerveSystem, uses modifiers.
    /// </summary>
    /// <param name="uid">Uid of the nerveSystem owner.</param>
    /// <param name="nerveUid">Uid of the nerve, to which damage was applied.</param>
    /// <param name="change">Number of pain to add.</param>
    /// <param name="nerveSys">NerveSystem component.</param>
    /// <returns>Returns true, if PAIN WAS APPLIED.</returns>
    public bool TryAddPainModifier(EntityUid uid, EntityUid nerveUid, FixedPoint2 change, NerveSystemComponent? nerveSys = null)
    {
        if (!Resolve(uid, ref nerveSys) || _net.IsClient)
            return false;

        var modifier = new PainModifier(ApplyModifiersToPain(nerveUid, change, nerveSys), MetaData(nerveUid).EntityPrototype!.ID);
        if (!nerveSys.Modifiers.TryAdd(nerveUid, modifier))
            return false;

        var ev = new PainModifierAddedEvent(uid, nerveUid, change, modifier.Change);
        RaiseLocalEvent(uid, ref ev);

        UpdateNerveSystemPain(uid, nerveSys);
        Dirty(uid, nerveSys);

        return true;
    }

    /// <summary>
    /// Sets pain modifier value to needed.
    /// </summary>
    /// <param name="uid">NerveSystem owner's uid.</param>
    /// <param name="nerveUid">Nerve owner's uid.</param>
    /// <param name="change">Value to set.</param>
    /// <param name="nerveSys">NerveSystemComponent.</param>
    /// <returns>True if it was set.</returns>
    public bool TrySetPainModifier(EntityUid uid, EntityUid nerveUid, FixedPoint2 change, NerveSystemComponent? nerveSys = null)
    {
        if (!Resolve(uid, ref nerveSys) || _net.IsClient)
            return false;

        if (!TryGetPainModifier(uid, nerveUid, out var modifier))
            return false;

        var modifierToSet =
            modifier!.Value with {Change = ApplyModifiersToPain(nerveUid, change, nerveSys)};
        nerveSys.Modifiers[nerveUid] = modifierToSet;

        var ev = new PainModifierChangedEvent(uid, nerveUid, nerveSys.Pain);
        RaiseLocalEvent(uid, ref ev);

        UpdateNerveSystemPain(uid, nerveSys);
        Dirty(uid, nerveSys);

        return true;
    }

    /// <summary>
    /// Removes pain modifier.
    /// </summary>
    /// <param name="uid">NerveSystemComponent owner.</param>
    /// <param name="nerveUid">Nerve Uid, to which pain is applied.</param>
    /// <param name="nerveSys">NerveSystemComponent.</param>
    /// <returns>Returns true, if pain modifier is removed.</returns>
    public bool TryRemovePainModifier(EntityUid uid, EntityUid nerveUid, NerveSystemComponent? nerveSys = null)
    {
        if (!Resolve(uid, ref nerveSys) || _net.IsClient)
            return false;

        if (!nerveSys.Modifiers.Remove(nerveUid))
            return false;

        var ev = new PainModifierRemovedEvent(uid, nerveUid, nerveSys.Pain);
        RaiseLocalEvent(uid, ref ev);

        UpdateNerveSystemPain(uid, nerveSys);
        Dirty(uid, nerveSys);

        return true;
    }

    /// <summary>
    /// Adds pain multiplier to nerveSystem.
    /// </summary>
    /// <param name="uid">NerveSystem owner's uid.</param>
    /// <param name="identifier">ID for the multiplier.</param>
    /// <param name="change">Number to multiply.</param>
    /// <param name="nerveSys">NerveSystemComponent.</param>
    /// <returns>Returns true, if multiplier was applied.</returns>
    public bool TryAddPainMultiplier(EntityUid uid, string identifier, FixedPoint2 change, NerveSystemComponent? nerveSys = null)
    {
        if (!Resolve(uid, ref nerveSys) || _net.IsClient)
            return false;

        var modifier = new PainMultiplier(change, identifier);
        if (!nerveSys.Multipliers.TryAdd(identifier, modifier))
            return false;

        UpdatePainModifiers(nerveSys);
        UpdateNerveSystemPain(uid, nerveSys);

        Dirty(uid, nerveSys);
        return true;
    }

    /// <summary>
    /// Removes pain multiplier.
    /// </summary>
    /// <param name="uid">NerveSystem owner's uid.</param>
    /// <param name="identifier">ID to seek for the multiplier, what must be removed.</param>
    /// <param name="nerveSys">NerveSystemComponent.</param>
    /// <returns>Returns true, if multiplier was removed.</returns>
    public bool TryRemovePainMultipliers(EntityUid uid, string identifier, NerveSystemComponent? nerveSys = null)
    {
        if (!Resolve(uid, ref nerveSys) || _net.IsClient)
            return false;

        if (!nerveSys.Multipliers.Remove(identifier))
            return false;

        UpdatePainModifiers(nerveSys);
        UpdateNerveSystemPain(uid, nerveSys);

        Dirty(uid, nerveSys);
        return true;
    }

    #endregion

    #region Private API

    private void UpdateNerveSystemPain(EntityUid uid, NerveSystemComponent nerveSys)
    {
        if (_net.IsClient)
            return;

        nerveSys.Pain = nerveSys.Modifiers.Aggregate((FixedPoint2) 0, (current, modifier) => current + modifier.Value.Change);

        if (nerveSys.Pain > nerveSys.PainCap)
            nerveSys.Pain = nerveSys.PainCap;

        if (nerveSys.Pain < 0)
            nerveSys.Pain = 0; //Nuh-uh.

        CleanNerveSystemModifiers(uid, nerveSys);
    }

    private void CleanNerveSystemModifiers(EntityUid nerveSysOwner, NerveSystemComponent nerveSys)
    {
        if (_net.IsClient)
            return;

        foreach (var modifier in
                 nerveSys.Modifiers.Where(modifier => modifier.Value.Change <= 0))
        {
            TryRemovePainModifier(nerveSysOwner, modifier.Key);
        }
    }

    private void UpdatePainModifiers(NerveSystemComponent nerveSys)
    {
        if (_net.IsClient)
            return;

        foreach (var modifier in nerveSys.Modifiers)
        {
            var modifierToSet =
                modifier.Value with {Change = ApplyModifiersToPainWithoutNerve(modifier.Value.Change, nerveSys)};
            nerveSys.Modifiers[modifier.Key] = modifierToSet;
        }
    }

    private FixedPoint2 ApplyModifiersToPain(EntityUid nerveUid, FixedPoint2 pain, NerveSystemComponent nerveSys, NerveComponent? nerve = null)
    {
        if (!Resolve(nerveUid, ref nerve) || _net.IsClient)
            return pain;

        var modifiedPain = pain * nerve.PainMultiplier;
        if (nerveSys.Multipliers.Count == 0)
            return modifiedPain;

        var toMultiply = nerveSys.Multipliers.Sum(multiplier => (int) multiplier.Value.Change);
        return modifiedPain * toMultiply / nerveSys.Multipliers.Count; //o(*^＠^*)o
    }

    private FixedPoint2 ApplyModifiersToPainWithoutNerve(FixedPoint2 pain, NerveSystemComponent nerveSys)
    {
        if (nerveSys.Multipliers.Count == 0 || _net.IsClient)
            return pain;

        var toMultiply = nerveSys.Multipliers.Sum(multiplier => (int) multiplier.Value.Change);
        return pain * toMultiply / nerveSys.Multipliers.Count;
    }

    #endregion
}
