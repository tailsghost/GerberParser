﻿using Clipper2Lib;
using GerberParser.Abstracts.Aperture;
using GerberParser.Abstracts.APERTURE;
using GerberParser.Abstracts.Coord;
using GerberParser.Abstracts.PLOT;
using GerberParser.Core.ClipperPath;
using Path = GerberParser.Core.ClipperPath.Path;

namespace GerberParser.Core.Aperture;

public class Obround : Base
{
    private double XSize { get; set; }
    private double YSize { get; set; }
    private double HoleDiameter { get; set; }

    public Obround(List<string> csep, FormatBase fmt)
    {
        if (csep.Count < 3 || csep.Count > 4)
        {
            throw new ArgumentException("Invalid obround aperture");
        }

        XSize = Math.Abs(fmt.ParseFloat(csep[1]));
        YSize = Math.Abs(fmt.ParseFloat(csep[2]));
        HoleDiameter = csep.Count > 3 ? fmt.ParseFloat(csep[3]) : 0;

        long x = (long)(XSize / 2);
        long y = (long)(YSize / 2);
        long r = Math.Min(x, y);  
        x -= r;
        y -= r;

        var paths = Path.Render(new Paths64
        {
            new Path64 { new Point64(-x, -y), new Point64(x, y) }
        }, r * 2.0, false, fmt.BuildClipperOffset());

        if (HoleDiameter > 0)
        {
            var hole = GetHole(fmt);
            paths.AddRange(hole);
        }

        plot = new Plot(paths);  
    }

    public override bool IsSimpleCircle(out double diameter)
    {
        diameter = 0;
        return false;
    }

    private Paths64 GetHole(FormatBase fmt)
    {
        if (HoleDiameter <= 0)
        {
            return new Paths64();
        }

        var holePaths = Path.Render(new Paths64 { new Path64 { new Point64(0, 0) } }, HoleDiameter, false, fmt.BuildClipperOffset());
        holePaths.Reverse();  
        return holePaths;
    }
}
