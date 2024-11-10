using Content.Shared._White.TargetDoll;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client._White.TargetDoll;

public sealed class TargetDollSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public event Action<TargetDollComponent>? TargetDollStartup;
    public event Action? TargetDollShutdown;

    public override void Initialize()
    {
        SubscribeLocalEvent<TargetDollComponent, LocalPlayerAttachedEvent>(HandlePlayerAttached);
        SubscribeLocalEvent<TargetDollComponent, LocalPlayerDetachedEvent>(HandlePlayerDetached);
        SubscribeLocalEvent<TargetDollComponent, ComponentStartup>(OnTargetingStartup);
        SubscribeLocalEvent<TargetDollComponent, ComponentShutdown>(OnTargetingShutdown);
    }

    private void HandlePlayerAttached(EntityUid uid, TargetDollComponent component, LocalPlayerAttachedEvent args)
    {
        TargetDollStartup?.Invoke(component);
    }

    private void HandlePlayerDetached(EntityUid uid, TargetDollComponent component, LocalPlayerDetachedEvent args)
    {
        TargetDollShutdown?.Invoke();
    }

    private void OnTargetingStartup(EntityUid uid, TargetDollComponent component, ComponentStartup args)
    {
        if (_playerManager.LocalEntity == uid)
            TargetDollStartup?.Invoke(component);
    }

    private void OnTargetingShutdown(EntityUid uid, TargetDollComponent component, ComponentShutdown args)
    {
        if (_playerManager.LocalEntity == uid)
            TargetDollShutdown?.Invoke();
    }
}
