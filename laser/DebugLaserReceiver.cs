using Godot;

public partial class DebugLaserReceiver : Node3D, ILaserReceiver
{
    [Export] public LaserReceiverType ReceiverType { get; set; } = LaserReceiverType.StopBouncing;
        
    public LaserReceiverType OnLaserHit(Vector3 hitPoint, Vector3 incomingDirection)
    {
        GD.Print("Received laser hit at " + hitPoint + " with incoming direction " + incomingDirection);
        return ReceiverType;
    }
}

public interface ILaserReceiver
{
    LaserReceiverType OnLaserHit(Vector3 hitPoint, Vector3 incomingDirection);
}

public enum LaserReceiverType
{
    StopBouncing,
    Bounce,
    Continue
}
