using ClipperLib;
using GerberParser.Abstracts.Aperture;
using GerberParser.Core.ClipperPath;
using GerberParser.Core.Coord;
using GerberParser.Core.PlotCore;



namespace GerberParser.Core.Aperture;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using PolygonClip = System.Collections.Generic.List<ClipperLib.IntPoint>;

public class Circle : Standard
{
    private long Diameter { get; set; }
    private double HoleDiameter { get; set; }

    public Circle(List<string> csep, ConcreteFormat fmt)
    {
        if (csep.Count < 2 || csep.Count > 3)
        {
            throw new ArgumentException("Invalid circle aperture");
        }

        Diameter = fmt.ParseFloat(csep[1]);
        HoleDiameter = csep.Count > 2 ? fmt.ParseFloat(csep[2]) : 0;

        var paths = new Polygons { new PolygonClip { new IntPoint(0, 0) } }
                        .Render(Diameter, false, fmt.BuildClipperOffset());

        if (HoleDiameter > 0)
        {
            var hole = GetHole(fmt);
            paths.AddRange(hole);
        }

        Plot = new Plot(paths);
    }

    public override bool IsSimpleCircle(out long? diameter)
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
