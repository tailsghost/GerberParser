using GerberParser.Abstracts.APERTURE;
using GerberParser.Core.PlotCore;

namespace GerberParser.Core.Aperture;

public class Custom : Base
{
    public Custom(Plot data)
    {
        Plot = data;
    }

    public override bool IsSimpleCircle(out double diameter)
    {
        diameter = 0;
        return false;
    }
}
