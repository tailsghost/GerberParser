using Clipper2Lib;
using GerberParser.Abstracts.APERTURE;
using GerberParser.Abstracts.Coord;
using GerberParser.Core.Coord;
using Path = GerberParser.Core.ClipperPath.Path;

namespace GerberParser.Abstracts.Aperture;

public abstract class Standard : Base
{
    protected long? HoleDiameter;

    protected List<Path64> GetHole(ConcreteFormat format = null)
    {

        if (HoleDiameter <= 0.0)
        {
            return new Paths64(); 
        }

        var holePath = new Path64
        {
            new Point64(0, 0)
        };

        var paths = Path.Render(new Paths64 { holePath }, (double)HoleDiameter, 
            false, format.BuildClipperOffset());

        Clipper.ReversePaths(paths);

        return paths;
    }
}
