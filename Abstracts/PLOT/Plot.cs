using Clipper2Lib;

namespace GerberParser.Abstracts.PLOT;

public abstract class Plot
{
    protected List<Path64> accumPaths;
    protected bool accumPolarity;
    protected List<Path64> dark;
    protected List<Path64> clear;
    protected bool simplified;

    protected Plot(List<Path64> dark = null, List<Path64> clear = null)
    {
        this.dark = dark ?? new List<Path64>();
        this.clear = clear ?? new List<Path64>();
        accumPaths = new List<Path64>();
        simplified = false;
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
        Plot plot,
        bool polarity = true,
        double translateX = 0,
        double translateY = 0,
        bool mirrorX = false,
        bool mirrorY = false,
        double rotate = 0.0,
        double scale = 1.0
    );

    public abstract List<Path64> GetDark();

    public abstract List<Path64> GetClear();
}

