using Clipper2Lib;

namespace GerberParser.Core.ClipperPath;

public static class Path
{
    public static Paths64 Render(Paths64 paths, double thickness, bool square, ClipperOffset co)
    {
        JoinType joinType = square ? JoinType.Miter : JoinType.Round;
        EndType endType = square ? EndType.Butt : EndType.Round;

        co.AddPaths(paths, joinType, endType);

        Paths64 outPaths = new Paths64();

        co.Execute(thickness * 0.5, outPaths);

        return outPaths;
    }

    public static void Append(ref Paths64 dest, Paths64 src)
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

        //Изменить 0.001
        Clipper.SimplifyPaths(dest, 0.001); 
    }

    private static Paths64 PathOp(Paths64 lhs, Paths64 rhs, ClipType op)
    {
        var clipper = new Clipper64();
        clipper.AddSubject(lhs);
        clipper.AddClip(rhs);

        Paths64 solutionClosed = new Paths64();
        Paths64 solutionOpen = new Paths64();

        clipper.Execute(op, FillRule.Positive, solutionClosed, solutionOpen);

        return solutionOpen; 
    }

    public static Paths64 Add(Paths64 lhs, Paths64 rhs)
    {
        return PathOp(lhs, rhs, ClipType.Union);
    }

    public static Paths64 Subtract(Paths64 lhs, Paths64 rhs)
    {
        return PathOp(lhs, rhs, ClipType.Difference);
    }

    public static Paths64 Intersect(Paths64 lhs, Paths64 rhs)
    {
        return PathOp(lhs, rhs, ClipType.Intersection);
    }

    public static Paths64 Offset(Paths64 src, double amount, bool square)
    {

        Clipper64 clipper = new();

        JoinType joinType = square ? JoinType.Miter : JoinType.Round;

        clipper.AddSubject(src);

        Paths64 result = new Paths64();

        if (amount < 0)
        {
            clipper.Execute(ClipType.Difference, FillRule.Positive, result);
        }
        else
        {
            clipper.Execute(ClipType.Union, FillRule.Positive, result);
        }

        return result;
    }
}
