﻿using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._White.Medical.Traumas.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BoneComponent : Component
{
    [DataField, AutoNetworkedField, ViewVariables]
    public EntityUid BoneWoundable = EntityUid.Invalid;

    [DataField, AutoNetworkedField, ViewVariables]
    public FixedPoint2 IntegrityCap = 100;

    [DataField, AutoNetworkedField, ViewVariables]
    public FixedPoint2 BoneIntegrity = 0;

    [DataField, AutoNetworkedField, ViewVariables]
    public BoneSeverity BoneSeverity = BoneSeverity.Normal;
}
