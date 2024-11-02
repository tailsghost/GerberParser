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
}
