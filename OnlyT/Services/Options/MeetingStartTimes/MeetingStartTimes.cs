using System;
using System.Collections.Generic;
using OnlyT.Utils;

namespace OnlyT.Services.Options.MeetingStartTimes;

public class MeetingStartTimes
{
    public MeetingStartTimes()
    {
        Times = new List<MeetingStartTime>();
    }

    public List<MeetingStartTime> Times { get; }

    public void Sanitize()
    {
        foreach (var startTime in Times)
        {
            startTime.Sanitize();
        }
    }

    public void FromText(string value)
    {
        Times.Clear();

        if (!string.IsNullOrWhiteSpace(value))
        {
            var lines = value.SplitIntoLines();
            foreach (var line in lines)
            {
                var startTime = MeetingStartTime.FromText(line);
                if (startTime != null)
                {
                    Times.Add(startTime);
                }
            }
        }
    }

    public string AsText()
    {
        var result = new List<string>();

        foreach (var startTime in Times)
        {
            AddStartTime(result, startTime);
        }

        return string.Join(Environment.NewLine, result);
    }

    private static void AddStartTime(List<string> result, MeetingStartTime? meetingStartTime)
    {
        var startTime = meetingStartTime?.AsText();
        if (!string.IsNullOrWhiteSpace(startTime))
        {
            result.Add(startTime);
        }
    }
}