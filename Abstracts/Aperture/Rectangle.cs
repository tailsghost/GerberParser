using GerberParser.Abstracts.Coord;

namespace GerberParser.Abstracts.Aperture;

public abstract class Rectangle : Standard
{
    protected long xSize;
    protected long ySize;

    public Rectangle(List<string> csep, Format fmt)
    {
    }

    public abstract override bool IsSimpleCircle(out long? diameter);
}
