using Clipper2Lib;
using GerberParser.Abstracts.Object;
using GerberParser.Abstracts.SVG;
using GerberParser.Core.PCB;

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

    public abstract Paths64 GetMask();

    public abstract LayerSvg ToSvg(ColorScheme colors, bool flipped, string idPrefix);

    public abstract void ToObj(ObjFile obj, int layerIndex, double z, string idPrefix);
}
