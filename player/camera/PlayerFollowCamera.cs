using Godot;
using System;

public partial class PlayerFollowCamera : RigidBody3D
{
	[Export] private Node3D _target;
	[Export] private float _distance = 5f;
	[Export] private float _followSpeed = 5f;
	[Export] private float _height = 1f;
	[Export] private float _xAngle = -10f; // degrees

	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		if (_target == null) return;

		// Position: fixed world +Z offset, ignores the target's rotation.
		// Drive velocity toward the goal (critically damped, still collides with walls)
		Vector3 targetPosition = _target.GlobalPosition + Vector3.Up * _height + Vector3.Back * _distance;
		state.LinearVelocity = (targetPosition - state.Transform.Origin) * _followSpeed;

		// Rotation: set through the physics state, otherwise the body overwrites it
		state.AngularVelocity = Vector3.Zero;
		state.Transform = new Transform3D(Basis.FromEuler(new Vector3(Mathf.DegToRad(_xAngle), 0, 0)), state.Transform.Origin);
	}
}
