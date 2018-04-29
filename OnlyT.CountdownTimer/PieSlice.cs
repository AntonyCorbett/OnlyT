namespace OnlyT.CountdownTimer
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    internal static class PieSlice
    {
        public static Geometry Get(double angle, Point centrePt, int innerRadius, int outerRadius)
        {
            PathSegmentCollection segs = new PathSegmentCollection();

            segs.Add(CreateLine1(angle, centrePt, outerRadius));
            segs.Add(CreateArc1(angle, centrePt, outerRadius));
            segs.Add(CreateLine2(angle, centrePt, innerRadius));
            segs.Add(CreateArc2(angle, centrePt, innerRadius));

            Point start = GetStartPoint(angle, centrePt, innerRadius);
            PathFigure pf = new PathFigure(start, segs, true);

            return new PathGeometry(new[] { pf }, FillRule.EvenOdd, null);
        }

        private static Point GetStartPoint(double angle, Point centrePt, int innerRadius)
        {
            var startX = centrePt.X + (Math.Sin(angle * Math.PI / 180) * innerRadius);
            var startY = centrePt.Y - (Math.Cos(angle * Math.PI / 180) * innerRadius);

            return new Point(startX, startY);
        }

        private static ArcSegment CreateArc1(double angle, Point centrePt, int outerRadius)
        {
            var endX = centrePt.X;
            var endY = centrePt.Y - outerRadius;
            Point endPt = new Point(endX, endY);
            Size sz = new Size
            {
                Height = outerRadius,
                Width = outerRadius
            };

            bool largeArc = angle < 180;
            return new ArcSegment(endPt, sz, 360 - angle, largeArc, SweepDirection.Clockwise, true);
        }

        private static ArcSegment CreateArc2(double angle, Point centrePt, int innerRadius)
        {
            Point endPt = GetStartPoint(angle, centrePt, innerRadius);
            Size sz = new Size
            {
                Height = innerRadius,
                Width = innerRadius
            };

            bool largeArc = angle < 180;
            return new ArcSegment(endPt, sz, 360 - angle, largeArc, SweepDirection.Counterclockwise, true);
        }

        private static LineSegment CreateLine1(double angle, Point centrePt, int outerRadius)
        {
            var endX = centrePt.X + (Math.Sin(angle * Math.PI / 180) * outerRadius);
            var endY = centrePt.Y - (Math.Cos(angle * Math.PI / 180) * outerRadius);
            var ls = new LineSegment(new Point(endX, endY), true) { IsStroked = angle > 0 };
            return ls;
        }

        private static LineSegment CreateLine2(double angle, Point centrePt, int innerRadius)
        {
            var endX = centrePt.X;
            var endY = centrePt.Y - innerRadius;
            var ls = new LineSegment(new Point(endX, endY), true) { IsStroked = angle > 0 };
            return ls;
        }
    }
}
