using Clipper2Lib;
using GerberParser.Abstracts.Aperture;
using GerberParser.Abstracts.Coord;
using GerberParser.Core.PlotCore;

namespace GerberParser.Core.Aperture;

public class Polygon : Standard
{
    private double Diameter { get; set; }
    private int NVertices { get; set; }
    private double Rotation { get; set; }
    private double HoleDiameter { get; set; }

    public Polygon(List<string> csep, FormatBase fmt)
    {
        if (csep.Count < 3 || csep.Count > 5)
        {
            throw new ArgumentException("Invalid polygon aperture");
        }

        Diameter = fmt.ParseFloat(csep[1]);
        NVertices = int.Parse(csep[2]);

        if (NVertices < 3)
        {
            throw new ArgumentException("Invalid polygon aperture");
        }

        Rotation = csep.Count > 3 ? double.Parse(csep[3]) * Math.PI / 180.0 : 0.0;
        HoleDiameter = csep.Count > 4 ? fmt.ParseFloat(csep[4]) : 0;

        Paths64 paths = new Paths64();
        Path64 polygonPath = new Path64();

        for (int i = 0; i < NVertices; i++)
        {
            double angle = ((double)i / NVertices) * 2.0 * Math.PI + Rotation;
            long x = (long)Math.Round(Diameter * 0.5 * Math.Cos(angle));
            long y = (long)Math.Round(Diameter * 0.5 * Math.Sin(angle));
            polygonPath.Add(new Point64(x, y));
        }

        paths.Add(polygonPath); 

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
