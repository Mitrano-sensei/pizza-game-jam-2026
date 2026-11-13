using Godot;

/// <summary>Counts continuous laser contact. Call Hit() from OnLaserHit; resets if a physics frame is skipped.</summary>
public class HoldTimer
{
    public double Duration { get; set; }
    public double Held { get; private set; }
    public double Progress => Mathf.Min(Held / Duration, 1.0);
    private ulong _lastFrame;

    public HoldTimer(double duration) => Duration = duration;

    /// <summary>Returns true once contact has lasted Duration seconds without a break.</summary>
    public bool Hit()
    {
        var frame = Engine.GetPhysicsFrames();
        if (frame == _lastFrame) return Held >= Duration; // Second beam on the same frame: don't double count
        if (frame != _lastFrame + 1) Held = 0; // Beam was interrupted
        _lastFrame = frame;
        Held += 1.0 / Engine.PhysicsTicksPerSecond;
        return Held >= Duration;
    }
}
