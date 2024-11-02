using GerberParser.Abstracts.APERTURE;
using GerberParser.Abstracts.Coord;

namespace GerberParser.Abstracts.Aperture;

public abstract class ApertureMacroBase
{
    protected List<Expression> cmd = new();

    public abstract void Append(string cmd);

    public abstract Base Build(List<string> csep, FormatBase fmt);
}
