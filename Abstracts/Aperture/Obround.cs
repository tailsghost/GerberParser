using GerberParser.Abstracts.Coord;

namespace GerberParser.Abstracts.Aperture;

public abstract class Obround : Standard
{
    protected long xSize;
    protected long ySize;

    public Obround(List<string> csep, Format fmt)
    {
    }

    public abstract override bool IsSimpleCircle(out long? diameter);
}
