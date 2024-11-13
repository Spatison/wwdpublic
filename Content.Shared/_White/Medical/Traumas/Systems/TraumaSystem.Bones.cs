using System.Linq;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.FixedPoint;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared._White.Medical.Pain.Components;
using Content.Shared._White.Medical.Traumas.Components;
using Content.Shared._White.Medical.Wounds.Components;
using Robust.Shared.Random;

namespace Content.Shared._White.Medical.Traumas.Systems;

public partial class TraumaSystem
{
    private void InitBones()
    {
        SubscribeLocalEvent<BoneComponent, BoneSeverityChangedEvent>(OnBoneSeverityChanged);
        SubscribeLocalEvent<BoneComponent, BoneSeverityPointChangedEvent>(OnBoneSeverityPointChanged);
    }

    #region Event handling

    private void OnBoneSeverityChanged(EntityUid uid, BoneComponent component, BoneSeverityChangedEvent args)
    {
        if (_net.IsClient)
            return;

        ApplyBoneDamageEffects(component);

        if (!TryComp<BodyPartComponent>(component.BoneWoundable, out var bodyPart)
            || bodyPart.Body == null || !TryComp<BodyComponent>(bodyPart.Body, out var body))
            return;

        ProcessLegsState(bodyPart.Body.Value, body);
    }

    private void OnBoneSeverityPointChanged(EntityUid uid, BoneComponent component, BoneSeverityPointChangedEvent args)
    {
        if (_net.IsClient)
            return;

        if (!TryComp<BodyPartComponent>(component.BoneWoundable, out var bodyPart)
            || bodyPart.Body == null || !TryComp<BodyComponent>(bodyPart.Body, out var body))
            return;

        var brainUid = EntityUid.Invalid;
        foreach (var child in _body.GetBodyOrgans(bodyPart.Body, body))
        {
            if (!TryComp<NerveSystemComponent>(child.Id, out _))
                continue;
            brainUid = child.Id;
        }

        if (brainUid == EntityUid.Invalid)
            return;

        if (!_pain.TryChangePainModifier(brainUid, component.BoneWoundable,
                args.SeverityDelta * _bonePainModifiers[component.BoneSeverity]))
        {
            _pain.TryAddPainModifier(brainUid, component.BoneWoundable,
                args.SeverityDelta * _bonePainModifiers[component.BoneSeverity]);
        }
    }


    #endregion

    #region Public API

    public bool ApplyDamageToBone(EntityUid bone, FixedPoint2 severity, BoneComponent? boneComp = null)
    {
        if (!Resolve(bone, ref boneComp) || _net.IsClient)
            return false;

        boneComp.BoneIntegrity = FixedPoint2.Clamp(boneComp.BoneIntegrity + severity, 0, 100);

        CheckBoneSeverity(bone, boneComp);

        var ev = new BoneSeverityPointChangedEvent(bone, boneComp, boneComp.BoneIntegrity, severity);
        RaiseLocalEvent(bone, ref ev, true);

        Dirty(bone, boneComp);
        return true;
    }

    public bool RandomBoneTraumaChance(WoundableComponent woundableComp)
    {
        var bone = Comp<BoneComponent>(woundableComp.Bone!.ContainedEntities[0]);

        // We do complete random to get the chance for trauma to happen,
        // We combine multiple parameters and do some math, to get the chance.
        // Even if we get 0.1 damage there's still a chance for injury to be applied, but with the extremely low chance.
        // The more damage, the bigger is the chance.
        var chance =
            woundableComp.WoundableIntegrity / (woundableComp.WoundableIntegrity + bone.BoneIntegrity)
            * _boneTraumaChanceMultipliers[woundableComp.WoundableSeverity];

        // Some examples of how this works:
        // 38 / (38 + 19) * 0.3 (Moderate) = 0.2. Or 20%:
        // 19 / (19 + 0) * 0.1 (Minor) = 0.1. Or 10%;
        // 57 / (57 + 17) * 0.5 (Severe) = 0.39~. Or approximately 40%;

        return _random.Prob((float) chance);
    }

    #endregion

    #region Private API

    private void CheckBoneSeverity(EntityUid bone, BoneComponent boneComp)
    {
        if (_net.IsClient)
            return;

        var nearestSeverity = boneComp.BoneSeverity;

        foreach (var (severity, value) in _boneThresholds.OrderByDescending(kv => kv.Value))
        {
            if (boneComp.BoneIntegrity < value)
                continue;

            nearestSeverity = severity;
            break;
        }

        if (nearestSeverity != boneComp.BoneSeverity)
        {
            var ev = new BoneSeverityChangedEvent(bone, nearestSeverity);
            RaiseLocalEvent(bone, ref ev, true);
        }
        boneComp.BoneSeverity = nearestSeverity;

        Dirty(bone, boneComp);
    }

    private void ApplyBoneDamageEffects(BoneComponent boneComp)
    {
        if (_net.IsClient)
            return;

        var bodyPart = Comp<BodyPartComponent>(boneComp.BoneWoundable);

        if (bodyPart.Body == null || !TryComp<BodyComponent>(bodyPart.Body, out var body))
            return;

        if (bodyPart.PartType != BodyPartType.Leg || body.RequiredLegs <= 0)
            return;

        if (!TryComp<MovementBodyPartComponent>(boneComp.BoneWoundable, out var movementPart))
            return;

        var modifier = boneComp.BoneSeverity switch
        {
            BoneSeverity.Normal => 1f,
            BoneSeverity.Damaged => 0.6f,
            BoneSeverity.Broken => 0f,
            _ => 1f
        };

        movementPart.WalkSpeed *= modifier;
        movementPart.SprintSpeed *= modifier;
        movementPart.Acceleration *= modifier;

        UpdateLegsMovementSpeed(bodyPart.Body.Value, body);
    }

    private void ProcessLegsState(EntityUid body, BodyComponent bodyComp)
    {
        if (_net.IsClient)
            return;

        var brokenLegs = 0;
        foreach (var legEntity in bodyComp.LegEntities)
        {
            if (!TryComp<WoundableComponent>(legEntity, out var legWoundable))
                continue;

            if (Comp<BoneComponent>(legWoundable.Bone!.ContainedEntities[0]).BoneSeverity == BoneSeverity.Broken)
            {
                brokenLegs++;
            }
        }

        if (brokenLegs >= bodyComp.LegEntities.Count / 2 && brokenLegs < bodyComp.LegEntities.Count)
        {
            _movementSpeed.ChangeBaseSpeed(body, 0.4f, 0.4f, 0.4f);
        }
        else if (brokenLegs == bodyComp.LegEntities.Count)
        {
            _standing.Down(body);
        }
        else
        {
            _standing.Stand(body);
            _movementSpeed.ChangeBaseSpeed(body, 1f, 1f, 1f);
        }
    }

    private void UpdateLegsMovementSpeed(EntityUid body, BodyComponent bodyComp)
    {
        if (_net.IsClient)
            return;

        var walkSpeed = 0f;
        var sprintSpeed = 0f;
        var acceleration = 0f;

        foreach (var legEntity in bodyComp.LegEntities)
        {
            if (!TryComp<MovementBodyPartComponent>(legEntity, out var legModifier))
                continue;

            if (!TryComp<BodyPartComponent>(legEntity, out var bodyPart))
                continue;

            var feet =
                (from child in _body.GetBodyPartChildren(legEntity, bodyPart) where child.Component.PartType == BodyPartType.Foot select child.Id).ToList();

            var feetModifier = 1f;
            if (feet.Count != 0 && TryComp<BoneComponent>(feet.First(), out var bone)
                && bone.BoneSeverity == BoneSeverity.Broken)
            {
                feetModifier = 0.4f;
            }

            walkSpeed += legModifier.WalkSpeed * feetModifier;
            sprintSpeed += legModifier.SprintSpeed * feetModifier;
            acceleration += legModifier.Acceleration * feetModifier;
        }
        walkSpeed /= bodyComp.RequiredLegs;
        sprintSpeed /= bodyComp.RequiredLegs;
        acceleration /= bodyComp.RequiredLegs;

        _movementSpeed.ChangeBaseSpeed(body, walkSpeed, sprintSpeed, acceleration);
    }

    #endregion
}
