using Clipper2Lib;
using GerberParser.Abstracts.APERTURE;
using GerberParser.Abstracts.Coord;
using GerberParser.Core.ClipperPath;
using GerberParser.Core.Coord;

namespace GerberParser.Abstracts.Aperture;

public abstract class Standard : Base
{
    protected long? HoleDiameter;

    protected List<Path64> GetHole(ConcreteFormat format)
    {

        if (HoleDiameter <= 0.0)
        {
            return new Paths64(); 
        }

        var holePath = new Path64
        {
            new Point64(0, 0)
        };

        var paths = new Paths64 { holePath }.Render((double)HoleDiameter, 
            false, format.BuildClipperOffset());

        Clipper.ReversePaths(paths);

        return paths;
    }
}
