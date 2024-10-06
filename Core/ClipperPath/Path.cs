using Clipper2Lib;
using System.Net;

namespace GerberParser.Core.ClipperPath;

public static class Path
{
    public static Paths64 Render(Paths64 paths, double thickness, bool square, ClipperOffset co)
    {
        // Определяем тип соединения и концов линии
        JoinType joinType = square ? JoinType.Miter : JoinType.Round;
        EndType endType = square ? EndType.Butt : EndType.Round;

        // Добавляем пути в ClipperOffset
        co.AddPaths(paths, joinType, endType);

        // Выходной путь
        Paths64 outPaths = new Paths64();

        // Выполняем рендеринг с заданной толщиной
        co.Execute(thickness * 0.5, outPaths);

        return outPaths;
    }
}
