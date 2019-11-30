namespace OnlyT.Services.Report
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using OnlyT.Common.Services.DateTime;
    using OnlyT.Report.Models;
    using OnlyT.Report.Pdf;
    using OnlyT.Utils;
    using Serilog;

    internal static class TimingReportGeneration
    {
        // generate report and return file path (or null)
        public static Task<string> ExecuteAsync(
            ILocalTimingDataStoreService dataService, 
            IDateTimeService dateTimeService,
            IQueryWeekendService queryWeekendService,
            bool weekendIncludesFriday,
            string commandLineIdentifier)
        {
            return Task.Run(() =>
            {
                if (!dataService.ValidCurrentMeetingTimes())
                {
                    dataService.PurgeCurrentMeetingTimes();
                    Log.Logger.Warning("Meeting times invalid so not stored");
                    return null;
                }

                var outputFolder = FileUtils.GetTimingReportsFolder(commandLineIdentifier);
                Log.Logger.Debug($"Timer report output folder = {outputFolder}");

                if (string.IsNullOrEmpty(outputFolder) || !Directory.Exists(outputFolder))
                {
                    return null;
                }

                var yearFolder = GetDatedOutputFolder(outputFolder, dateTimeService.Now());
                Directory.CreateDirectory(yearFolder);
                
                if (Directory.Exists(yearFolder))
                {
                    var data = dataService.GetCurrentMeetingTimes();
                    if (data != null)
                    {
                        var historicalTimes = dataService.GetHistoricalMeetingTimes();

                        var report = new PdfTimingReport(
                            data, 
                            historicalTimes,
                            queryWeekendService,
                            weekendIncludesFriday,
                            yearFolder);

                        return report.Execute();
                    }
                }

                return null;
            });
        }

        private static string GetDatedOutputFolder(string outputFolder, DateTime now)
        {
            var monthFolderName = $"{now.Year}-{now.Month:D2}";
            return Path.Combine(outputFolder, now.Year.ToString(), monthFolderName);
        }
    }
}
