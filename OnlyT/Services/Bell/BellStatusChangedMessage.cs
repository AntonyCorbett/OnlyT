namespace OnlyT.Services.Bell
{
    internal class BellStatusChangedMessage
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool Playing { get; }

        public BellStatusChangedMessage(bool playing)
        {
            Playing = playing;
        }
    }
}
