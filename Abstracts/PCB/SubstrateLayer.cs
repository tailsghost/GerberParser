using Clipper2Lib;
using GerberParser.Abstracts.Object;
using GerberParser.Abstracts.SVG;
using GerberParser.Core.PCB;

namespace GerberParser.Abstracts.PCB;

public abstract class SubstrateLayer : Layer
{
    public Paths64 Shape { get;}
    public Paths64 Dielectric { get; }
    public Paths64 Plating { get;}

    protected SubstrateLayer(string name, Paths64 shape, Paths64 dielectric,
        Paths64 plating, double thickness) : base(name, thickness) { }

    public abstract override Paths64 GetMask();

    public abstract override LayerSvg ToSvg(ColorScheme colors, bool flipped, string idPrefix);

    public abstract override void ToObj(ObjFile obj, int layerIndex, double z, string idPrefix);
}
