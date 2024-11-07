using Clipper2Lib;
using GerberParser.Abstracts.PLOT;
using GerberParser.Core.ClipperPath;

namespace GerberParser.Core.PlotCore;

public class Plot : PlotBase
{

    public Plot(Paths64 dark = null, Paths64 clear = null)
        : base(dark, clear)
    {   
    }

    public override void DrawPaths(List<Path64> paths, bool polarity = true)
    {
        if (paths.Count == 0) return;

        if (polarity != AccumPolarity) CommitPaths();
        AccumPolarity = polarity;

        AccumPaths.AddRange(paths);
    }

    public override void DrawPaths(List<Path64> paths, bool polarity, double translateX, double translateY = 0, bool mirrorX = false, bool mirrorY = false, double rotate = 0, double scale = 1, bool specialFillType = false, FillRule fillRule = FillRule.NonZero)
    {
        if (paths.Count == 0) return;

        if (specialFillType) CommitPaths();

        DrawPaths(paths, polarity);

        double ixx = mirrorX ? -scale : scale;
        double iyy = mirrorY ? -scale : scale;
        double sinRot = Math.Sin(rotate);
        double cosRot = Math.Cos(rotate);

        double xx = ixx * cosRot;
        double xy = ixx * sinRot;
        double yx = iyy * -sinRot;
        double yy = iyy * cosRot;

        for (int i = AccumPaths.Count - paths.Count; i < AccumPaths.Count; i++)
        {
            var path = AccumPaths[i];
            for (int j = 0; j < path.Count; j++)
            {
                var c = path[j];
                long cx = (long)(Math.Round(c.X * xx + c.Y * yx) + translateX);
                long cy = (long)(Math.Round(c.X * xy + c.Y * yy) + translateY);
                path[j] = new Point64(cx, cy);
            }
        }

        if (mirrorX != mirrorY)
        {
            for (int i = AccumPaths.Count - paths.Count; i < AccumPaths.Count; i++)
            {
                AccumPaths[i].Reverse();
            }
        }

        if (specialFillType) CommitPaths(fillRule);
    }

    public override void DrawPlot(PlotBase plot, bool polarity = true, double translateX = 0, double translateY = 0, bool mirrorX = false, bool mirrorY = false, double rotate = 0, double scale = 1)
    {
        DrawPaths(plot.GetDark(), polarity, translateX, translateY, mirrorX, mirrorY, rotate, scale);
        DrawPaths(plot.GetClear(), !polarity, translateX, translateY, mirrorX, mirrorY, rotate, scale);
    }

    public override List<Path64> GetClear()
    {
        CommitPaths();
        Simplify();
        return Clear;
    }

    public override Paths64 GetDark()
    {
        CommitPaths();
        Simplify();
        return Dark;
    }

    protected override void CommitPaths(FillRule fillRule = FillRule.NonZero)
    {
        if (AccumPaths.Count == 0) return;

        AccumPaths = AccumPaths.SimplifyPolygons(fillRule);
        var cld = new Clipper64();
        var clc = new Clipper64();

        cld.AddOpenSubject(Dark);
        clc.AddOpenSubject(Clear);

        cld.AddClip(AccumPaths);
        clc.AddClip(AccumPaths);

        Paths64 darkSolutionClosed = new Paths64();
        Paths64 darkSolutionOpen = new Paths64();
        Paths64 clearSolutionClosed = new Paths64();
        Paths64 clearSolutionOpen = new Paths64();

        if (AccumPolarity)
        {
            cld.Execute(ClipType.Union, fillRule, darkSolutionClosed, darkSolutionOpen);
            clc.Execute(ClipType.Difference, fillRule, clearSolutionClosed, clearSolutionOpen);
        }
        else
        {
            cld.Execute(ClipType.Difference, fillRule, darkSolutionClosed, darkSolutionOpen);
            clc.Execute(ClipType.Union, fillRule, clearSolutionClosed, clearSolutionOpen);
        }

        Simplified = false;
        Dark = new Paths64(darkSolutionClosed.Distinct());
        Clear = new Paths64(clearSolutionClosed.Distinct());
        AccumPaths.Clear();
    }

    protected override void Simplify()
    {
        if (Simplified) return;

        Dark = Dark.SimplifyPolygons(FillRule.NonZero);
        Clear = Clear.SimplifyPolygons(FillRule.NonZero);

        Simplified = true;
    }
}
