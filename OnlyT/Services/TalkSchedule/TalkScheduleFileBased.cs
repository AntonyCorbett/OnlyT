using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using OnlyT.Models;
using OnlyT.Utils;
using Serilog;

namespace OnlyT.Services.TalkSchedule
{
    /// <summary>
    /// The talk schedule when using "File-based" operating mode
    /// </summary>
    internal static class TalkScheduleFileBased
    {
        private static readonly string _fileName = "talk_schedule.xml";

        private static string GetFullPath()
        {
            string path = FileUtils.GetOnlyTMyDocsFolder();
            Directory.CreateDirectory(path);
            return Path.Combine(path, _fileName);
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

        public static IEnumerable<TalkScheduleItem> Read()
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

                        int talkId = 1000;
                        foreach (XElement elem in items.Elements("item"))
                        {
                            result.Add(new TalkScheduleItem
                            {
                                Id = talkId++,
                                Name = (string)elem.Attribute("name"),
                                CountUp = AttributeToNullableBool(elem.Attribute("countup"), null),
                                OriginalDuration = AttributeToDuration(elem.Attribute("duration")),
                                Editable = AttributeToBool(elem.Attribute("editable"), false),
                                Bell = AttributeToBool(elem.Attribute("bell"), false)
                            });
                        }

                        PrefixDurationsToTalkNames(result);
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

        private static void PrefixDurationsToTalkNames(IReadOnlyCollection<TalkScheduleItem> talks)
        {
            foreach (var talk in talks)
            {
                talk.PrefixDurationToName();
            }
        }
    }
}
