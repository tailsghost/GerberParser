using ClipperLib;
using GerberParser.Abstracts.Aperture;
using GerberParser.Core.ClipperPath;
using GerberParser.Core.Coord;
using GerberParser.Core.PlotCore;

namespace GerberParser.Core.Aperture;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using PolygonClip = System.Collections.Generic.List<ClipperLib.IntPoint>;

public class Obround : Standard
{
    private double XSize { get; set; }
    private double YSize { get; set; }
    private double HoleDiameter { get; set; }

    public Obround(List<string> csep, ConcreteFormat fmt)
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

        var paths = new Polygons
        {
            new PolygonClip { new IntPoint(-x, -y), new IntPoint(x, y) }
        }.Render(r * 2.0, false, fmt.BuildClipperOffset());

        if (HoleDiameter > 0)
        {
            var hole = GetHole(fmt);
            paths.AddRange(hole);
        }

        Plot = new Plot(paths);  
    }

    public override bool IsSimpleCircle(out long? diameter)
    {
        diameter = 0;
        return false;
    }
}
