using Robust.Shared.Serialization;

namespace Content.Shared._White.TargetDoll;

public enum BodyPart
{
    Other,
    Head,
    Chest,
    Groin,
    LeftArm,
    LeftHand,
    RightArm,
    RightHand,
    LeftLeg,
    LeftFoot,
    RightLeg,
    RightFoot,
    Eyes,
    Mouth,
    Tail,
}

[Serializable, NetSerializable]
public sealed class TargetDollChangeEvent(NetEntity uid, BodyPart bodyPart) : EntityEventArgs
{
    public NetEntity Uid { get; } = uid;
    public BodyPart BodyPart { get; } = bodyPart;

}
