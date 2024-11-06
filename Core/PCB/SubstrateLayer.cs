using Clipper2Lib;
using GerberParser.Abstracts.PCB;
using GerberParser.Core.OBJECT;
using GerberParser.Core.Svg;
using GerberParser.Property.PCB;

namespace GerberParser.Core.PCB;

public class SubstrateLayer : Layer
{
    public Paths64 Shape { get; }
    public Paths64 Dielectric { get; init; }
    public Paths64 Plating { get; }

    public SubstrateLayer(string name, Paths64 shape, Paths64 dielectric,
        Paths64 plating, double thickness) : base(name, thickness)
    {
        Shape = shape;
        Dielectric = dielectric;
        Plating = plating;
    }

    public override Paths64 GetMask()
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
