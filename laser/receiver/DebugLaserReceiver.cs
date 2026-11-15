using Godot;

public partial class DebugLaserReceiver : Node3D, ILaserReceiver
{
    [Export] public LaserReceiverType ReceiverType { get; set; } = LaserReceiverType.StopBouncing;

    public LaserReceiverType OnLaserHit(Vector3 hitPoint, Vector3 incomingDirection, LaserColor color)
    {
        GD.Print("Received laser hit at " + hitPoint + " with incoming direction " + incomingDirection + "and color " + color);
        return ReceiverType;
    }
}