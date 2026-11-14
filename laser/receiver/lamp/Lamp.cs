using Godot;

public partial class Lamp : Node3D, ILaserReceiver, ICameraFocusable
{
    [ExportCategory("Lamp Color Settings")]
    [Export] private StandardMaterial3D _material;
    [Export] private MeshInstance3D _meshThatChangesColor;
    [Export] private LaserColor _color = LaserColor.Red;
    
    [ExportCategory("Lamp Countdown Settings")]
    [Export] private float _countdownTimeInSeconds = 10.0f;
    [Export] private float _timeToLight = 1f;

    [ExportCategory("Camera Focus Settings")]
    [Export] private Camera3D _camera;

    private StandardMaterial3D _defaultMaterialValue;
    private CountdownTimer _countdownTimer;
    private HoldTimer _litTimer;
    private bool _isActive = false;
    
    [Signal] public delegate void LitEventHandler();
    [Signal] public delegate void UnlitEventHandler();

    public bool IsActive => _isActive;

    public override void _Ready()
    {
        base._Ready();
        // Per-instance copy of the sphere's surface material; the scene's material is shared by all lamps
        _material = (StandardMaterial3D)_meshThatChangesColor.GetActiveMaterial(0).Duplicate();
        _meshThatChangesColor.SetSurfaceOverrideMaterial(0, _material);
        _defaultMaterialValue = (StandardMaterial3D)_material.Duplicate();
        _defaultMaterialValue.AlbedoColor = GetAlbedoFromColor() * .75f;
        _material.AlbedoColor = GetAlbedoFromColor() * .75f;
        
        _countdownTimer = new CountdownTimer(_countdownTimeInSeconds);
        _countdownTimer.Stop();

        _litTimer = new HoldTimer(_timeToLight);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!_isActive || !_countdownTimer.Tick(delta)) return;
        
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
        _material.AlbedoColor = GetAlbedoFromColor();
        _material.Emission = _material.AlbedoColor * 1.75f;
        
        // Manage Timer
        if (!_isActive) EmitSignal(SignalName.Lit);
        if (!_isActive) GD.Print("Lit!");
        _countdownTimer.Restart();
        _isActive = true;

        return LaserReceiverType.StopBouncing;
    }
    
    private Color GetAlbedoFromColor()
    {
        return _color switch
        {
            LaserColor.Red => new Color(1, 0, 0),
            LaserColor.Green => new Color(0, 1, 0),
            _ => new Color(0, 0, 1),
        };
    }

    public Camera3D GetCamera()
    {
        return _camera;
    }
}