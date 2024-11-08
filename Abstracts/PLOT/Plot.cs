

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using ClipperLib;

namespace GerberParser.Abstracts.PLOT;

public abstract class PlotBase
{
    protected Polygons AccumPaths = new();
    protected bool AccumPolarity;
    protected Polygons Dark = new();
    protected Polygons Clear = new();
    protected bool Simplified;

    protected PlotBase(Polygons dark = null, Polygons clear = null)
    {
        Simplified = false;
        if (dark != null)
            Dark = dark;
        if (clear != null)
            Clear = clear;
    }

    protected abstract void CommitPaths(PolyFillType fillRule = PolyFillType.pftNonZero);

    protected abstract void Simplify();

    public abstract void DrawPaths(List<Polygon> paths, bool polarity = true);

    public abstract void DrawPaths(
        Polygons paths,
        bool polarity,
        double translateX,
        double translateY = 0,
        bool mirrorX = false,
        bool mirrorY = false,
        double rotate = 0.0,
        double scale = 1.0,
        bool specialFillType = false,
        PolyFillType fillRule = PolyFillType.pftNonZero
    );

    public abstract void DrawPlot(
        PlotBase plot,
        bool polarity = true,
        double translateX = 0,
        double translateY = 0,
        bool mirrorX = false,
        bool mirrorY = false,
        double rotate = 0.0,
        double scale = 1.0
    );

    public abstract Polygons GetDark();

    public abstract Polygons GetClear();
}

