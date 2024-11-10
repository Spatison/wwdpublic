using Robust.Shared.GameStates;

namespace Content.Shared._White.Medical.Consciousness.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ConsciousnessRequiredComponent : Component
{
    /// <summary>
    /// Identifier, basically
    /// </summary>
    [AutoNetworkedField, DataField]
    public string Identifier = "requiredConsciousnessPart";

    /// <summary>
    /// Not having this part means death, or unconsciousness if false
    /// </summary>
    [AutoNetworkedField, DataField]
    public bool CausesDeath = true;
}
