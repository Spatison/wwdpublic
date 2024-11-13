using Content.Shared.FixedPoint;
using Content.Shared._White.Medical.Traumas.Components;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Medical.Traumas;

#region Bones

[Serializable, NetSerializable]
public enum BoneSeverity
{
    Normal,
    Damaged,
    Broken, // Ha-ha.
}

[ByRefEvent]
public record struct BoneSeverityPointChangedEvent(EntityUid Bone, BoneComponent BoneComponent, FixedPoint2 CurrentSeverity, FixedPoint2 SeverityDelta);

[ByRefEvent]
public record struct BoneSeverityChangedEvent(EntityUid Bone, BoneSeverity NewSeverity);

#endregion
