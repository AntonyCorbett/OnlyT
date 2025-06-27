﻿namespace OnlyT.CountdownTimer;

using System;
using System.Windows;
using System.Windows.Media;

internal static class PieSlice
{
    public static PathGeometry Get(double angle, Point centrePt, int innerRadius, int outerRadius)
    {
        var segments = new PathSegmentCollection
        {
            CreateLine1(angle, centrePt, outerRadius),
            CreateArc1(angle, centrePt, outerRadius),
            CreateLine2(angle, centrePt, innerRadius),
            CreateArc2(angle, centrePt, innerRadius)
        };

        var start = GetStartPoint(angle, centrePt, innerRadius);
        var pf = new PathFigure(start, segments, true);

        return new PathGeometry([pf], FillRule.EvenOdd, null);
    }

    private static Point GetStartPoint(double angle, Point centrePt, int innerRadius)
    {
        var radians = angle * (Math.PI / 180);
        var startX = centrePt.X + (Math.Sin(radians) * innerRadius);
        var startY = centrePt.Y - (Math.Cos(radians) * innerRadius);

        return new Point(startX, startY);
    }

    private static ArcSegment CreateArc1(double angle, Point centrePt, int outerRadius)
    {
        var endX = centrePt.X;
        var endY = centrePt.Y - outerRadius;
        var endPt = new Point(endX, endY);
        var sz = new Size
        {
            Height = outerRadius,
            Width = outerRadius
        };

        var largeArc = angle < 180;
        return new ArcSegment(endPt, sz, 360 - angle, largeArc, SweepDirection.Clockwise, true);
    }

    private static ArcSegment CreateArc2(double angle, Point centrePt, int innerRadius)
    {
        var endPt = GetStartPoint(angle, centrePt, innerRadius);
        var sz = new Size
        {
            Height = innerRadius,
            Width = innerRadius
        };

        var largeArc = angle < 180;
        return new ArcSegment(endPt, sz, 360 - angle, largeArc, SweepDirection.Counterclockwise, true);
    }

    private static LineSegment CreateLine1(double angle, Point centrePt, int outerRadius)
    {
        var radians = angle * (Math.PI / 180);
        var endX = centrePt.X + (Math.Sin(radians) * outerRadius);
        var endY = centrePt.Y - (Math.Cos(radians) * outerRadius);
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