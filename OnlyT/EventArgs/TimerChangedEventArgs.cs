namespace OnlyT.EventArgs
{
    /// <inheritdoc />
    /// <summary>
    /// Event args for change in timer values
    /// </summary>
    public class TimerChangedEventArgs : System.EventArgs
    {
        public int TargetSecs { get; set; }

        public int ElapsedSecs { get; set; }

        public bool IsRunning { get; set; }

        public int RemainingSecs => TargetSecs - ElapsedSecs;
    }
}
