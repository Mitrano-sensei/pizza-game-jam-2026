using Godot;

public partial class Lamp : Node3D, ILaserReceiver, ICameraFocusable
{
	[ExportCategory("Lamp Color Settings")]
	[Export] private StandardMaterial3D _material;
	[Export] private MeshInstance3D _meshThatChangesColor;
	[Export] private LaserColor _color = LaserColor.Red;
	
	[ExportCategory("Lamp Countdown Settings")]
	[Export] private float _countdownTimeInSeconds = 10.0f;
	[Export] private float _timeToLight = 0.75f;

	[ExportCategory("Camera Focus Settings")]
	[Export] private Camera3D _camera;
	
	[ExportCategory("Lamp Sound Settings")]
	[Export(PropertyHint.Range, "0.1,3.0,0.01")] private float _minPitch = 0.9f;
	[Export(PropertyHint.Range, "0.1,3.0,0.01")] private float _maxPitch = 1.1f;

	private StandardMaterial3D _defaultMaterialValue;
	private AudioStreamPlayer3D _audioStreamPlayer3D;
	private CountdownTimer _countdownTimer;
	private HoldTimer _litTimer;
	private bool _isActive = false;
	private bool _isCountdownFreeze = false;
	
	private readonly AudioStream _lightOffSound = ResourceLoader.Load<AudioStream>("res://audio/light_off.mp3");
	private readonly AudioStream _lightOnSound = ResourceLoader.Load<AudioStream>("res://audio/light_on.mp3");
	
	[Signal] public delegate void LitEventHandler();
	[Signal] public delegate void UnlitEventHandler();

	public bool IsActive => _isActive;
	public LaserColor Color => _color;

	public override void _Ready()
	{
		base._Ready();
		// Per-instance copy of the sphere's surface material; the scene's material is shared by all lamps
		_material = (StandardMaterial3D)_meshThatChangesColor.GetActiveMaterial(0).Duplicate();
		_meshThatChangesColor.SetSurfaceOverrideMaterial(0, _material);
		_defaultMaterialValue = (StandardMaterial3D)_material.Duplicate();
		_defaultMaterialValue.AlbedoColor = _color.ToColor() * .75f;
		_material.AlbedoColor = _color.ToColor() * .75f;
		
		_audioStreamPlayer3D = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");
		
		_countdownTimer = new CountdownTimer(_countdownTimeInSeconds);
		_countdownTimer.Stop();

		_litTimer = new HoldTimer(_timeToLight);
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if (!_isActive || _isCountdownFreeze || !_countdownTimer.Tick(delta)) return;
		
		// Play Sound
		_audioStreamPlayer3D.Stream = _lightOffSound;
		_audioStreamPlayer3D.PitchScale = (float)GD.RandRange(_minPitch, _maxPitch);
		_audioStreamPlayer3D.Play();
		
		// Reset the lamp to its default state
		_material.AlbedoColor = _defaultMaterialValue.AlbedoColor;
		_material.Emission = _defaultMaterialValue.Emission;
		_isActive = false;
		EmitSignal(SignalName.Unlit);
		GD.Print("Unlit!");
	}

	public LaserReceiverType OnLaserHit(Vector3 hitPoint, Vector3 incomingDirection, LaserColor color)
	{
		if (!Equals(_color, color)) return LaserReceiverType.StopBouncing;
		if (!_isActive && !_litTimer.Hit()) return LaserReceiverType.StopBouncing;

		// GD.Print("Lamp received laser hit at " + hitPoint + " with incoming direction " + incomingDirection + " and color " + color);
		// Change color to _color
		_material.AlbedoColor = _color.ToColor();
		_material.Emission = _material.AlbedoColor * 1.75f;
		

		
		// Manage Timer
		if (!_isActive) {
			EmitSignal(SignalName.Lit);
			GD.Print("Lit!");
			
			// Play Sound
			_audioStreamPlayer3D.Stream = _lightOnSound;
			_audioStreamPlayer3D.PitchScale = (float)GD.RandRange(_minPitch, _maxPitch);
			_audioStreamPlayer3D.Play();
		}

		_countdownTimer.Restart();
		_isActive = true;
		

		return LaserReceiverType.StopBouncing;
	}
	
	public Camera3D GetCamera()
	{
		return _camera;
	}

	public void FreezeCountdown()
	{
		_isCountdownFreeze = true;
	}
}
