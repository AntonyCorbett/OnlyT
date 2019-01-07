namespace OnlyT.Services.TalkSchedule
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;
    using Models;
    using Serilog;
    using Utils;

    /// <summary>
    /// The talk schedule when using "File-based" operating mode
    /// </summary>
    internal static class TalkScheduleFileBased
    {
        private static readonly string FileName = "talk_schedule.xml";
        private static readonly int StartId = 5000;

        public static IEnumerable<TalkScheduleItem> Read(bool autoBell)
        {
            List<TalkScheduleItem> result = null;

            if (Exists())
            {
                var path = GetFullPath();
                try
                {
                    XDocument x = XDocument.Load(path);
                    var items = x.Root?.Element("items");
                    if (items != null)
                    {
                        result = new List<TalkScheduleItem>();

                        int talkId = StartId;
                        foreach (XElement elem in items.Elements("item"))
                        {
                            result.Add(new TalkScheduleItem
                            {
                                Id = talkId++,
                                Name = (string)elem.Attribute("name"),
                                MeetingSectionNameInternal = (string)elem.Attribute("section"),
                                MeetingSectionNameLocalised = (string)elem.Attribute("section"),
                                CountUp = AttributeToNullableBool(elem.Attribute("countup"), null),
                                OriginalDuration = AttributeToDuration(elem.Attribute("duration")),
                                Editable = AttributeToBool(elem.Attribute("editable"), false),
                                BellApplicable = AttributeToBool(elem.Attribute("bell"), false),
                                AutoBell = autoBell,
                                PersistFinalTimerValue = AttributeToBool(elem.Attribute("persist"), false)
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = null;
                    Log.Logger.Error(ex, $"Unable to read {path}");
                }
            }

            return result;
        }

        private static string GetFullPath()
        {
            string path = FileUtils.GetOnlyTMyDocsFolder();
            FileUtils.CreateDirectory(path);
            return Path.Combine(path, FileName);
        }

        private static bool Exists()
        {
            return File.Exists(GetFullPath());
        }

        private static bool? AttributeToNullableBool(XAttribute attribute, bool? defaultValue)
        {
            return string.IsNullOrEmpty(attribute?.Value)
                ? defaultValue
                : Convert.ToBoolean(attribute.Value);
        }

        private static bool AttributeToBool(XAttribute attribute, bool defaultValue)
        {
            return attribute == null
               ? defaultValue
               : Convert.ToBoolean(attribute.Value);
        }

        private static TimeSpan AttributeToDuration(XAttribute attribute)
        {
            return attribute == null
               ? TimeSpan.Zero
               : StringToTimeSpan(attribute.Value);
        }

        private static TimeSpan StringToTimeSpan(string s)
        {
            TimeSpan.TryParse(s, out var result);
            return result;
        }
    }
}
