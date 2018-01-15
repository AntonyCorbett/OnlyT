namespace OnlyT.ViewModel.Messages
{
    internal class StartCountDownMessage
    {
        public int OffsetSeconds { get; }
    
        public StartCountDownMessage(int offsetSeconds)
        {
            OffsetSeconds = offsetSeconds;
        }
    }
}
