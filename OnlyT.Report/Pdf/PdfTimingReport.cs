namespace OnlyT.Report.Pdf
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using OnlyT.Report.Models;
    using OnlyT.Report.Properties;
    using PdfSharp.Charting;
    using PdfSharp.Drawing;
    using PdfSharp.Pdf;
    using Serilog;

    public class PdfTimingReport
    {
        private const int LargestDeviationMins = 10;
        private const int MinChartSeries = 5;

        private readonly MeetingTimes _data;
        private readonly string _outputFolder;
        private readonly XBrush _blackBrush = XBrushes.Black;
        private readonly XBrush _grayBrush = XBrushes.Gray;
        private readonly XBrush _lightGrayBrush = XBrushes.LightGray;
        private readonly XBrush _greenBrush = XBrushes.Green;
        private readonly XBrush _redBrush = XBrushes.Red;
        private readonly HistoricalMeetingTimes _historicalASummary;

        private XFont _titleFont;
        private XFont _subTitleFont;
        private XFont _itemTitleFont;
        private XFont _itemFont;
        private XFont _stdTimeFont;
        private XFont _durationFont;
        private XFont _smallTimeFont;
        private double _leftMargin;
        private double _leftIndent;
        private double _topMargin;
        private double _currentY;
        private double _rightX;
        private double _rightXIndent;
        
        public PdfTimingReport(MeetingTimes data, HistoricalMeetingTimes historicalASummary, string outputFolder)
        {
            _data = data;
            _historicalASummary = historicalASummary;
            _outputFolder = outputFolder;
        }

        private enum ChartMeetingType
        {
            Midweek,
            Weekend,
            Both
        }

        public string Execute()
        {
            var fileName = GetOutputFileName();

            Log.Logger.Debug($"Executing PdfTimerReport: {fileName}");

            using (var doc = new PdfDocument())
            {
                var page = doc.AddPage();

                using (var g = XGraphics.FromPdfPage(page))
                {
                    Log.Logger.Debug("Creating fonts and page metrics");
                    CreateFonts();
                    CalcMetrics(page);

                    DrawTitle(g, Resources.REPORT_TITLE);

                    var itemArray = GetSanitizedItemTimings();
                    for (int n = 0; n < itemArray.Length; ++n)
                    {
                        var item = itemArray[n];

                        Log.Logger.Debug($"Printing item: {item.Description}, {item.Start} - {item.End}");

                        DrawItem(g, item);
                        if (item.IsStudentTalk && n != itemArray.Length - 1)
                        {
                            var nextItem = itemArray[n + 1];

                            var counselDuration = nextItem.Start - item.End;

                            if (counselDuration.TotalSeconds < 0.0)
                            {
                                counselDuration = TimeSpan.FromSeconds(0);
                            }

                            DrawCounselItem(g, counselDuration);
                        }
                    }

                    if (_data.MeetingPlannedEnd != default(TimeSpan) && _data.MeetingActualEnd != default(TimeSpan))
                    {
                        Log.Logger.Debug("Printing summary");
                        DrawSummary(g, _data.MeetingPlannedEnd, _data.MeetingActualEnd);
                    }
                }

                Log.Logger.Debug("Printing historical summary");
                DrawHistoricalSummary(doc);

                doc.Save(fileName);
            }

            return fileName;
        }

        private MeetingTimedItem[] GetSanitizedItemTimings()
        {
            var result = new List<MeetingTimedItem>();
            
            foreach (var item in _data.Items)
            {
                // ignore very short items that are likely mistakes
                if ((item.End - item.Start).TotalSeconds > 20)
                {
                    if (item.Start >= _data.MeetingStart && item.End <= _data.MeetingActualEnd)
                    {
                        result.Add(item);
                    }
                }
            }

            return result.ToArray();
        }

        private void DrawHistoricalSummary(PdfDocument doc)
        {
            if (_historicalASummary != null && UsableSummaryCount(ChartMeetingType.Both) > MinChartSeries)
            {
                var page = doc.AddPage();
                CalcMetrics(page);

                using (var g = XGraphics.FromPdfPage(page))
                {
                    DrawTitle(g, Resources.TIMING_SUMMARY);

                    DrawChart(g, page, ChartMeetingType.Midweek);
                    DrawChart(g, page, ChartMeetingType.Weekend);
                    DrawChart(g, page, ChartMeetingType.Both);
                }
            }
        }

        private void DrawChart(XGraphics g, PdfPage page, ChartMeetingType mtgType)
        {
            if (UsableSummaryCount(mtgType) > MinChartSeries)
            {
                Chart c = new Chart(ChartType.Column2D);
                c.Font.Name = "Verdana";
                
                var xSeries = c.XValues.AddXSeries();
                var ySeries = c.SeriesCollection.AddSeries();

                c.YAxis.MaximumScale = LargestDeviationMins;
                c.YAxis.MinimumScale = -LargestDeviationMins;
                c.YAxis.LineFormat.Visible = true;

                c.PlotArea.LineFormat.Visible = true;

                c.YAxis.MajorTick = 5;
                c.YAxis.Title.Caption = Resources.OVERTIME_MINS;
                c.YAxis.Title.Orientation = 90;
                c.YAxis.Title.VerticalAlignment = VerticalAlignment.Center;
                c.YAxis.HasMajorGridlines = true;
                c.YAxis.MajorGridlines.LineFormat.Color = XColor.FromGrayScale(50);
                c.YAxis.MajorTickMark = TickMarkType.Outside;

                c.XAxis.MajorTickMark = TickMarkType.None;

                switch (mtgType)
                {
                    case ChartMeetingType.Midweek:
                        c.XAxis.Title.Caption = Resources.MIDWEEK_MTGS;
                        break;
                    case ChartMeetingType.Weekend:
                        c.XAxis.Title.Caption = Resources.WEEKEND_MTGS;
                        break;
                    case ChartMeetingType.Both:
                        c.XAxis.Title.Caption = Resources.MIDWEEK_AND_WEEKEND_MTGS;
                        break;
                }

                var currentMonth = default(DateTime);

                foreach (var summary in _historicalASummary.Summaries)
                {
                    if (UseSummary(summary, mtgType))
                    {
                        if (summary.MeetingDate.Year != currentMonth.Year ||
                            summary.MeetingDate.Month != currentMonth.Month)
                        {
                            string monthName = summary.MeetingDate.ToString("MMM", CultureInfo.CurrentUICulture);
                            xSeries.Add(monthName);
                            currentMonth = summary.MeetingDate;
                        }
                        else
                        {
                            xSeries.AddBlank();
                        }

                        var p = ySeries.Add(LimitOvertime(summary.Overtime.TotalMinutes));
                        p.FillFormat.Color = XColor.FromKnownColor(p.Value > 0 ? XKnownColor.Red : XKnownColor.Green);
                    }
                }

                var frame = new ChartFrame();

                var chartHeight = _itemFont.Height * 15;
                frame.Size = new XSize(page.Width - (_leftMargin * 2), chartHeight);
                frame.Location = new XPoint(_leftMargin, _currentY);
                frame.Add(c);
                frame.DrawChart(g);

                _currentY += chartHeight + (_itemFont.Height * 2);
            }
        }

        private int UsableSummaryCount(ChartMeetingType mtgType)
        {
            int count = 0;

            foreach (var s in _historicalASummary.Summaries)
            {
                if (UseSummary(s, mtgType))
                {
                    ++count;
                }
            }

            return count;
        }

        private bool UseSummary(MeetingTimeSummary summary, ChartMeetingType mtgType)
        {
            if (mtgType == ChartMeetingType.Both ||
                (mtgType == ChartMeetingType.Weekend &&
                 (summary.MeetingDate.DayOfWeek == DayOfWeek.Saturday || summary.MeetingDate.DayOfWeek == DayOfWeek.Sunday)) ||
                (mtgType == ChartMeetingType.Midweek && summary.MeetingDate.DayOfWeek != DayOfWeek.Saturday &&
                 summary.MeetingDate.DayOfWeek != DayOfWeek.Sunday))
            {
                return Math.Abs(summary.Overtime.TotalMinutes) < 30;
            }

            return false;
        }

        private double LimitOvertime(double totalMinutes)
        {
            if (totalMinutes >= 0)
            {
                return totalMinutes > LargestDeviationMins ? LargestDeviationMins : totalMinutes;
            }

            return totalMinutes < -LargestDeviationMins ? -LargestDeviationMins : totalMinutes;
        }

        private void DrawCounselItem(XGraphics g, TimeSpan counselDuration)
        {
            _currentY -= 2 * (double)_itemFont.Height / 5;

            string title = Resources.COUNSEL;
            var sz = g.MeasureString(title, _itemFont);

            var x = _leftIndent + (2 * _itemTitleFont.Height);

            g.DrawString(title, _itemFont, _grayBrush, new XPoint(x, _currentY));

            counselDuration = NormaliseCounselDuration(counselDuration);
            DrawItemOvertime(g, x + sz.Width, counselDuration, TimeSpan.FromSeconds(60));

            _currentY += (3 * (double)_itemTitleFont.Height) / 2;
        }

        private TimeSpan NormaliseCounselDuration(TimeSpan counselDuration)
        {
            TimeSpan result = counselDuration - TimeSpan.FromSeconds(20);
            if (result.TotalSeconds <= 5)
            {
                result = counselDuration;
            }

            while (((int)result.TotalSeconds % 5) != 0)
            {
                result = result.Add(TimeSpan.FromMilliseconds(500));
            }

            return result;
        }

        private void DrawSummary(XGraphics g, TimeSpan plannedEnd, TimeSpan actualEnd)
        {
            _currentY += 2 * _itemTitleFont.Height;

            XPen linePen = new XPen(XColors.Gray);
            g.DrawLine(linePen, _leftMargin, _currentY, _rightX, _currentY);

            _currentY += _subTitleFont.Height;

            g.DrawString(Resources.OVERALL_STATUS, _subTitleFont, _blackBrush, new XPoint(_leftMargin, _currentY));
            _currentY += _subTitleFont.Height;

            var minsOvertime = (actualEnd - plannedEnd).TotalMinutes;
            if (minsOvertime > 1)
            {
                int mins = (int)Math.Round(minsOvertime);

                string msg = mins == 1
                   ? Resources.OVERTIME_BY_1
                   : string.Format(Resources.OVERTIME_BY, mins);

                g.DrawString(msg, _itemTitleFont, _redBrush, new XPoint(_leftMargin, _currentY));
            }
            else if (minsOvertime < -1)
            {
                int mins = Math.Abs((int)Math.Round(minsOvertime));
                string msg = mins == 1
                   ? Resources.UNDERTIME_BY_1
                   : string.Format(Resources.UNDERTIME_BY, mins);

                g.DrawString(msg, _itemTitleFont, _greenBrush, new XPoint(_leftMargin, _currentY));
            }
            else
            {
                g.DrawString(Resources.ON_TIME, _itemTitleFont, _greenBrush, new XPoint(_leftMargin, _currentY));
            }

            _currentY += (double)_itemFont.Height / 2;
            g.DrawLine(linePen, _leftMargin, _currentY, _rightX, _currentY);
        }

        private void DrawItem(XGraphics g, MeetingTimedItem item)
        {
            var curX = _leftIndent;
            var desc = item.Description;

            var textBrush = _blackBrush;

            if (item.IsSongSegment)
            {
                var itemHeight = _itemTitleFont.Height;

                g.DrawRectangle(
                    _lightGrayBrush, 
                    _leftMargin, 
                    _currentY - itemHeight, 
                    _rightX - _leftMargin, 
                    _itemTitleFont.Height * 2.5);
            }

            // title
            g.DrawString(desc, _itemTitleFont, textBrush, new XPoint(curX, _currentY));
            _currentY += _itemTitleFont.Height;

            // duration
            var duration = item.End - item.Start;
            var szDur = DrawDurationString(g, duration, curX, textBrush);
            curX += szDur.Width;

            // start/end times
            curX = DrawTimesString(g, item, curX, textBrush);
            
            var ts = item.AdaptedDuration == default(TimeSpan) ? item.PlannedDuration : item.AdaptedDuration;
            DrawItemOvertime(g, curX, duration, ts);

            _currentY += (3 * (double)_itemTitleFont.Height) / 2;
        }

        private double DrawTimesString(XGraphics g, MeetingTimedItem item, double curX, XBrush textBrush)
        {
            var timesStr1 = $"  ({item.Start.Hours:D2}:{item.Start.Minutes:D2}";
            var timesStr2 = $":{item.Start.Seconds:D2}";
            var hyphenStr = " - ";

            var sz1 = g.MeasureString(timesStr1, _itemFont);
            var sz2 = g.MeasureString(timesStr2, _itemFont);

            g.DrawString(timesStr1, _stdTimeFont, textBrush, new XPoint(curX, _currentY));
            g.DrawString(timesStr2, _smallTimeFont, textBrush, new XPoint(curX += sz1.Width, _currentY));
            g.DrawString(hyphenStr, _stdTimeFont, textBrush, new XPoint(curX += sz2.Width, _currentY));

            var timesStr3 = $"{item.End.Hours:D2}:{item.End.Minutes:D2}";
            var timesStr4 = $":{item.End.Seconds:D2}";

            // note that MeasureString ignores trailing spaces
            // so we must explicitly calculate the size of
            // a space character.
            var szSpace = g.MeasureString(" ", _stdTimeFont);
            var szHyphen = g.MeasureString(hyphenStr, _stdTimeFont);
            var widthHyphenStr = szHyphen.Width + szSpace.Width;

            var sz3 = g.MeasureString(timesStr3, _stdTimeFont);
            var sz4 = g.MeasureString(timesStr4, _smallTimeFont);
            
            g.DrawString(timesStr3, _stdTimeFont, textBrush, new XPoint(curX += widthHyphenStr, _currentY));
            g.DrawString(timesStr4, _smallTimeFont, textBrush, new XPoint(curX += sz3.Width, _currentY));

            g.DrawString(")", _stdTimeFont, textBrush, new XPoint(curX += sz4.Width, _currentY));

            if (item.AdaptedDuration != item.PlannedDuration)
            {
                var adaptedTimeStr = $"{item.AdaptedDuration.Hours:D2}:{item.AdaptedDuration.Minutes:D2}:{item.AdaptedDuration.Seconds:D2}";

                // "adapted duration = {0}"
                var adaptedStr = $" {string.Format(Resources.ADAPTED_DURATION, adaptedTimeStr)}";
                var sz = g.MeasureString(adaptedStr, _smallTimeFont);

                g.DrawString(adaptedStr, _smallTimeFont, _lightGrayBrush, new XPoint(curX += widthHyphenStr, _currentY));

                curX += sz.Width;
            }

            return curX;
        }

        private XSize DrawDurationString(XGraphics g, TimeSpan duration, double curX, XBrush textBrush)
        {
            string durStr = $"{(int)duration.TotalMinutes:D2}:{duration.Seconds:D2}";
            var szDur = g.MeasureString(durStr, _durationFont);
            g.DrawString(durStr, _durationFont, textBrush, new XPoint(curX, _currentY));
            return szDur;
        }

        private void DrawItemOvertime(XGraphics g, double curX, TimeSpan duration, TimeSpan allowedDuration)
        {
            var overtime = allowedDuration - duration;
            var inTheRed = (int)overtime.TotalSeconds < 0;
            var inTheGreen = (int)overtime.TotalSeconds >= 0;
            var spotOn = (int)overtime.TotalSeconds == 0;

            var prefix = string.Empty;

            if (!spotOn)
            {
                prefix = inTheGreen ? "-" : "+";
            }

            var s = $"{prefix} {Math.Abs(overtime.Minutes):D2}:{Math.Abs(overtime.Seconds):D2}";

            var sz = g.MeasureString(s, _durationFont);

            var startX = _rightXIndent - sz.Width;

            var dotsStartX = curX + _stdTimeFont.Height;
            var dotsLength = startX - _stdTimeFont.Height - dotsStartX;

            var sb = new StringBuilder(".");
            var dotsSz = g.MeasureString(sb.ToString(), _smallTimeFont);
            while (dotsSz.Width < dotsLength)
            {
                sb.Append(".");
                dotsSz = g.MeasureString(sb.ToString(), _smallTimeFont);
            }

            dotsStartX = startX - _stdTimeFont.Height - dotsSz.Width;
            g.DrawString(sb.ToString(), _smallTimeFont, XBrushes.LightGray, new XPoint(dotsStartX, _currentY));

            var brush = inTheGreen
                ? _greenBrush
                : inTheRed
                    ? _redBrush
                    : _blackBrush;

            g.DrawString(s, _durationFont, brush, new XPoint(startX, _currentY));
        }

        private void CalcMetrics(PdfPage page)
        {
            var indentDelta = (double)_titleFont.Height / 3;

            _leftMargin = 2 * _titleFont.Height;
            _leftIndent = _leftMargin + indentDelta;
            _topMargin = 2 * _titleFont.Height;
            _currentY = _topMargin;
            _rightX = page.Width - _leftMargin;
            _rightXIndent = _rightX - indentDelta;
        }

        private void DrawTitle(XGraphics g, string title)
        {
            g.DrawString(title, _titleFont, _blackBrush, new XPoint(_leftMargin, _currentY));

            _currentY += (5 * (double)_titleFont.Height) / 6;
            g.DrawString(_data.MeetingDate.ToLongDateString(), _subTitleFont, _blackBrush, new XPoint(_leftMargin, _currentY));

            _currentY += _titleFont.Height;
        }

        private void CreateFonts()
        {
            _titleFont = new XFont("Verdana", 22, XFontStyle.Bold, XPdfFontOptions.UnicodeDefault);
            _subTitleFont = new XFont("Verdana", 16, XFontStyle.Bold, XPdfFontOptions.UnicodeDefault);
            _itemTitleFont = new XFont("Verdana", 11, XFontStyle.Bold, XPdfFontOptions.UnicodeDefault);
            _itemFont = new XFont("Verdana", 10, XFontStyle.Regular, XPdfFontOptions.UnicodeDefault);
            _stdTimeFont = new XFont("Verdana", 10, XFontStyle.Regular, XPdfFontOptions.UnicodeDefault);
            _durationFont = new XFont("Verdana", 10, XFontStyle.Bold, XPdfFontOptions.UnicodeDefault);
            _smallTimeFont = new XFont("Verdana", 8.5, XFontStyle.Regular, XPdfFontOptions.UnicodeDefault);
        }

        private string GetOutputFileName()
        {
            DateTime today = DateTime.Today;
            var result = Path.Combine(_outputFolder, $"{today.Year}-{today.Month:D2}-{today.Day:D2}.pdf");

            int counter = 1;
            while (File.Exists(result))
            {
                result = Path.Combine(_outputFolder, $"{today.Year}-{today.Month:D2}-{today.Day:D2} ({counter}).pdf");
                ++counter;
            }

            return result;
        }
    }
}
