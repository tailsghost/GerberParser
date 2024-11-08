using GerberParser.Abstracts.PCB;
using GerberParser.Core.OBJECT;
using GerberParser.Core.Svg;
using GerberParser.Property.PCB;

namespace GerberParser.Core.PCB;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;

public class SubstrateLayer : Layer
{
    public Polygons Shape { get; }
    public Polygons Dielectric { get; init; }
    public Polygons Plating { get; }

    public SubstrateLayer(string name, Polygons shape, Polygons dielectric,
        Polygons plating, double thickness) : base(name, thickness)
    {
        Shape = shape;
        Dielectric = dielectric;
        Plating = plating;
    }

    public override Polygons GetMask()
        => Dielectric;

    public override LayerSvg ToSvg(ColorScheme colors, bool flipped, string idPrefix)
    {
        var layer = new LayerSvg(idPrefix + Name);
        layer.Add(Dielectric, colors.substrate);
        layer.Add(Plating, colors.finish);
        return layer;
    }

    public override void ToObj(ObjFile obj, int layerIndex, double z, string idPrefix)
    {
        obj.AddObject($"layer{layerIndex}_{Name}", "substrate")
               .AddSheet(Dielectric, z, z + Thickness);
    }
}
