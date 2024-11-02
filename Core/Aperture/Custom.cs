using GerberParser.Abstracts.APERTURE;
using GerberParser.Abstracts.PLOT;
using GerberParser.Core.PlotCore;

namespace GerberParser.Core.Aperture;

public class Custom : Base
{
    public Custom(Plot data)
    {
        plot = data;
    }

    public override bool IsSimpleCircle(out long? diameter)
    {
        diameter = 0;
        return false;
    }
}
