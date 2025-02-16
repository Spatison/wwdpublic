using Content.Shared._White.Light.Components;
using Robust.Shared.Containers;

namespace Content.Shared._White.RotatePointLight;

public abstract class SharedRotatePointLightSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<RotatePointLightComponent, EntGotInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<RotatePointLightComponent, EntGotRemovedFromContainerMessage>(OnRemoved);
    }

    //public override void FrameUpdate(float frameTime)
    //{
    //    var query = EntityQueryEnumerator<RotatePointLightComponent>();
    //
    //    while(query.MoveNext(out var uid, out var comp))
    //    {
    //        if (!comp.Enabled ||
    //            !TryComp<TransformComponent>(uid, out var xform))
    //            continue;
    //
    //        xform.LocalRotation += comp.Angle;
    //    }
    //}

    private void OnInserted(EntityUid uid, RotatePointLightComponent comp, EntGotInsertedIntoContainerMessage args)
    {
        comp.Enabled = false;
        Dirty(uid, comp);
        UpdateRotation(uid, comp);
    }

    private void OnRemoved(EntityUid uid, RotatePointLightComponent comp, EntGotRemovedFromContainerMessage args)
    {
        comp.Enabled = true;
        Dirty(uid, comp);
        UpdateRotation(uid, comp);
    }

    protected virtual void UpdateRotation(EntityUid uid, RotatePointLightComponent comp) { }

}
