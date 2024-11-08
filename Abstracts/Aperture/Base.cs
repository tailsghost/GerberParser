using GerberParser.Core.PlotCore;

namespace GerberParser.Abstracts.APERTURE;

public abstract class Base
{
    public Plot Plot { get; protected set; } = new();
    public abstract bool IsSimpleCircle(out double diameter);
}
