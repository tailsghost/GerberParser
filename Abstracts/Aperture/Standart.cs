using Clipper2Lib;
using GerberParser.Abstracts.APERTURE;

namespace GerberParser.Abstracts.Aperture;

public abstract class Standard : Base
{
    protected long holeDiameter;

    protected abstract List<Path64> GetHole();
}
