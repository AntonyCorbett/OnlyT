namespace OnlyT.Utils
{
    using System;
    using System.IO;

    /// <summary>
    /// General file / folder utilities
    /// </summary>
    public static class FileUtils
    {
        private static readonly string AppNamePathSegment = "OnlyT";
        private static readonly string OptionsFileName = "options.json";
        private static readonly string LogsFolderName = "Logs";
        private static readonly string TimingReportsFolderName = "TimingReports";
        private static readonly string TimingReportsDatabaseFolderName = "TimingReportsDatabase";
        private static readonly string CachedFeedFileName = "feed.json";
        private static readonly string TalkScheduleFileName = "talk_schedule.xml";

        private static string? _documentFolderOverride = null;
        
        /// <summary>
        /// Creates directory if it doesn't exist. Throws if cannot be created
        /// </summary>
        /// <param name="folderPath">Directory to create</param>
        public static void CreateDirectory(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                if (!Directory.Exists(folderPath))
                {
                    // "Could not create folder {0}"
                    throw new Exception(string.Format(Properties.Resources.CREATE_FOLDER_ERROR, folderPath));
                }
            }
        }

        /// <summary>
        /// Gets the log folder
        /// </summary>
        /// <returns>Log folder</returns>
        public static string GetLogFolder()
        {
            return Path.Combine(GetOnlyTMyDocsFolder(), LogsFolderName);
        }

        /// <summary>
        /// Gets the file path for storing the user options
        /// </summary>
        /// <param name="commandLineIdentifier">Optional command-line id</param>
        /// <param name="optionsVersion">The options schema version</param>
        /// <returns>User Options file path.</returns>
        public static string GetUserOptionsFilePath(string? commandLineIdentifier, int optionsVersion)
        {
            return Path.Combine(
                GetAppDataFolder(),
                commandLineIdentifier ?? string.Empty,
                optionsVersion.ToString(),
                OptionsFileName);
        }

        /// <summary>
        /// Gets the folder for storing the timing reports
        /// </summary>
        /// <param name="commandLineIdentifier">Optional command-line id</param>
        /// <returns>Folder path.</returns>
        public static string GetTimingReportsFolder(string? commandLineIdentifier)
        {
            var folder = Path.Combine(GetOnlyTMyDocsFolder(), TimingReportsFolderName, commandLineIdentifier ?? string.Empty);
            Directory.CreateDirectory(folder);
            return folder;
        }

        /// <summary>
        /// Gets the folder for storing the timing reports db
        /// </summary>
        /// <param name="commandLineIdentifier">Optional command-line id</param>
        /// <returns>Folder path.</returns>
        public static string GetTimingReportsDatabaseFolder(string? commandLineIdentifier)
        {
            var folder = Path.Combine(GetAppDataFolder(), TimingReportsDatabaseFolderName, commandLineIdentifier ?? string.Empty);
            Directory.CreateDirectory(folder);
            return folder;
        }

        /// <summary>
        /// Gets teh full path to the cached timer feed.
        /// </summary>
        /// <returns>Full path.</returns>
        public static string GetTimesFeedPath()
        {
            return Path.Combine(GetAppDataFolder(), CachedFeedFileName);
        }

        /// <summary>
        /// Gets the full path to any manual schedule file.
        /// </summary>
        /// <returns>Full path.</returns>
        public static string GetTalkSchedulePath()
        {
            return Path.Combine(GetOnlyTMyDocsFolder(), TalkScheduleFileName);
        }

        public static void OverrideDocumentFolder(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _documentFolderOverride = null;
                return;
            }
            
            try
            {
                CreateDirectory(value);
                _documentFolderOverride = value;
            }
            catch (Exception)
            {
                // ignore exceptions
            }
        }

        /// <summary>
        /// Gets the OnlyT application data folder.
        /// </summary>
        /// <returns>AppData folder.</returns>
        private static string GetAppDataFolder()
        {
            // NB - user-specific folder
            // e.g. C:\Users\Antony\AppData\Roaming\OnlyT
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppNamePathSegment);
            Directory.CreateDirectory(folder);
            return folder;
        }

        /// <summary>
        /// Gets the application's MyDocs folder, e.g. "...MyDocuments\OnlyT"
        /// </summary>
        /// <returns>Folder path</returns>
        private static string GetOnlyTMyDocsFolder()
        {
            var folder = _documentFolderOverride ??
                         Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AppNamePathSegment);

            Directory.CreateDirectory(folder);
            return folder;
        }
    }
}
