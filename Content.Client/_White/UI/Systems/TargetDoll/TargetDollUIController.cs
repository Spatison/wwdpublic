using Content.Client.Gameplay;
using Content.Client._White.TargetDoll;
using Content.Client._White.UI.Systems.TargetDoll.Widgets;
using Content.Shared._White.TargetDoll;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.Player;

namespace Content.Client._White.UI.Systems.TargetDoll;

public sealed class TargetDollUIController : UIController, IOnStateEntered<GameplayState>, IOnSystemChanged<TargetDollSystem>
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IEntityNetworkManager _net = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private TargetDollComponent? _targetingComponent;
    private TargetDollGui? TargetDollGui => UIManager.GetActiveUIWidgetOrNull<TargetDollGui>();

    public void OnSystemLoaded(TargetDollSystem system)
    {
        system.TargetDollStartup += AddTargetingControl;
        system.TargetDollShutdown += RemoveTargetingControl;
    }

    public void OnSystemUnloaded(TargetDollSystem system)
    {
        system.TargetDollStartup -= AddTargetingControl;
        system.TargetDollShutdown -= RemoveTargetingControl;
    }

    public void OnStateEntered(GameplayState state)
    {
        if (TargetDollGui == null)
            return;

        TargetDollGui.SetTargetDollVisible(_targetingComponent != null);

        if (_targetingComponent != null)
            TargetDollGui.SetBodyPartsVisible(_targetingComponent.Target);
    }

    public void AddTargetingControl(TargetDollComponent component)
    {
        _targetingComponent = component;

        if (TargetDollGui == null)
            return;

        TargetDollGui.SetTargetDollVisible(_targetingComponent != null);

        if (_targetingComponent != null)
            TargetDollGui.SetBodyPartsVisible(_targetingComponent.Target);

    }

    public void RemoveTargetingControl()
    {
        if (TargetDollGui != null)
            TargetDollGui.SetTargetDollVisible(false);

        _targetingComponent = null;
    }

    public void CycleTarget(BodyPart bodyPart)
    {
        if (_playerManager.LocalEntity is not { } user
            || _entManager.GetComponent<TargetDollComponent>(user) is not { } targetingComponent
            || TargetDollGui == null)
            return;

        var player = _entManager.GetNetEntity(user);

        if (bodyPart == targetingComponent.Target)
            return;

        var msg = new TargetDollChangeEvent(player, bodyPart);
        _net.SendSystemNetworkMessage(msg);
        TargetDollGui?.SetBodyPartsVisible(bodyPart);
    }
}
