using Godot;
using System;

public partial class SpringCharacterController : RigidBody3D
{
    [ExportCategory(" Height Settings")]
    [Export] private Node3D _bodyCenter;
    [Export] private float _rideHeight = 1.4f;
    [Export] private float _rayLength = 1.7f;
    [Export] private float _springForce = 100f;
    [Export] private float _heightDamping = 7f;

    [ExportCategory(" Upright Settings")]
    [Export] private float _uprightStrength = 25f;
    [Export] private float _uprightDamping = 1f;
    [Export] private float _maxSlopeAngle = 45f;
    private bool _grounded;
    private Vector3 _groundNormal = Vector3.Up;

    [ExportCategory("Movement Settings")]
    [Export] private float _moveSpeed = 5f;
    [Export] private float _moveDamping = 7f;
    [Export] private StringName _forwardInput = "player_forward";
    [Export] private StringName _backwardInput = "player_backward";
    [Export] private StringName _leftInput = "player_rotate_left"; // For now, using left/right input for strafing instead of rotation
    [Export] private StringName _rightInput = "player_rotate_right";

    [ExportCategory("Rotation Settings")]
    [Export] private bool _isDirectionToggle = true;
    [Export] private float _lookAtAngle = 45f;
    [Export] private float _lookAtSpeed = 5f;
    [Export] private float _lookAtDamping = 5f;
    [Export] private StringName _lookLeftInput = "player_look_left";
    [Export] private StringName _lookRightInput = "player_look_right";
    [Export] private float _lookDebounce = 0.2f; // seconds
    private LookAtMode _currentLookAtMode = LookAtMode.None;
    private ulong _lastLookInputMsec;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        ManageHeight();
        KeepUpright();
        ManageMovement();
        ManageLook();
    }

    private void ManageMovement()
    {
        float moveInput = Input.GetActionStrength(_forwardInput) - Input.GetActionStrength(_backwardInput);
        float strafeInput = Input.GetActionStrength(_rightInput) - Input.GetActionStrength(_leftInput);
        Vector3 input = new Vector3(strafeInput, 0, moveInput).Normalized();
        Vector3 targetVelocity = input.Slide(_groundNormal).Normalized() * _moveSpeed * input.Length();
        Vector3 planarVelocity = LinearVelocity.Slide(_groundNormal);

        Vector3 force = (targetVelocity - planarVelocity) * _moveDamping * Mass;
        ApplyCentralForce(force);
    }

    private void KeepUpright()
    {
        Vector3 axis = GlobalBasis.Y.Cross(Vector3.Up);
        Vector3 tiltVelocity = AngularVelocity with { Y = 0 };
        ApplyTorque(axis * _uprightStrength - tiltVelocity * _uprightDamping);
    }

    private void ManageLook()
    {
        // Manage input
        ManageLookInput();

        // Set Look Direction
        float targetAngle = GetLookAtAngle();
        float currentAngle = RotationDegrees.Y;
        float angleDifference = targetAngle - currentAngle;

        float angularVelocityY = AngularVelocity.Y;
        float torque = angleDifference * _lookAtSpeed - angularVelocityY * _lookAtDamping;

        ApplyTorque(Vector3.Up * torque);
    }

    private void ManageLookInput()
    {
        if (!_isDirectionToggle)
        {
            int held = (Input.IsActionPressed(_lookRightInput) ? 1 : 0) - (Input.IsActionPressed(_lookLeftInput) ? 1 : 0);
            _currentLookAtMode = (LookAtMode)((int)LookAtMode.None + held);
            return;
        }

        int direction = 0;
        if (Input.IsActionJustPressed(_lookRightInput)) direction++;
        if (Input.IsActionJustPressed(_lookLeftInput)) direction--;
        if (direction == 0) return;

        ulong now = Time.GetTicksMsec();
        if (now - _lastLookInputMsec < (ulong)(_lookDebounce * 1000f)) return;
        _lastLookInputMsec = now;

        int nextLookAtMode = Mathf.Clamp((int)_currentLookAtMode + direction, (int)LookAtMode.Left, (int)LookAtMode.Right);
        _currentLookAtMode = (LookAtMode)nextLookAtMode;
    }

    private void ManageHeight()
    {
        // Shoot a ray downwards from the character's position to detect the ground
        Vector3 rayOrigin = _bodyCenter.GlobalPosition;
        Vector3 rayDirection = Vector3.Down;

        float slopeCos = Mathf.Max(_groundNormal.Dot(Vector3.Up), 0.5f);
        Vector3 rayEnd = rayOrigin + rayDirection * (_rayLength / slopeCos);

        // Perform the raycast
        var space = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
        query.CollideWithAreas = false;
        query.Exclude = new Godot.Collections.Array<Rid> { this.GetRid() }; // Exclude the character itself

        var hit = space.IntersectRay(query);
        if (hit.Count == 0) return;

        var hitPosition = (Vector3)hit["position"];
        var hitNormal = (Vector3)hit["normal"];
        var hitCollider = (Node3D)hit["collider"];

        _grounded = hit.Count > 0 && Mathf.RadToDeg(hitNormal.AngleTo(Vector3.Up)) <= _maxSlopeAngle;
        _groundNormal = _grounded ? hitNormal : Vector3.Up;
        if (!_grounded) return;

        var currentHeight = _bodyCenter.GlobalPosition.Y - hitPosition.Y;
        var deltaHeight = _rideHeight - currentHeight;

        // Apply a force to the character's RigidBody3D to adjust its height
        float expectedY = LinearVelocity.Slide(_groundNormal).Y; // vertical speed the slope itself (if any) causes
        var force = deltaHeight * _springForce - (LinearVelocity.Y - expectedY) * _heightDamping;
        ApplyCentralForce(Vector3.Up * force);
    }

    private float GetLookAtAngle()
    {
        switch (_currentLookAtMode)
        {
            case LookAtMode.Left:
                return -_lookAtAngle;
            case LookAtMode.Right:
                return _lookAtAngle;
            default:
                return 0f;
        }
    }

    private enum LookAtMode
    {
        Left = 0,
        None = 1,
        Right = 2
    }
}