using Godot;

public partial class CameraScript : RigidBody3D
{
	[Export] private Node3D _playerTarget;
	[Export] private float _distance = 5f;
	[Export] private float _followSpeed = 5f;
	[Export] private float _height = 1f;
	[Export] private float _xAngle = -10f; // degrees

	private CameraMode _currentCameraMode = CameraMode.FollowPlayer;
	private Camera3D _focusCamera;

	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		if (_playerTarget == null) return;

		// Position: fixed world +Z offset, ignores the target's rotation.
		// Drive velocity toward the goal (critically damped, still collides with walls)
		Vector3 targetPosition = GetTargetPosition();
		state.LinearVelocity = (targetPosition - state.Transform.Origin) * _followSpeed;

		// Rotation: set through the physics state, otherwise the body overwrites it
		state.AngularVelocity = Vector3.Zero;
		state.Transform = new Transform3D(GetRotationToTarget(), state.Transform.Origin);
	}

	private Basis GetRotationToTarget()
	{
		switch (_currentCameraMode)
		{
			case CameraMode.FollowPlayer:
				return Basis.FromEuler(new Vector3(Mathf.DegToRad(_xAngle), 0, 0));
			case CameraMode.FocusOnObject:
				return _focusCamera.GlobalTransform.Basis;
			default:
				throw new System.Exception("Unknown camera mode: " + _currentCameraMode);
		}
	}

	private Vector3 GetTargetPosition()
	{
		switch (_currentCameraMode)
		{
			case CameraMode.FollowPlayer:
				return _playerTarget.GlobalPosition + Vector3.Up * _height + Vector3.Back * _distance;
			case CameraMode.FocusOnObject:
				return _focusCamera.GlobalPosition;
			default:
				throw new System.Exception("Unknown camera mode: " + _currentCameraMode);
		}
	}

	public void SetFocus(ICameraFocusable focusable)
	{
		if (focusable == null)
		{
			return;
		}

		_focusCamera = focusable.GetCamera();
		_currentCameraMode = CameraMode.FocusOnObject;
	}

	public void SetPlayerTargetMode()
	{
		_focusCamera = null;
		_currentCameraMode = CameraMode.FollowPlayer;
	}
	
	
	private enum CameraMode
	{
		FollowPlayer,
		FocusOnObject
	}
}

public interface ICameraFocusable
{
	Camera3D GetCamera();
}
