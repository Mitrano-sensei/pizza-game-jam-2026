using Godot;
using System;

public partial class PlayerController : CharacterBody3D
{
	[Export] private StringName _forwardInput = "player_forward";
	[Export] private StringName _backwardInput = "player_backward";
	[Export] private StringName _rotateLeftInput = "player_rotate_left";
	[Export] private StringName _rotateRightInput = "player_rotate_right";

	[Export(PropertyHint.Range, "0.1,50,0.1")] private float _moveSpeed = 5.0f;
	[Export(PropertyHint.Range, "0.1,20,0.1")] private float _turnSpeed = 2.0f;

	public override void _PhysicsProcess(double delta)
	{
		float moveInput = Input.GetActionStrength(_forwardInput) - Input.GetActionStrength(_backwardInput);
		float turnInput = Input.GetActionStrength(_rotateRightInput) - Input.GetActionStrength(_rotateLeftInput);

		if (Mathf.Abs(turnInput) > 0.001f)
		{
			RotateY(turnInput * _turnSpeed * (float)delta);
		}

		Vector3 movement = Vector3.Zero;
		if (Mathf.Abs(moveInput) > 0.001f)
		{
			movement = Transform.Basis * Vector3.Forward * moveInput;
		}

		Velocity = new Vector3(movement.X, 0.0f, movement.Z) * _moveSpeed;
		MoveAndSlide();
	}
}
