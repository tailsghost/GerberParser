using Clipper2Lib;
using GerberParser.Abstracts.PLOT;

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

        if (polarity != accumPolarity) CommitPaths();
        accumPolarity = polarity;

        accumPaths.AddRange(paths);
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

        for (int i = accumPaths.Count - paths.Count; i < accumPaths.Count; i++)
        {
            var path = accumPaths[i];
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
            for (int i = accumPaths.Count - paths.Count; i < accumPaths.Count; i++)
            {
                accumPaths[i].Reverse();
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
        return clear;
    }

    public override Paths64 GetDark()
    {
        CommitPaths();
        Simplify();
        return dark;
    }

    protected override void CommitPaths(FillRule fillRule = FillRule.NonZero)
    {
        if (accumPaths.Count == 0) return;

        double epsilon = 0.001;

        accumPaths = Clipper.SimplifyPaths(accumPaths, epsilon, true);
        var cld = new Clipper64();
        var clc = new Clipper64();

        cld.AddSubject(dark);
        clc.AddSubject(clear);

        cld.AddClip(accumPaths);
        clc.AddClip(accumPaths);

        Paths64 darkSolutionClosed = new Paths64();
        Paths64 darkSolutionOpen = new Paths64();
        Paths64 clearSolutionClosed = new Paths64();
        Paths64 clearSolutionOpen = new Paths64();

        if (accumPolarity)
        {
            cld.Execute(ClipType.Union, fillRule, darkSolutionClosed, darkSolutionOpen);
            clc.Execute(ClipType.Difference, fillRule, clearSolutionClosed, clearSolutionOpen);
        }
        else
        {
            cld.Execute(ClipType.Difference, fillRule, darkSolutionClosed, darkSolutionOpen);
            clc.Execute(ClipType.Union, fillRule, clearSolutionClosed, clearSolutionOpen);
        }

        simplified = false;
        accumPaths.Clear();
    }

    protected override void Simplify()
    {
        if (simplified) return;

        double epsilon = 0.001;

        //Необходимо использовать в дальнейшем Execute NonZero

        dark = Clipper.SimplifyPaths(dark, epsilon, true);
        clear = Clipper.SimplifyPaths(clear, epsilon, true);

        simplified = true;
    }
}
