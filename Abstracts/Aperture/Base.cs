using GerberParser.Core.PlotCore;

namespace GerberParser.Abstracts.APERTURE;

public abstract class Base
{
    public Plot plot {  get; protected set; }
    public abstract bool IsSimpleCircle(out long? diameter);
}
