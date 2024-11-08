using GerberParser.Abstracts.PLOT;


using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using ClipperLib;

namespace GerberParser.Core.PlotCore;

public class Plot : PlotBase
{

    public Plot(Polygons dark = null, Polygons clear = null)
        : base(dark, clear)
    {   
    }

    public override void DrawPaths(Polygons paths, bool polarity = true)
    {
        if (paths.Count == 0) return;

        if (polarity != AccumPolarity) CommitPaths();
        AccumPolarity = polarity;

        AccumPaths.AddRange(paths);
    }

    public override void DrawPaths(Polygons paths, bool polarity, double translateX, double translateY = 0,
        bool mirrorX = false, bool mirrorY = false, double rotate = 0, double scale = 1,
        bool specialFillType = false, PolyFillType fillType = PolyFillType.pftNonZero)
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
                path[j] = new IntPoint(cx, cy);
            }
        }

        if (mirrorX != mirrorY)
        {
            for (int i = AccumPaths.Count - paths.Count; i < AccumPaths.Count; i++)
            {
                AccumPaths[i].Reverse();
            }
        }

        if (specialFillType) CommitPaths(fillType);
    }

    public override void DrawPlot(PlotBase plot, bool polarity = true, double translateX = 0, double translateY = 0, bool mirrorX = false, bool mirrorY = false, double rotate = 0, double scale = 1)
    {
        DrawPaths(plot.GetDark(), polarity, translateX, translateY, mirrorX, mirrorY, rotate, scale);
        DrawPaths(plot.GetClear(), !polarity, translateX, translateY, mirrorX, mirrorY, rotate, scale);
    }

    public override Polygons GetClear()
    {
        CommitPaths();
        Simplify();
        return Clear;
    }

    public override Polygons GetDark()
    {
        CommitPaths();
        Simplify();
        return Dark;
    }

    protected override void CommitPaths(PolyFillType fillType = PolyFillType.pftNonZero)
    {
        if (AccumPaths.Count == 0) return;

        AccumPaths = Clipper.SimplifyPolygons(AccumPaths);
        var cld = new Clipper();
        var clc = new Clipper();

        cld.AddPaths(Dark, PolyType.ptSubject, true);
        clc.AddPaths(Clear, PolyType.ptSubject, true);

        cld.AddPaths(AccumPaths, PolyType.ptClip, true);
        clc.AddPaths(AccumPaths, PolyType.ptClip, true);

        if (AccumPolarity)
        {
            cld.Execute(ClipType.ctUnion, Dark, PolyFillType.pftNonZero, fillType);
            clc.Execute(ClipType.ctDifference, Clear, PolyFillType.pftNonZero, fillType);
        }
        else
        {
            cld.Execute(ClipType.ctDifference, Dark, PolyFillType.pftNonZero, fillType);
            clc.Execute(ClipType.ctUnion, Clear, PolyFillType.pftNonZero, fillType);
        }

        Simplified = false;
        AccumPaths.Clear();
    }

    protected override void Simplify()
    {
        if (Simplified) return;

        Dark = Clipper.SimplifyPolygons(Dark, PolyFillType.pftNonZero);
        Clear = Clipper.SimplifyPolygons(Clear, PolyFillType.pftNonZero);
        Simplified = true;
    }
}
