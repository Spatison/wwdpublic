﻿using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._White.Medical.Consciousness.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ConsciousnessComponent : Component
{
    /// <summary>
    /// Represents the limit at which point the entity falls unconscious.
    /// </summary>
    [DataField(required: true)]
    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 Threshold = 30;

    /// <summary>
    /// Represents the base consciousness value before applying any modifiers.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 RawConsciousness = -1;

    /// <summary>
    /// Gets the consciousness value after applying the multiplier and clamping between 0 and Cap.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 Consciousness => FixedPoint2.Clamp(RawConsciousness * Multiplier, 0, Cap);

    /// <summary>
    /// Represents the multiplier to be applied on the RawConsciousness.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 Multiplier = 1.0;

    /// <summary>
    /// Represents the maximum possible consciousness value. Also used as the default RawConsciousness value if it is set to -1.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 Cap = 100;

    /// <summary>
    /// Represents the collection of additional effects that modify the base consciousness level.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<(EntityUid, ConsciousnessModType), ConsciousnessModifier> Modifiers = new();

    /// <summary>
    /// Represents the collection of coefficients that further modulate the consciousness level.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<(EntityUid, ConsciousnessModType), ConsciousnessMultiplier> Multipliers = new();

    /// <summary>
    /// Defines which parts of the consciousness state are necessary for the entity.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<string, (EntityUid?, bool, bool)> RequiredConsciousnessParts = new();

    // Forceful control attributes, if you change those WITHOUT a function I KILL YOU in the most BRUTAL WAY KNOWN TO MAN
    [ViewVariables(VVAccess.ReadWrite)]
    public bool PassedOut;

    [ViewVariables(VVAccess.ReadOnly)]
    public float AccumulatedPassedOutTime;

    [ViewVariables(VVAccess.ReadOnly)]
    public float PassedOutTime;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool ForceDead;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool ForceUnconscious;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsConscious = true;
    // Forceful control attributes
}
