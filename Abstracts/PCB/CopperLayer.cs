using Clipper2Lib;
using GerberParser.Abstracts.Object;
using GerberParser.Abstracts.SVG;
using GerberParser.Core.PCB;

namespace GerberParser.Abstracts.PCB;

public abstract class CopperLayer : Layer
{
    public Paths64 Layer { get; }
    public Paths64 Copper { get; }
    public Paths64 Copper_excl_pth { get; }

    protected CopperLayer(string name, Paths64 board_shape, Paths64 board_shape_excl_pth,
        Paths64 copper_layer, double thickness) : base(name, thickness) { }

    public abstract override Paths64 GetMask();

    public abstract override LayerSvg ToSvg(ColorScheme colors, bool flipped, string idPrefix);

    public abstract override void ToObj(ObjFile obj, int layerIndex, double z, string idPrefix);
}
