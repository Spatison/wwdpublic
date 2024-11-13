﻿using Content.Shared.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared._White.Medical.Wounds.Components;
using Robust.Shared.Serialization;

namespace Content.Shared._White.Medical.Wounds;

[Serializable, NetSerializable]
public enum WoundType
{
    External,
    Internal,
}

[Serializable, NetSerializable]
public enum WoundSeverity
{
    Healed,
    Minor,
    Moderate,
    Severe,
    Critical,
    Loss,
}

[Serializable, NetSerializable]
public enum BleedingSeverity
{
    Minor,
    Severe,
}

[Serializable, NetSerializable]
public enum WoundableSeverity
{
    Minor,
    Moderate,
    Severe,
    Critical,
    Loss,
}

[Serializable, NetSerializable]
public enum WoundVisibility
{
    Always,
    HandScanner,
    AdvancedScanner,
}

[Serializable, NetSerializable]
public enum WoundableVisualizerKeys
{
    Wounds,
    Severity,
    Update,
}

[Serializable, NetSerializable]
public sealed class WoundsVisualizerGroupData(List<NetEntity> woundsList) : ICloneable
{
    public List<NetEntity> WoundsList = woundsList;

    public object Clone()
    {
        return new WoundsVisualizerGroupData(new List<NetEntity>(WoundsList));
    }
}

/// <summary>
/// Is called from server, when woundable is about to be deleted. After this, client processes data given in the event,
/// And updates body part visuals, after this sending the same event on the server (if it was updated),
/// saying that update is done. After the update, body part is being deleted.
/// </summary>
[Serializable, NetSerializable]
public sealed class OnWoundableLossDeleteMessage(NetEntity woundable, NetEntity body, HumanoidVisualLayers layer) : EntityEventArgs
{
    /// <summary>
    /// Woundable that is being deleted.
    /// </summary>
    public NetEntity Woundable { get; } = woundable;

    /// <summary>
    /// Woundables Body.
    /// </summary>
    public NetEntity Body { get; } = body;

    /// <summary>
    /// Woundables body part layer to update.
    /// </summary>
    public HumanoidVisualLayers Layer { get; } = layer;
}

[ByRefEvent]
public record struct WoundAddedEvent(EntityUid WoundEntity, WoundComponent WoundComponent, WoundableComponent Woundable, WoundableComponent RootWoundable);

[ByRefEvent]
public record struct WoundRemovedEvent(EntityUid WoundEntity, WoundComponent WoundComponent, WoundableComponent OldWoundable, WoundableComponent OldRootWoundable);

[ByRefEvent]
public record struct WoundableAttachedEvent(EntityUid ParentWoundableEntity, WoundableComponent ParentWoundableComponent);

[ByRefEvent]
public record struct WoundableDetachedEvent(EntityUid ParentWoundableEntity, WoundableComponent ParentWoundableComponent);

[ByRefEvent]
public record struct WoundSeverityPointChangedEvent(EntityUid Wound, WoundComponent WoundComponent, FixedPoint2 OldSeverity, FixedPoint2 NewSeverity);

[ByRefEvent]
public record struct WoundSeverityChangedEvent(EntityUid Wound, WoundSeverity NewSeverity);

[ByRefEvent]
public record struct WoundableIntegrityChangedEvent(EntityUid Woundable, FixedPoint2 CurrentIntegrity);

[ByRefEvent]
public record struct WoundableSeverityChangedEvent(EntityUid Woundable, WoundableSeverity NewSeverity);

[Serializable, DataRecord]
public record struct WoundSeverityMultiplier(FixedPoint2 Change, string Identifier = "Unspecified");

[Serializable, DataRecord]
public record struct HealingMultiplier(FixedPoint2 Change, string Identifier = "Unspecified");
