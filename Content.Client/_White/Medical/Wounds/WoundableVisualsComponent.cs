namespace Content.Client._White.Medical.Wounds;

[RegisterComponent]
public sealed partial class WoundableVisualsComponent : Component
{
    [DataField] public Dictionary<string, string>? DamageOverlayGroups = new();

    [DataField] public string BleedingOverlay;

    [DataField] public List<Enum>? TargetLayers;

    public List<Enum> TargetLayerMapKeys = [];
}
