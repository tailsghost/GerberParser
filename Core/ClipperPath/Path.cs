using Clipper2Lib;

namespace GerberParser.Core.ClipperPath;

public static class ClipperPath
{
    public static Paths64 Render(this Paths64 paths, double thickness, bool square, ClipperOffset co)
    {
        JoinType joinType = square ? JoinType.Miter : JoinType.Round;
        EndType endType = square ? EndType.Butt : EndType.Round;

        co.AddPaths(paths, joinType, endType);

        Paths64 outPaths = new Paths64();

        co.Execute(thickness * 0.5, outPaths);

        return outPaths;
    }

    public static void Append(this Paths64 dest, Paths64 src)
    {
        if (src.Count == 0)
        {
            return;
        }

        if (dest.Count == 0)
        {
            dest = src;
            return;
        }

        dest.AddRange(src);

        dest = dest.SimplifyPolygons(); 
    }

    private static Paths64 PathOp(this Paths64 lhs, Paths64 rhs, ClipType op)
    {
        var clipper = new Clipper64();
        clipper.AddSubject(lhs);
        clipper.AddClip(rhs);

        Paths64 solutionClosed = new Paths64();

        clipper.Execute(op, FillRule.NonZero, solutionClosed);

        return solutionClosed; 
    }

    public static Paths64 Add(this Paths64 lhs, Paths64 rhs)
    {
        return PathOp(lhs, rhs, ClipType.Union);
    }

    public static Paths64 Subtract(this Paths64 lhs, Paths64 rhs)
    {
        return PathOp(lhs, rhs, ClipType.Difference);
    }

    public static Paths64 Intersect(this Paths64 lhs, Paths64 rhs)
    {
        return PathOp(lhs, rhs, ClipType.Intersection);
    }

    public static Paths64 Offset(this Paths64 src, double amount, bool square)
    {

        Clipper64 clipper = new();

        JoinType joinType = square ? JoinType.Miter : JoinType.Round;

        clipper.AddSubject(src);

        Paths64 result = new Paths64();

        if (amount < 0)
        {
            clipper.Execute(ClipType.Difference, FillRule.NonZero, result);
        }
        else
        {
            clipper.Execute(ClipType.Union, FillRule.NonZero, result);
        }

        return result;
    }

    public static Paths64 SimplifyPolygons(this Paths64 paths, FillRule fillRule = FillRule.EvenOdd, double epsilon = 0.001)
    {
        Paths64 simplifiedPaths = Clipper.SimplifyPaths(paths, epsilon, true);

        var clipper = new Clipper64();

        clipper.AddSubject(simplifiedPaths);

        Paths64 resultClosed = new Paths64();
        Paths64 resultOpen = new Paths64();

        clipper.Execute(ClipType.Union, fillRule, resultClosed, resultOpen);

        return resultClosed;
    }
}
