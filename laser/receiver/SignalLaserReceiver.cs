using Godot;

public partial class SignalLaserReceiver : Node3D, ILaserReceiver
{
    [Signal] public delegate void LaserHitEventHandler(Vector3 hitPoint, Vector3 incomingDirection, int color);

    [Export] public LaserReceiverType ReceiverType { get; set; } = LaserReceiverType.StopBouncing;

    public LaserReceiverType OnLaserHit(Vector3 hitPoint, Vector3 incomingDirection, LaserColor color)
    {
        EmitSignal(SignalName.LaserHit, hitPoint, incomingDirection, (int)color);
        return ReceiverType;
    }
}


public interface ILaserReceiver
{
    LaserReceiverType OnLaserHit(Vector3 hitPoint, Vector3 incomingDirection, LaserColor color);
}

public enum LaserReceiverType
{
    StopBouncing,
    Bounce,
    Continue
}

public enum LaserColor
{
    Red = 0,
    Green = 1,
    Blue = 2
}