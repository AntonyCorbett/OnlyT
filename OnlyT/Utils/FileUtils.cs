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
        /// Gets system temp folder
        /// </summary>
        /// <returns>Temp folder</returns>
        public static string GetSystemTempFolder()
        {
            return Path.GetTempPath();
        }

        /// <summary>
        /// Gets the log folder
        /// </summary>
        /// <returns>Log folder</returns>
        public static string GetLogFolder()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                AppNamePathSegment,
                "Logs");
        }

        /// <summary>
        /// Gets the application's MyDocs folder, e.g. "...MyDocuments\OnlyT"
        /// </summary>
        /// <returns>Folder path</returns>
        public static string GetOnlyTMyDocsFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AppNamePathSegment);
        }

        /// <summary>
        /// Gets the file path for storing the user options
        /// </summary>
        /// <param name="commandLineIdentifier">Optional command-line id</param>
        /// <param name="optionsVersion">The options schema version</param>
        /// <returns>User Options file path.</returns>
        public static string GetUserOptionsFilePath(string commandLineIdentifier, int optionsVersion)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                AppNamePathSegment,
                commandLineIdentifier ?? string.Empty,
                optionsVersion.ToString(),
                OptionsFileName);
        }

        /// <summary>
        /// Gets the OnlyT application data folder.
        /// </summary>
        /// <returns>AppData folder.</returns>
        public static string GetAppDataFolder()
        {
            // NB - user-specific folder
            // e.g. C:\Users\Antony\AppData\Roaming\OnlyT
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppNamePathSegment);
            Directory.CreateDirectory(folder);
            return folder;
        }

        private static bool DirectoryIsAvailable(string dir)
        {
            if (string.IsNullOrEmpty(dir))
            {
                return false;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                return Directory.Exists(dir);
            }

            return true;
        }
    }
}
