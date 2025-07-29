namespace OnlyT.EventArgs;

/// <inheritdoc />
/// <summary>
/// Event args for change in timer values
/// </summary>
public class TimerChangedEventArgs : System.EventArgs
{
    public int TargetSecs { get; init; }

    public int ElapsedSecs { get; init; }

    public bool IsRunning { get; init; }

    public int ClosingSecs { get; init; }

    public int RemainingSecs => TargetSecs - ElapsedSecs;
}