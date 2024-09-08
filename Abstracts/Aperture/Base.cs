using GerberParser.Abstracts.PLOT;

namespace GerberParser.Abstracts.APERTURE;

public abstract class Base
{
    protected Plot plot {  get; set; }
    public abstract bool IsSimpleCircle(out long? diameter);
}
