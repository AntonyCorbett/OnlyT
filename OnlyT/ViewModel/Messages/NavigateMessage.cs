namespace OnlyT.ViewModel.Messages
{
    /// <summary>
    /// When we navigate between pages (e.g. from Operator page to Settings page)
    /// </summary>
    internal class NavigateMessage
    {
        public NavigateMessage(string? originalPageName, string targetPageName, object? state)
        {
            OriginalPageName = originalPageName ?? string.Empty;
            TargetPageName = targetPageName;
            State = state;
        }

        public string OriginalPageName { get; }

        /// <summary>
        /// Name of the target page
        /// </summary>
        public string TargetPageName { get; }

        /// <summary>
        /// Optional context-specific state
        /// </summary>
        public object? State { get; }
    }
}
