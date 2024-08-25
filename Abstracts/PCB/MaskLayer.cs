using Clipper2Lib;
using GerberParser.Abstracts.Object;
using GerberParser.Abstracts.SVG;
using GerberParser.Core.PCB;

namespace GerberParser.Abstracts.PCB;

public abstract class MaskLayer : Layer
{
    public Paths64 Mask { get; }

    public Paths64 Silk { get; }

    public bool Bottom { get; }

    protected MaskLayer(string name, Paths64 board_outline, Paths64 mask_layer,
        Paths64 silk_layer, double thickness, bool bottom) : base(name, thickness) { }

    public abstract override Paths64 GetMask();

    public abstract override LayerSvg ToSvg(ColorScheme colors, bool flipped, string idPrefix);

    public abstract override void ToObj(ObjFile obj, int layerIndex, double z, string idPrefix);
}
