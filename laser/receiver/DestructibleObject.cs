using Godot;

public partial class DestructibleObject : Node3D, ILaserReceiver
{
    public LaserReceiverType OnLaserHit(Vector3 hitPoint, Vector3 incomingDirection, LaserColor color)
    {
        GD.Print("Object destroyed : " + Name);
        QueueFree();
        return LaserReceiverType.StopBouncing;
    }
}