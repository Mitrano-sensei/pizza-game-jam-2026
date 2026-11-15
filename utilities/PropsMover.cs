using Godot;

/// Moves a target (Vampire, Lamp...) along this node's curve. Draw the curve with the Path3D gizmo.
public partial class PropsMover : Path3D
{
    public enum MoveMode { Loop, PingPong }

    [Export] private Node3D _target;
    [Export] private float _speed = 2f;
    [Export] private MoveMode _mode = MoveMode.Loop;
    [Export] private bool _faceDirection = true;
    [Export] private float _turnSpeedDegrees = 360f; // 0 = instant

    private float _distance;
    private int _direction = 1;

    public override void _Ready()
    {
        base._Ready();
        if (_target == null || Curve == null) return;

        // Kinematic freeze: otherwise gravity fights the mover, and it still pushes other bodies
        if (_target is RigidBody3D body)
        {
            body.FreezeMode = RigidBody3D.FreezeModeEnum.Kinematic;
            body.Freeze = true;
        }

        // Start wherever the prop was placed along the path
        _distance = Curve.GetClosestOffset(ToLocal(_target.GlobalPosition));
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_target == null || Curve == null) return;

        float length = Curve.GetBakedLength();
        if (length <= 0f) return;

        _distance += _speed * (float)delta * _direction;

        if (_mode == MoveMode.Loop)
        {
            _distance = Mathf.PosMod(_distance, length);
        }
        else if (_distance >= length || _distance <= 0f)
        {
            _distance = Mathf.Clamp(_distance, 0f, length);
            _direction = -_direction;
        }

        Vector3 newPosition = ToGlobal(Curve.SampleBaked(_distance));
        Vector3 move = newPosition - _target.GlobalPosition;
        _target.GlobalPosition = newPosition;

        // Yaw only so props stay upright on slopes; assumes model front is +Z (glTF convention)
        if (_faceDirection && new Vector2(move.X, move.Z).LengthSquared() > 1e-8f)
        {
            Vector3 rotation = _target.GlobalRotation;
            float targetYaw = Mathf.Atan2(move.X, move.Z);
            rotation.Y = _turnSpeedDegrees <= 0f
                ? targetYaw
                : Mathf.RotateToward(rotation.Y, targetYaw, Mathf.DegToRad(_turnSpeedDegrees) * (float)delta);
            _target.GlobalRotation = rotation;
        }
    }
}
