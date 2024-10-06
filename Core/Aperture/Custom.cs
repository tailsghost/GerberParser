using GerberParser.Abstracts.APERTURE;
using GerberParser.Abstracts.PLOT;

namespace GerberParser.Core.Aperture;

public class Custom : Base
{
    public Custom(Plot data)
    {
        plot = data;
    }


    public override bool IsSimpleCircle(out double diameter)
    {
        diameter = 0;
        return false;
    }
}
