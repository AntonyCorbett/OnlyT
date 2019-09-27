namespace OnlyT.CountdownTimer
{
    internal class AnnulusSize
    {
        public int InnerRadius { get; set; }

        public int OuterRadius { get; set; }

        public int OuterDiameter => OuterRadius * 2;
    }
}
