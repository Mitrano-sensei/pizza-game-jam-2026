using Godot;

public partial class DestructibleObject : RigidBody3D, ILaserReceiver, ICameraFocusable
{
    [Export] private Camera3D _camera;
    [Export] private Node3D _visualNode;
    [Export] private CollisionShape3D _collisionObject;
    
    public bool IsDestroyed { get; private set; } = false;
    
    public LaserReceiverType OnLaserHit(Vector3 hitPoint, Vector3 incomingDirection, LaserColor color)
    {
        GD.Print("Object destroyed : " + Name);
        IsDestroyed = true;
        
        _visualNode.Visible = false;
        _collisionObject.Disabled = true;
        GravityScale = 0f;
        
        return LaserReceiverType.StopBouncing;
    }

    public Camera3D GetCamera()
    {
        return _camera;
    }
}