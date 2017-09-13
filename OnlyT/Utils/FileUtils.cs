using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Services.Monitors;

namespace OnlyT.Utils
{
   /// <summary>
   /// General file / folder utilities
   /// </summary>
   public static class FileUtils
   {
      private static readonly string _appNamePathSegment = "OnlyT";
      private static readonly string _optionsFileName = "options.json";

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
         return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
             _appNamePathSegment,
             "Logs");
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

      /// <summary>
      /// Gets the application's MyDocs folder, e.g. "...MyDocuments\OnlyT"
      /// </summary>
      /// <returns>Folder path</returns>
      public static string GetOnlyTMyDocsFolder()
      {
         return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), _appNamePathSegment);
      }

      /// <summary>
      /// Gets the file path for storing the user options
      /// </summary>
      /// <param name="commandLineIdentifier">Optional command-line id</param>
      /// <param name="optionsVersion">The options schema version</param>
      /// <returns></returns>
      public static string GetUserOptionsFilePath(string commandLineIdentifier, int optionsVersion)
      {
         return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
             _appNamePathSegment,
             commandLineIdentifier ?? string.Empty,
             optionsVersion.ToString(),
             _optionsFileName);
      }

      /// <summary>
      /// Gets the OnlyT application data folder
      /// </summary>
      /// <returns></returns>
      public static string GetAppDataFolder()
      {
         // NB - user-specific folder
         // e.g. C:\Users\Antony\AppData\Roaming\OnlyT
         string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _appNamePathSegment);
         Directory.CreateDirectory(folder);
         return folder;
      }


   }
}
