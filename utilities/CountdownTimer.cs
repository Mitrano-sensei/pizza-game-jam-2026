public class CountdownTimer
{
    public double Duration { get; set; }
    public double Remaining { get; private set; }
    public bool IsRunning => Remaining > 0;

    public CountdownTimer(double duration)
    {
        Duration = duration;
    }

    public void Restart() => Remaining = Duration;

    public void Stop() => Remaining = 0;

    /// <summary>Advances the timer. Returns true only on the tick it reaches zero.</summary>
    public bool Tick(double delta)
    {
        if (!IsRunning) return false;

        Remaining -= delta;
        return !IsRunning;
    }
}
