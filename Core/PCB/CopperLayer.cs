using Clipper2Lib;
using GerberParser.Abstracts.PCB;
using GerberParser.Core.OBJECT;
using GerberParser.Core.Svg;
using GerberParser.Property.PCB;
using GerberParser.Core.ClipperPath;
namespace GerberParser.Core.PCB;

public class CopperLayer : Layer
{
    public Paths64 Layer { get; }
    public Paths64 Copper { get; }
    public Paths64 CopperExclPth { get; }

    public CopperLayer(string name, Paths64 board_shape, Paths64 board_shape_excl_pth,
        Paths64 copper_layer, double thickness) : base(name, thickness) {
        Layer = copper_layer;
        Copper = board_shape.Intersect(copper_layer);
        CopperExclPth = board_shape_excl_pth.Intersect(copper_layer);
    }

    public override Paths64 GetMask()
        => Layer;

    public override LayerSvg ToSvg(ColorScheme colors, bool flipped, string idPrefix)
    {
        var layer = new LayerSvg(idPrefix + Name);
        layer.Add(Layer, colors.copper);
        return layer;
    }

    public override void ToObj(ObjFile obj, int layerIndex, double z, string idPrefix) {
    
    }
}
