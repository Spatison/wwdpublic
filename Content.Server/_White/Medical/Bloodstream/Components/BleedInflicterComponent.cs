using Content.Server.Body.Systems;

namespace Content.Server._White.Medical.Bloodstream.Components;

[RegisterComponent, Access(typeof(BloodstreamSystem))]
public sealed partial class BleedInflicterComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public bool IsBleeding;
}
