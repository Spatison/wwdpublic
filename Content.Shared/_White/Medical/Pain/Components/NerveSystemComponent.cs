﻿using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._White.Medical.Pain.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NerveSystemComponent : Component
{
    /// <summary>
    /// Pain.
    /// </summary>
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 Pain = 0f;

    /// <summary>
    /// How many Pain can hold this nerve system.
    /// </summary>
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 PainCap = 100f;

    // Don't change, OR I will break your knees, filled up upon initialization.
    public Dictionary<EntityUid, NerveComponent> Nerves = new();

    // Don't add manually!! Use built-in functions.
    public Dictionary<string, PainMultiplier> Multipliers = new();
    public Dictionary<EntityUid, PainModifier> Modifiers = new();

    //TODO: thresholda!!!!
}
