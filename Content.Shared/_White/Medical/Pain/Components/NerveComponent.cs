using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._White.Medical.Pain.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NerveComponent : Component
{
    //Yuh-uh
    [DataField, AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 PainMultiplier = 1.0f;

    /// <summary>
    /// Nerve system, to which this nerve is parented.
    /// </summary>
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public EntityUid ParentedNerveSystem;
}
