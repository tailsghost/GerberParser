using ClipperLib;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace GerberParser.Core.ClipperPath;

public static class ClipperPath
{
    public static Polygons Render(this Polygons paths, double thickness, bool square, ClipperOffset co)
    {
        JoinType joinType = square ? JoinType.jtMiter : JoinType.jtRound;
        EndType endType = square ? EndType.etOpenButt : EndType.etOpenRound;

        co.AddPaths(paths, joinType, endType);

        Polygons outPaths = new Polygons();

        co.Execute(ref outPaths,thickness * 0.5);

        return outPaths;
    }

    public static void Append(this Polygons dest, Polygons src)
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

        dest = Clipper.SimplifyPolygons(dest); 
    }

    private static Polygons PathOp(this Polygons lhs, Polygons rhs, ClipType op)
    {
        var clipper = new Clipper();

        clipper.AddPaths(lhs, PolyType.ptSubject, true);
        clipper.AddPaths(rhs, PolyType.ptClip, true);

        Polygons solutionClosed = new Polygons();

        clipper.Execute(op, solutionClosed);

        return solutionClosed; 
    }

    public static Polygons Add(this Polygons lhs, Polygons rhs)
    {
        return PathOp(lhs, rhs, ClipType.ctUnion);
    }

    public static Polygons Subtract(this Polygons lhs, Polygons rhs)
    {
        return PathOp(lhs, rhs, ClipType.ctDifference);
    }

    public static Polygons Intersect(this Polygons lhs, Polygons rhs)
    {
        return PathOp(lhs, rhs, ClipType.ctIntersection);
    }

    public static Polygons Offset(this Polygons src, double amount, bool square, ClipperOffset co)
    {
        JoinType joinType = square ? JoinType.jtMiter : JoinType.jtRound;

        co.AddPaths(src, square ? JoinType.jtMiter : JoinType.jtRound, EndType.etClosedLine);

        Polygons result = new Polygons();

        if (amount < 0)
        {
            co.Execute(ref result, -amount);
            return Subtract(src, result);
        }
        else
        {
            co.Execute(ref result, amount);
            return Add(src, result);
        }
    }
}
