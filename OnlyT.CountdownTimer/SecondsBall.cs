namespace OnlyT.CountdownTimer;

using System;
using System.Windows;

internal static class SecondsBall
{
    public static Point GetPos(Point centrePt, int seconds, double ballRadius, int innerCircleRadius)
    {
        var ballPathRadius = innerCircleRadius - (3 * ballRadius / 2);
        var angle = 6.0 * seconds;
        var radians = angle * (Math.PI / 180);

        var startX = centrePt.X + (Math.Sin(radians) * ballPathRadius);
        var startY = centrePt.Y - (Math.Cos(radians) * ballPathRadius);

        return new Point(startX - ballRadius, startY - ballRadius);
    }
}