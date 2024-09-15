using Clipper2Lib;
using GerberParser.Abstracts.Aperture;

namespace GerberParser.Abstracts.NetList;

public abstract class Shape
{
    protected Path64 outline;

    protected List<Path64> holes;

    protected Rect64 boundingBox;

    protected int layer;

    protected Shape(Path64 outline, List<Path64> holes, int layer)
    {
        this.outline = outline;
        this.holes = holes;
        this.layer = layer;
        this.boundingBox = CalculateBoundingBox(outline, holes);
    }

    public abstract bool Contains(Point64 point);


    protected abstract Rect64 CalculateBoundingBox(Path64 outline, List<Path64> holes);
}
