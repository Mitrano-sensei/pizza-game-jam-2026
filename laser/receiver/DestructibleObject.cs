using Godot;

public partial class DestructibleObject : RigidBody3D, ILaserReceiver, ICameraFocusable
{
    [Export] private Camera3D _camera;
    [Export] private Godot.Collections.Array<Node3D> _visualNodes;
    [Export] private CollisionShape3D _collisionObject;
    [Export] private GpuParticles3D _particles3D;
    [Export] private float _timeToDestroy = 1f;

    private HoldTimer _holdTimer;

    public bool IsDestroyed { get; private set; } = false;

    public override void _Ready()
    {
        base._Ready();
        _holdTimer = new HoldTimer(_timeToDestroy);
    }

    public LaserReceiverType OnLaserHit(Vector3 hitPoint, Vector3 incomingDirection, LaserColor color)
    {
        if (!_holdTimer.Hit()) return LaserReceiverType.StopBouncing;

        GD.Print("Object destroyed : " + Name);
        IsDestroyed = true;
        
        foreach (var visualNode in _visualNodes)
        {
            visualNode.Visible = false;
        }
        _collisionObject.Disabled = true;
        GravityScale = 0f;

        if (_particles3D != null)
        {
            _particles3D.Emitting = true;
        }
        
        return LaserReceiverType.StopBouncing;
    }

    public Camera3D GetCamera()
    {
        return _camera;
    }
}