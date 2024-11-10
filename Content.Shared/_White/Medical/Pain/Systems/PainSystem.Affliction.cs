using Content.Shared.Body.Part;
using Content.Shared._White.Medical.Pain.Components;
using Content.Shared._White.Medical.Wounds;

namespace Content.Shared._White.Medical.Pain.Systems;

public partial class PainSystem
{
    private void InitAffliction()
    {
        // Pain management hooks.
        SubscribeLocalEvent<PainInflicterComponent, WoundAddedEvent>(OnPainAdded);
        SubscribeLocalEvent<PainInflicterComponent, WoundSeverityPointChangedEvent>(OnPainChanged);
    }

    #region Event Handling

    private void OnPainAdded(EntityUid uid, PainInflicterComponent pain, WoundAddedEvent args)
    {
        if (_net.IsClient)
            return;

        if (!TryComp<BodyPartComponent>(args.Woundable.RootWoundable, out var bodyPart))
            return;

        if (bodyPart.Body == null)
            return;

        var brainUid = EntityUid.Invalid;
        foreach (var organ in _body.GetBodyOrgans(bodyPart.Body.Value))
        {
            // May more than God have mercy on my soul.
            if (!TryComp<NerveSystemComponent>(organ.Id, out _))
                continue;
            brainUid = organ.Id;
        }

        if (brainUid == EntityUid.Invalid)
            return;

        pain.Pain = args.WoundComponent.WoundSeverityPoint
            * _painMultipliers[args.WoundComponent.WoundSeverity]  / 3;
        if (!TryChangePainModifier(brainUid, args.WoundComponent.Parent, pain.Pain))
        {
            TryAddPainModifier(brainUid, args.WoundComponent.Parent, pain.Pain);
        }
    }

    private void OnPainChanged(EntityUid uid, PainInflicterComponent pain, WoundSeverityPointChangedEvent args)
    {
        if (_net.IsClient)
            return;

        if (!TryComp<BodyPartComponent>(args.WoundComponent.Parent, out var bodyPart))
            return;

        if (bodyPart.Body == null)
            return;

        var brainUid = EntityUid.Invalid;
        foreach (var organ in _body.GetBodyOrgans(bodyPart.Body.Value))
        {
            if (!TryComp<NerveSystemComponent>(organ.Id, out _))
                continue;
            brainUid = organ.Id;
        }

        if (brainUid == EntityUid.Invalid)
            return;

        pain.Pain +=
            args.NewSeverity * _painMultipliers[args.WoundComponent.WoundSeverity] / 3;
        if (pain.Pain < 0)
            pain.Pain = 0;

        TryChangePainModifier(brainUid, args.WoundComponent.Parent, args.NewSeverity * _painMultipliers[args.WoundComponent.WoundSeverity] / 3);
    }

    #endregion
}
