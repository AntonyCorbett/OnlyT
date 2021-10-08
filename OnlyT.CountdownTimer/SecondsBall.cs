namespace OnlyT.CountdownTimer
{
    using System;
    using System.Windows;

    internal static class SecondsBall
    {
        public static Point GetPos(Point centrePt, int seconds, double ballRadius, int innerCircleRadius)
        {
            var ballPathRadius = innerCircleRadius - (3 * ballRadius / 2);
            var angle = ((double)360 * seconds) / 60;

            var startX = centrePt.X + (Math.Sin(angle * Math.PI / 180) * ballPathRadius);
            var startY = centrePt.Y - (Math.Cos(angle * Math.PI / 180) * ballPathRadius);

            return new Point(startX - ballRadius, startY - ballRadius);
        }
    }
}
