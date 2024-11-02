using Clipper2Lib;
using GerberParser.Abstracts.Aperture;
using GerberParser.Abstracts.APERTURE;
using GerberParser.Abstracts.Coord;
using GerberParser.Core.PlotCore;

namespace GerberParser.Core.Aperture;

public class Rectangle : Standard
{
    private double XSize { get; set; }
    private double YSize { get; set; }
    private double HoleDiameter { get; set; }

    public Rectangle(List<string> csep, FormatBase fmt)
    {
        if (csep.Count < 3 || csep.Count > 4)
        {
            throw new ArgumentException("Invalid rectangle aperture");
        }

        XSize = Math.Abs(fmt.ParseFloat(csep[1]));
        YSize = Math.Abs(fmt.ParseFloat(csep[2]));
        HoleDiameter = csep.Count > 3 ? fmt.ParseFloat(csep[3]) : 0;

        Path64 rectangle = new Path64
        {
            new Point64(XSize / 2, YSize / 2),
            new Point64(XSize / 2, -YSize / 2),
            new Point64(-XSize / 2, -YSize / 2),
            new Point64(-XSize / 2, YSize / 2)
        };
        var paths = new Paths64 { rectangle };

        if (HoleDiameter > 0)
        {
            var hole = GetHole(fmt);
            paths.AddRange(hole);
        }

        plot = new Plot(paths);
    }

    public override bool IsSimpleCircle(out long? diameter)
    {
        diameter = 0;
        return false;
    }
}
