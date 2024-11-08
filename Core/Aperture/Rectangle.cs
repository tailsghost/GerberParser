using ClipperLib;
using GerberParser.Abstracts.Aperture;
using GerberParser.Core.Coord;
using GerberParser.Core.PlotCore;

namespace GerberParser.Core.Aperture;

using Polygons = List<List<IntPoint>>;
using PolygonClip = List<IntPoint>;

public class Rectangle : Standard
{
    private double XSize { get; set; }
    private double YSize { get; set; }

    public Rectangle(List<string> csep, ConcreteFormat fmt)
    {
        if (csep.Count < 3 || csep.Count > 4)
        {
            throw new ArgumentException("Invalid rectangle aperture");
        }

        XSize = Math.Abs(fmt.ParseFloat(csep[1]));
        YSize = Math.Abs(fmt.ParseFloat(csep[2]));
        HoleDiameter = csep.Count > 3 ? fmt.ParseFloat(csep[3]) : 0;

        var rectangle = new PolygonClip
        {
            new IntPoint(XSize / 2, YSize / 2),
            new IntPoint(XSize / 2, -YSize / 2),
            new IntPoint(-XSize / 2, -YSize / 2),
            new IntPoint(-XSize / 2, YSize / 2)
        };
        var paths = new Polygons { rectangle };

        if (HoleDiameter > 0)
        {
            var hole = GetHole(fmt);
            paths.AddRange(hole);
        }

        Plot = new Plot(paths);
    }

    public override bool IsSimpleCircle(out double diameter)
    {
        diameter = 0;
        return false;
    }
}
