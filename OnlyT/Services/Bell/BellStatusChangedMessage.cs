namespace OnlyT.Services.Bell
{
    internal class BellStatusChangedMessage
    {
        public BellStatusChangedMessage(bool playing)
        {
            Playing = playing;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool Playing { get; }
    }
}
