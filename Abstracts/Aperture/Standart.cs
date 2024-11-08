using GerberParser.Abstracts.APERTURE;
using GerberParser.Core.ClipperPath;
using GerberParser.Core.Coord;
using ClipperLib;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;

namespace GerberParser.Abstracts.Aperture;

public abstract class Standard : Base
{
    protected double HoleDiameter;

    protected Polygons GetHole(ConcreteFormat format)
    {

        if (HoleDiameter <= 0.0)
        {
            return [];
        }

        var holePath = new Polygon
        {
            new IntPoint(0, 0)
        };

        var paths = new Polygons { holePath }.Render(HoleDiameter, 
            false, format.BuildClipperOffset());

        Clipper.ReversePaths(paths);

        return paths;
    }
}
