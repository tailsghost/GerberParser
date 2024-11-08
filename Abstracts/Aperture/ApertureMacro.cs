using GerberParser.Abstracts.APERTURE;
using GerberParser.Core.Coord;

namespace GerberParser.Abstracts.Aperture;

public abstract class ApertureMacroBase
{
    protected List<Expression> Cmd = [];

    public abstract void Append(string cmd);

    public abstract Base Build(List<string> csep, ConcreteFormat fmt);
}
