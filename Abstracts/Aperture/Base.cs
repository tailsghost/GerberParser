using GerberParser.Abstracts.PLOT;

namespace GerberParser.Abstracts.APERTURE;

public abstract class Base
{
    public Plot plot {  get; protected set; }
    public abstract bool IsSimpleCircle(out double diameter);
}
