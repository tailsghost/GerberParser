using GerberParser.Abstracts.PCB;
using GerberParser.Core.OBJECT;
using GerberParser.Core.Svg;
using GerberParser.Property.PCB;
using GerberParser.Core.ClipperPath;
namespace GerberParser.Core.PCB;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;

public class CopperLayer : Layer
{
    public Polygons Layer { get; }
    public Polygons Copper { get; }
    public Polygons CopperExclPth { get; }

    public CopperLayer(string name, Polygons board_shape, Polygons board_shape_excl_pth,
        Polygons copper_layer, double thickness) : base(name, thickness) {
        Layer = copper_layer;
        Copper = board_shape.Intersect(copper_layer);
        CopperExclPth = board_shape_excl_pth.Intersect(copper_layer);
    }

    public override Polygons GetMask()
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
