using GerberParser.Abstracts.APERTURE;
using GerberParser.Abstracts.PLOT;

namespace GerberParser.Abstracts.Aperture;

public abstract class Custom : Base
{
    public Custom(Plot data)
    {
        plot = data;
    }

    public abstract override bool IsSimpleCircle(out long? diameter);
}
