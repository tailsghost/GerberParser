using GerberParser.Abstracts.APERTURE;
using GerberParser.Abstracts.Coord;

namespace GerberParser.Abstracts.Aperture;

public abstract class ApertureMacroBase
{
    protected List<Expression> cmds = new();

    public void Append(string cmd)
    {
        
    }

    public abstract Base Build(List<string> csep, FormatBase fmt);
}
