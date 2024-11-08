using GerberParser.Core.OBJECT;
using GerberParser.Core.Svg;
using GerberParser.Property.PCB;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;

namespace GerberParser.Abstracts.PCB;

public abstract class Layer
{
    public string Name { get; }
    public double Thickness { get; }

    protected Layer(string name, double thickness)
    {
        Name = name;
        Thickness = thickness;
    }

    public abstract Polygons GetMask();

    public abstract LayerSvg ToSvg(ColorScheme colors, bool flipped, string idPrefix);

    public abstract void ToObj(ObjFile obj, int layerIndex, double z, string idPrefix);
}
