using Clipper2Lib;

namespace GerberParser.Abstracts.PLOT;

public abstract class PlotBase
{
    protected Paths64 AccumPaths = new();
    protected bool AccumPolarity;
    protected Paths64 Dark = new();
    protected Paths64 Clear = new();
    protected bool Simplified;

    protected PlotBase(Paths64 dark = null, Paths64 clear = null)
    {
        Simplified = false;
        if (dark != null)
            Dark = dark;
        if (clear != null)
            Clear = clear;
    }

    protected abstract void CommitPaths(FillRule fillRule = FillRule.NonZero);

    protected abstract void Simplify();

    public abstract void DrawPaths(List<Path64> paths, bool polarity = true);

    public abstract void DrawPaths(
        List<Path64> paths,
        bool polarity,
        double translateX,
        double translateY = 0,
        bool mirrorX = false,
        bool mirrorY = false,
        double rotate = 0.0,
        double scale = 1.0,
        bool specialFillType = false,
        FillRule fillRule = FillRule.NonZero
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

    public abstract Paths64 GetDark();

    public abstract List<Path64> GetClear();
}

