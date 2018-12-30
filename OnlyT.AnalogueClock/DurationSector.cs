namespace OnlyT.AnalogueClock
{
    public class DurationSector
    {
        public double StartAngle { get; set; }

        public double CurrentAngle { get; set; }

        public double EndAngle { get; set; }

        public bool IsOvertime { get; set; }

        public bool ShowElapsedSector { get; set; }

        public DurationSector Clone()
        {
            return new DurationSector
            {
                StartAngle = StartAngle,
                CurrentAngle = CurrentAngle,
                EndAngle = EndAngle,
                IsOvertime = IsOvertime,
                ShowElapsedSector = ShowElapsedSector
            };
        }
    }
}
