using Clipper2Lib;
using GerberParser.Abstracts.PCB;
using GerberParser.Core.OBJECT;
using GerberParser.Core.Svg;
using GerberParser.Property.PCB;

namespace GerberParser.Core.PCB;

public class MaskLayer : Layer
{
    public Paths64 Mask { get; } = new();

    public Paths64 Silk { get; } = new();

    public bool Bottom { get; }

    public MaskLayer(string name, Paths64 board_outline, Paths64 mask_layer,
        Paths64 silk_layer, bool bottom) : base(name, 0.01) 
    {

        Bottom = bottom;
        Mask = ClipperPath.Path.Subtract(board_outline, mask_layer);
        Silk = ClipperPath.Path.Intersect(Mask, silk_layer);
    }

    public override Paths64 GetMask()
        => Mask;

    public  override LayerSvg ToSvg(ColorScheme colors, bool flipped, string idPrefix)
    {
        var layer = new LayerSvg(idPrefix + Name);
        if (Bottom == flipped)
        {
            layer.Add(Mask, colors.soldermask);
            layer.Add(Silk, colors.silkscreen);
        }
        else
        {
            layer.Add(Silk, colors.silkscreen);
            layer.Add(Mask, colors.soldermask);
        }
        return layer;
    }

    public override void ToObj(ObjFile obj, int layerIndex, double z, string idPrefix)
    {
        double maskZ1, maskZ2, silkZ;
        string maskName, silkName;

        if (Bottom)
        {
            maskName = "_GBS";
            silkName = "_GBO";
            silkZ = z;
        }
        else
        {
            maskName = "_GTS";
            silkName = "_GTO";
            silkZ = z + Thickness;
        }

        maskZ1 = z + Thickness * 0.01;
        maskZ2 = z + Thickness * 0.99;

        obj.AddObject($"layer{layerIndex}{maskName}", "soldermask")
           .AddSheet(Mask, maskZ1, maskZ2);

        obj.AddObject($"layer{layerIndex}{silkName}", "silkscreen")
           .AddSurface(Silk, silkZ);
    }
}

