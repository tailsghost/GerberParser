using GerberParser.Abstracts.Coord;

namespace GerberParser.Abstracts.Aperture;

public abstract class Circle : Standard
{
    protected long diameter;

    public Circle(List<string> csep, Format fmt)
    {
    }

    public abstract override bool IsSimpleCircle(out long? diameterOut);
}
