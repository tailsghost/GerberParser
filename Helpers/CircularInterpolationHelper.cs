using Clipper2Lib;

namespace GerberParser.Helpers;

public class CircularInterpolationHelper
{
    private double centerX, centerY;
    private double r1, r2;
    private double a1, a2;

    private void ToPolar(double x, double y, out double r, out double a)
    {
        x -= centerX;
        y -= centerY;
        r = Math.Sqrt(x * x + y * y);
        a = Math.Atan2(y, x);
    }

    public CircularInterpolationHelper(
        Point64 start,
        Point64 end,
        Point64 center,
        bool ccw,
        bool multi)
    {
        centerX = center.X;
        centerY = center.Y;
        ToPolar(start.X, start.Y, out r1, out a1);
        ToPolar(end.X, end.Y, out r2, out a2);

        if (multi)
        {
            if (ccw)
            {
                if (a2 <= a1) a2 += 2.0 * Math.PI;
            }
            else
            {
                if (a1 <= a2) a1 += 2.0 * Math.PI;
            }
        }
        else
        {
            if (ccw)
            {
                if (a2 < a1) a2 += 2.0 * Math.PI;
            }
            else
            {
                if (a1 < a2) a1 += 2.0 * Math.PI;
            }
        }
    }

    public bool IsSingleQuadrant()
    {
        return Math.Abs(a1 - a2) <= Math.PI / 2 + 1e-3;
    }

    public double Error()
    {
        return Math.Max(r1, r2);
    }

    public Path64 ToPath(double epsilon)
    {
        double r = (r1 + r2) * 0.5;
        double x = (r > epsilon) ? (1.0 - epsilon / r) : 0.0;
        double th = Math.Acos(2.0 * x * x - 1.0) + 1e-3;
        int nVertices = (int)Math.Ceiling(Math.Abs(a2 - a1) / th);
        Path64 p = new Path64();

        for (int i = 0; i <= nVertices; i++)
        {
            double f2 = (double)i / nVertices;
            double f1 = 1.0 - f2;
            double vr = f1 * r1 + f2 * r2;
            double va = f1 * a1 + f2 * a2;
            double vx = centerX + vr * Math.Cos(va);
            double vy = centerY + vr * Math.Sin(va);
            p.Add(new Point64((int)Math.Round(vx), (int)Math.Round(vy)));
        }

        return p;
    }
}
