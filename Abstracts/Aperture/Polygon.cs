using GerberParser.Abstracts.Coord;

namespace GerberParser.Abstracts.Aperture;

public abstract class Polygon : Standard
{
    protected long diameter;
    protected int nVertices;
    protected double rotation;

    public Polygon(List<string> csep, FormatBase fmt)
    {
    }

    public abstract override bool IsSimpleCircle(out long? diameterOut);
}
