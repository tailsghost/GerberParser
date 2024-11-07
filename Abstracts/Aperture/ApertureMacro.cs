using GerberParser.Abstracts.APERTURE;
using GerberParser.Abstracts.Coord;
using GerberParser.Core.Coord;

namespace GerberParser.Abstracts.Aperture;

public abstract class ApertureMacroBase
{
    protected List<Expression> Cmd = new();

    public abstract void Append(string cmd);

    public abstract Base Build(List<string> csep, ConcreteFormat fmt);
}
