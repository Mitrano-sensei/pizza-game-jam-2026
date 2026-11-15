using Godot;

public partial class DestructibleObject : RigidBody3D, ILaserReceiver, ICameraFocusable
{
	[Export] private Camera3D _camera;
	[Export] private Godot.Collections.Array<Node3D> _visualNodes;
	[Export] private CollisionShape3D _collisionObject;
	[Export] private GpuParticles3D _particles3D;
	[Export] private float _timeToDestroy = 1f;

	private AudioStreamPlayer3D _audioStreamPlayer3D;
	private AudioStream _sufferSound = ResourceLoader.Load<AudioStream>("res://audio/vampire.mp3");
	private AudioStream _deathSound = ResourceLoader.Load<AudioStream>("res://audio/vampire_death.mp3");


	private HoldTimer _holdTimer;

	public bool IsDestroyed { get; private set; } = false;

	public override void _Ready()
	{
		base._Ready();
		_holdTimer = new HoldTimer(_timeToDestroy);
		_audioStreamPlayer3D = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
	}

	public LaserReceiverType OnLaserHit(Vector3 hitPoint, Vector3 incomingDirection, LaserColor color)
	{
		// Play Sound
		if (!_audioStreamPlayer3D.Playing){
			_audioStreamPlayer3D.Stream = _sufferSound;
			_audioStreamPlayer3D.Play();
		}
		
		if (!_holdTimer.Hit()) return LaserReceiverType.StopBouncing;

		GD.Print("Object destroyed : " + Name);
		IsDestroyed = true;
		
		// Play Sound
		_audioStreamPlayer3D.Stream = _deathSound;
		_audioStreamPlayer3D.Play();
		
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
