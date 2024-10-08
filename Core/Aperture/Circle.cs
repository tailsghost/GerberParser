﻿using Clipper2Lib;
using GerberParser.Abstracts.Aperture;
using GerberParser.Abstracts.APERTURE;
using GerberParser.Abstracts.Coord;
using GerberParser.Abstracts.PLOT;
using Path = GerberParser.Core.ClipperPath.Path;

namespace GerberParser.Core.Aperture;

public class Circle : Base
{
    private double Diameter { get; set; }
    private double HoleDiameter { get; set; }

    public Circle(List<string> csep, FormatBase fmt)
    {
        if (csep.Count < 2 || csep.Count > 3)
        {
            throw new ArgumentException("Invalid circle aperture");
        }

        Diameter = fmt.ParseFloat(csep[1]);
        HoleDiameter = csep.Count > 2 ? fmt.ParseFloat(csep[2]) : 0;

        var paths = Path.Render(new Paths64 { new Path64 { new Point64(0, 0) } }, Diameter, false, fmt.BuildClipperOffset());

        if (HoleDiameter > 0)
        {
            var hole = new Standard(HoleDiameter).GetHole(fmt);
            paths.AddRange(hole);
        }

        plot = new Plot(paths);
    }

    public override bool IsSimpleCircle(out double diameter)
    {
        if (HoleDiameter > 0.0)
        {
            diameter = 0;
            return false;
        }

        diameter = Diameter;
        return true;
    }
}
