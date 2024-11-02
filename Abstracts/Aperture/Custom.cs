using GerberParser.Abstracts.APERTURE;
using GerberParser.Core.PlotCore;

namespace GerberParser.Abstracts.Aperture;

public abstract class Custom : Base
{
    public Custom(Plot data)
    {
        plot = data;
    }

    public abstract override bool IsSimpleCircle(out long? diameter);
}
