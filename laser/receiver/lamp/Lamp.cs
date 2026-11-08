using Godot;

public partial class Lamp : Node3D, ILaserReceiver
{
    [ExportCategory("Lamp Color Settings")]
    [Export] private StandardMaterial3D _material;
    [Export] private LaserColor _color = LaserColor.Red;
    
    [ExportCategory("Lamp Countdown Settings")]
    [Export] private float _countdownTimeInSeconds = 10.0f;

    private StandardMaterial3D _defaultMaterialValue;
    private CountdownTimer _countdownTimer;
    private bool _isActive = false;

    public bool IsActive => _isActive;

    public override void _Ready()
    {
        base._Ready();
        _defaultMaterialValue = new StandardMaterial3D();
        _defaultMaterialValue.AlbedoColor = _material.AlbedoColor;
        _defaultMaterialValue.Emission = _material.Emission;
        
        _countdownTimer = new CountdownTimer(_countdownTimeInSeconds);
        _countdownTimer.Stop();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!_isActive || !_countdownTimer.Tick(delta)) return;
        
        // Reset the lamp to its default state
        _material.AlbedoColor = _defaultMaterialValue.AlbedoColor;
        _material.Emission = _defaultMaterialValue.Emission;
        _isActive = false;
        GD.Print("Unlit!");
    }

    public LaserReceiverType OnLaserHit(Vector3 hitPoint, Vector3 incomingDirection, LaserColor color)
    {
        if (!Equals(_color, color)) return LaserReceiverType.StopBouncing;

        GD.Print("Lamp received laser hit at " + hitPoint + " with incoming direction " + incomingDirection + " and color " + color);
        // Change color to _color
        _material.AlbedoColor = color switch
        {
            LaserColor.Red => new Color(1, 0, 0),
            LaserColor.Green => new Color(0, 1, 0),
            _ => new Color(0, 0, 1),
        };
        _material.Emission = _material.AlbedoColor * 1.75f;
        
        // Manage Timer
        GD.Print("Lit!");
        _countdownTimer.Restart();
        _isActive = true;

        return LaserReceiverType.StopBouncing;
    }
}