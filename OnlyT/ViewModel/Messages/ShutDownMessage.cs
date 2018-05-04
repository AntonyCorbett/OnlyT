namespace OnlyT.ViewModel.Messages
{
    /// <summary>
    /// When the app is shutting down
    /// </summary>
    internal class ShutDownMessage
    {
        /// <summary>
        /// Name of the current page
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string CurrentPageName { get; }

        public ShutDownMessage(string currentPageName)
        {
            CurrentPageName = currentPageName;
        }
    }
}
