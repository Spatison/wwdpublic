using Content.Shared.CCVar;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Content.Client.AntiParkinsons;

// The following code is slightly esoteric and higly schizophrenic. You have been warned.

#pragma warning disable RA0002 // wraps both systems

public sealed class AntiParkinsonsSystem : EntitySystem
{
    [Dependency] private readonly IReflectionManager _refl = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;


    private bool _enabled = false;

    public override void Initialize()
    {
        UpdatesOutsidePrediction = true;
        _cfg.OnValueChanged(CCVars.PixelSnapCamera, OnEnabledDisabled, true);
        // eat sand
        foreach(Type sys in _refl.GetAllChildren<EntitySystem>())
        {
            if (sys.IsAbstract || sys == typeof(AntiParkinsonsSystem))
                continue;

            UpdatesAfter.Add(sys);
        }

        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnAttached);
        SubscribeLocalEvent<PixelSnapEyeComponent, LocalPlayerDetachedEvent>(OnDetached);
    }

    private void OnEnabledDisabled(bool val)
    {
        _enabled = val;
        if (_enabled)
        {
            if (_player.LocalEntity is not EntityUid player)
                return;

            var ppComp = EnsureComp<PixelSnapEyeComponent>(player);
            if (TryComp<EyeComponent>(player, out var eyeComp) && eyeComp.Eye != null)
            {
                ppComp.EyePosition = eyeComp.Eye.Position;
                ppComp.EyePositionModified = eyeComp.Eye.Position;
                ppComp.EyeOffset = eyeComp.Eye.Offset;
                ppComp.EyeOffsetModified = eyeComp.Eye.Offset;
            }
            if (TryComp<SpriteComponent>(player, out var sprite))
                ppComp.SpriteOffset = sprite.Offset;

        }
        else
        {
            if (_player.LocalEntity is not EntityUid player || !TryComp<PixelSnapEyeComponent>(player, out var ppComp))
                return;

            if (TryComp<EyeComponent>(player, out var eyeComp) && eyeComp.Eye != null)
            {
                eyeComp.Eye.Position = ppComp.EyePosition;
                eyeComp.Eye.Offset = ppComp.EyeOffset;
                eyeComp.Offset = ppComp.EyeOffset;
            }

            if (TryComp<SpriteComponent>(player, out var sprite) && ppComp.SpriteOffset is System.Numerics.Vector2 orig)
                sprite.Offset = orig;

            RemComp<PixelSnapEyeComponent>(player);
        }
    }

    private void OnAttached(LocalPlayerAttachedEvent args)
    {
        if (!_enabled)
            return;

        EnsureComp<PixelSnapEyeComponent>(args.Entity);
    }

    private void OnDetached(EntityUid uid, PixelSnapEyeComponent comp, LocalPlayerDetachedEvent args)
    {
        if (!_enabled)
            return;

        if (TryComp<EyeComponent>(uid, out var eyeComp) && eyeComp.Eye != null)
        {
            eyeComp.Eye.Position = comp.EyePosition;
            eyeComp.Eye.Offset = comp.EyeOffset;
            eyeComp.Offset = comp.EyeOffset;
        }

        if (TryComp<SpriteComponent>(uid, out var sprite) && comp.SpriteOffset is System.Numerics.Vector2 orig)
            sprite.Offset = orig;

        RemComp<PixelSnapEyeComponent>(uid);
    }

    public override void FrameUpdate(float frameTime)
    {
        var query = AllEntityQuery<PixelSnapEyeComponent>();

        while (query.MoveNext(out var uid, out var ppComp))
        {
            if (!TryComp<EyeComponent>(uid, out var eyeComp) || eyeComp.Eye == null)
                continue;

            if (!TryComp<TransformComponent>(eyeComp.Target, out var xform))
                xform = Transform(uid);

            if (xform.GridUid.HasValue && xform.GridUid.Value.IsValid())
                ppComp.LastParent = xform.GridUid.Value;
            else
                if (!ppComp.LastParent.IsValid())
                ppComp.LastParent = xform.ParentUid; // fallback to whatever parent we have (in this case this will probably end up being the map)

            var vec = xform.LocalPosition;
            var offset = Vector2.Zero;

            ppComp.EyePosition = eyeComp.Eye.Position;
            ppComp.EyeOffset = eyeComp.Eye.Offset;

            var eyePos = PPCamHelper.WorldPosPixelRoundToParent(eyeComp.Eye.Position.Position, ppComp.LastParent, _transform);
            var eyeOffset = PPCamHelper.WorldPosPixelRoundToParent(eyeComp.Eye.Offset, ppComp.LastParent, _transform);
            //var eyePosDiff = eyePos - eyeComp.Eye.Position.Position;

            eyeComp.Eye.Position = new(eyePos, xform.MapID);
            eyeComp.Eye.Offset = eyeOffset;
            eyeComp.Offset = eyeOffset;

            ppComp.EyePositionModified = eyeComp.Eye.Position;
            ppComp.EyeOffsetModified = eyeComp.Eye.Offset;

            if (!TryComp<SpriteComponent>(uid, out var sprite))
                continue;

            ppComp.SpriteOffset = sprite.Offset;

            var (_, diff) = PPCamHelper.WorldPosPixelRoundToParentWithDiff(xform.WorldPosition, ppComp.LastParent, _transform);
            sprite.Offset += diff;
            ppComp.SpriteOffsetModified = sprite.Offset;
        }
    }
}



public sealed class AntiParkinsonsRevertSystem : EntitySystem
{
    [Dependency] private readonly IReflectionManager _refl = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly IPlayerManager _player = default!;


    public override void Initialize()
    {
        UpdatesOutsidePrediction = true;

        // dnas tae
        foreach (Type sys in _refl.GetAllChildren<EntitySystem>())
        {
            if (sys.IsAbstract || sys == typeof(AntiParkinsonsRevertSystem))
                continue;

            UpdatesBefore.Add(sys);
        }
    }

    // dnas tae
    public override void FrameUpdate(float frameTime)
    {
        var query = AllEntityQuery<PixelSnapEyeComponent>();

        while (query.MoveNext(out var uid, out var ppComp))
        {
            if (!TryComp<EyeComponent>(uid, out var eyeComp) || eyeComp.Eye == null)
                continue;

            eyeComp.Eye.Position = PPCamHelper.CheckForChange(eyeComp.Eye.Position, ppComp.EyePositionModified, ppComp.EyePosition);
            eyeComp.Eye.Offset = PPCamHelper.CheckForChange(eyeComp.Eye.Offset, ppComp.EyeOffsetModified, ppComp.EyeOffset);
            eyeComp.Offset = eyeComp.Eye.Offset;

            if(TryComp<SpriteComponent>(uid, out var sprite))
                sprite.Offset = PPCamHelper.CheckForChange(sprite.Offset, ppComp.SpriteOffsetModified, ppComp.SpriteOffset);
        }
    }
}

#pragma warning restore RA0002


[RegisterComponent]
[UnsavedComponent]
public sealed partial class PixelSnapEyeComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid LastParent;
    [ViewVariables(VVAccess.ReadWrite)]
    public System.Numerics.Vector2 SpriteOffset, SpriteOffsetModified;
    [ViewVariables(VVAccess.ReadWrite)]
    public MapCoordinates EyePosition, EyePositionModified;
    [ViewVariables(VVAccess.ReadWrite)]
    public System.Numerics.Vector2 EyeOffset, EyeOffsetModified;

}

public static class PPCamHelper
{
    private static int roundFactor => EyeManager.PixelsPerMeter;
    public static Vector2 RoundXY(Vector2 vec) => new Vector2(MathF.Round(vec.X * roundFactor) / roundFactor, MathF.Round(vec.Y * roundFactor) / roundFactor);

    /// <summary>
    /// Translates world vector into local (to parent) vector, rounds it to a 1 over <see cref="EyeManager.PixelsPerMeter"/> and translates back to world space.
    /// </summary>
    /// <param name="worldPos"></param>
    /// <param name="parentXform"></param>
    /// <returns></returns>
    public static Vector2 WorldPosPixelRoundToParent(Vector2 worldPos, EntityUid parent, SharedTransformSystem xformSystem)
    {
        var (_, _, mat, invmat) = xformSystem.GetWorldPositionRotationMatrixWithInv(parent);
        Vector2 localSpacePos = Vector2.Transform(worldPos, invmat);
        localSpacePos = RoundXY(localSpacePos);
        Vector2 worldRoundedPos = Vector2.Transform(localSpacePos, mat);
        return worldRoundedPos;
    }

    public static (Vector2 roundedWorldPos, Vector2 LocalSpaceDiff) WorldPosPixelRoundToParentWithDiff(Vector2 worldPos, EntityUid parent, SharedTransformSystem xformSystem)
    {
        var (_, _, mat, invmat) = xformSystem.GetWorldPositionRotationMatrixWithInv(parent);
        Vector2 localSpacePos = Vector2.Transform(worldPos, invmat);
        var roundedLocalSpacePos = RoundXY(localSpacePos);
        Vector2 worldRoundedPos = Vector2.Transform(localSpacePos, mat);
        return (worldRoundedPos, roundedLocalSpacePos - localSpacePos);
    }

    public static T CheckForChange<T>(T currentValue, T modifiedValue, T originalValue) where T : IEquatable<T>
    {
        // if this is false, this means that the value tracked was changed outside
        // of the engine's FrameUpdate loop, and this change should be preserved.
        if (currentValue.Equals(modifiedValue))
            return originalValue;
        return currentValue;
    }
}
