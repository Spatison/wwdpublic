using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared._White.Medical.Consciousness.Components;
using Content.Shared._White.Medical.Pain;
using Content.Shared._White.Medical.Pain.Components;

namespace Content.Shared._White.Medical.Consciousness.Systems;

public partial class ConsciousnessSystem
{
    public override void Update(float frameTime)
    {
        var consciousnessQuery = EntityQueryEnumerator<ConsciousnessComponent>();

        while (consciousnessQuery.MoveNext(out var uid, out var comp))
        {
            if (comp.PassedOut != true)
                continue;

            comp.AccumulatedPassedOutTime += frameTime;
            if (comp.PassedOutTime > comp.AccumulatedPassedOutTime)
                continue;

            comp.PassedOutTime = 0;
            comp.AccumulatedPassedOutTime = 0;

            comp.PassedOut = false;

            CheckConscious(uid);
        }
    }

    private void OnPainChanged(EntityUid uid, ConsciousnessRequiredComponent component, PainModifierChangedEvent args)
    {
        if (!TryComp<OrganComponent>(args.NerveSystem, out var nerveSysOrgan)
            || !TryComp<NerveSystemComponent>(args.NerveSystem, out var nerveSys))
            return;

        if (!SetConsciousnessModifier(nerveSysOrgan.Body!.Value, args.NerveSystem, -nerveSys.Pain, null, ConsciousnessModType.Pain))
        {
            AddConsciousnessModifier(nerveSysOrgan.Body!.Value, args.NerveSystem, -nerveSys.Pain, null, "Pain", ConsciousnessModType.Pain);
        }
    }

    private void OnConsciousnessMapInit(EntityUid uid, ConsciousnessComponent consciousness, MapInitEvent args)
    {
        if (consciousness.RawConsciousness < 0)
        {
            consciousness.RawConsciousness = consciousness.Cap;
            Dirty(uid, consciousness);
        }

        CheckConscious(uid, consciousness);
    }

    private void OnConsciousnessPartInit(EntityUid uid, ConsciousnessRequiredComponent component, ComponentInit args)
    {
        EntityUid? bodyId = null;

        if (TryComp<BodyPartComponent>(uid, out var bodyPart) && bodyPart.Body != null)
        {
            bodyId = bodyPart.Body;
        }
        else if (TryComp<OrganComponent>(uid, out var organ) && organ.Body != null)
        {
            bodyId = organ.Body;
        }

        if (bodyId == null || !TryComp<ConsciousnessComponent>(bodyId, out var consciousness))
            return;

        if (_net.IsClient)
            return;

        if (!consciousness.RequiredConsciousnessParts.TryAdd(component.Identifier, (uid, component.CausesDeath, false)))
        {
            _sawmill.Warning($"ConsciousnessRequirementPart with duplicate Identifier {component.Identifier}:{uid} added to a body:" +
                             $" {uid} this will result in unexpected behaviour!");
        }
    }

    private void OnBodyPartAdded(EntityUid uid, ConsciousnessRequiredComponent component, ref BodyPartAddedEvent args)
    {
        if (_net.IsClient)
            return;

        if (args.Part.Comp.Body == null ||
            !TryComp<ConsciousnessComponent>(args.Part.Comp.Body, out var consciousness))
            return;

        if (!consciousness.RequiredConsciousnessParts.ContainsKey(component.Identifier)
            && consciousness.RequiredConsciousnessParts[component.Identifier].Item1 != null)
        {
            _sawmill.Warning($"ConsciousnessRequirementPart with duplicate Identifier {component.Identifier}:{uid} added to a body:" +
                        $" {args.Part.Comp.Body} this will result in unexpected behaviour!");
        }

        consciousness.RequiredConsciousnessParts[component.Identifier] = (uid, component.CausesDeath, false);

        CheckRequiredParts(args.Part.Comp.Body.Value, consciousness);
    }

    private void OnBodyPartRemoved(EntityUid uid, ConsciousnessRequiredComponent component, ref BodyPartRemovedEvent args)
    {
        if (_net.IsClient)
            return;

        if (args.Part.Comp.Body == null || !TryComp<ConsciousnessComponent>(args.Part.Comp.Body.Value, out var consciousness))
            return;

        if (!consciousness.RequiredConsciousnessParts.TryGetValue(component.Identifier, out var value))
        {
            _sawmill.Warning($"ConsciousnessRequirementPart with identifier {component.Identifier} not found on body:{uid}");
            return;
        }

        consciousness.RequiredConsciousnessParts[component.Identifier] =
            (uid, value.Item2, true);

        CheckRequiredParts(args.Part.Comp.Body.Value, consciousness);
    }

    private void OnOrganAdded(EntityUid uid, ConsciousnessRequiredComponent component, OrganAddedToBodyEvent args)
    {
        if (_net.IsClient)
            return;

        if (!TryComp<ConsciousnessComponent>(args.Body, out var consciousness))
            return;

        if (!consciousness.RequiredConsciousnessParts.TryGetValue(component.Identifier, out var value) && value.Item1 != null)
        {
            _sawmill.Warning($"ConsciousnessRequirementPart with duplicate Identifier {component.Identifier}:{uid} added to a body:" +
                        $" {args.Body} this will result in unexpected behaviour!");
        }

        consciousness.RequiredConsciousnessParts[component.Identifier] = (uid, component.CausesDeath, false);

        CheckRequiredParts(args.Body, consciousness);
    }

    private void OnOrganRemoved(EntityUid uid, ConsciousnessRequiredComponent component, OrganRemovedFromBodyEvent args)
    {
        if (!TryComp<ConsciousnessComponent>(args.OldBody, out var consciousness))
            return;

        if (!consciousness.RequiredConsciousnessParts.TryGetValue(component.Identifier, out var value))
        {
            _sawmill.Warning($"ConsciousnessRequirementPart with identifier {component.Identifier} not found on body:{uid}");
            return;
        }

        consciousness.RequiredConsciousnessParts[component.Identifier] =
            (uid, value.Item2, true);

        CheckRequiredParts(args.OldBody, consciousness);
    }
}
