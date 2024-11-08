using GerberParser.Abstracts.PCB;
using GerberParser.Constants;
using GerberParser.Core.ClipperPath;
using GerberParser.Core.Coord;
using GerberParser.Core.GERBER;
using GerberParser.Core.NCDRILL;
using GerberParser.Core.NETLIST;
using GerberParser.Core.OBJECT;
using GerberParser.Core.Svg;
using GerberParser.Property.Net;
using GerberParser.Property.PCB;
using System.Text;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using ClipperLib;

namespace GerberParser.Core.PCB;

public class CircuitBoard : CircuitBoardBase
{
    public CircuitBoard(string outline, List<string> drill, string drill_nonplated, string mill, double plating_thickness = 0.0174)
    {
        PlatingThickness = FormatHelper.FromMM(0.5 * COPPER_OZ.Value);
        BoardOutLine = Read_Gerber(outline, true);
        Polygons pth = [], npth = [];
        BoardOutLine.AddRange(Read_Gerber("", true));

        foreach (var drillFile in drill)
        {
            Read_Drill(drillFile, true, ref pth, ref npth);
            if (string.IsNullOrEmpty(drill_nonplated))
            {
                Read_Drill(drillFile, false, ref pth, ref npth);
            }
        }

        Polygons holes = [.. pth, .. npth];
        BoardShape = BoardOutLine.Subtract(holes);
        BoardShapeExclPth = BoardOutLine.Subtract(npth);

        //Проверить Format.BuildClipperOffset()
        var pthDrill = pth.Offset(FormatHelper.FromMM(plating_thickness), true, Format.BuildClipperOffset());
        SubstrateDielectric = BoardOutLine.Subtract(pthDrill.Add(npth));
        SubstratePlating = pthDrill.Subtract(pth);
    }

    public override void Add_Copper_Layer(string gerber, double thickness = 0.0348)
    {
        Layers.Add(new CopperLayer(
            $"copper{++NumSubstrateLayers}", BoardShape, BoardShapeExclPth, Read_Gerber(gerber), thickness
        ));
    }

    public override void Add_Mask_Layer(string mask, string silk)
    {
        Layers.Add(new MaskLayer(
            $"mask{++NumSubstrateLayers}", BoardOutLine, Read_Gerber(mask), Read_Gerber(silk), Layers.Count == 0
        ));
    }

    public override void Add_Substrate_Layer(double thickness = 1.5)
    {
        Layers.Add(new SubstrateLayer(
            $"substrate{++NumSubstrateLayers}", BoardShape, SubstrateDielectric, SubstratePlating, thickness
        ));
    }

    public override void Add_surface_finish()
    {
        Polygons mask = [];

        foreach (var layer in Layers)
        {
            if (layer is CopperLayer copperLayer)
            {
                BottomFinish = copperLayer.GetMask().Subtract(mask);
                break;
            }
            mask = mask.Add(layer.GetMask());
        }

        mask.Clear();

        for (int i = Layers.Count - 1; i >= 0; i--)
        {
            if (Layers[i] is CopperLayer copperLayer)
            {
                TopFinish = copperLayer.GetMask().Subtract(mask);
                break;
            }
            mask = mask.Add(Layers[i].GetMask());
        }
    }

    public override void Generate_mtl_file(StringBuilder stream)
    {
        throw new NotImplementedException();
    }

    public override IntRect Get_Bounds()
    {
        var bounds = new IntRect
        {
            left = long.MaxValue,
            bottom = long.MaxValue,
            right = long.MinValue,
            top = long.MinValue
        };

        foreach (var path in BoardOutLine)
        {
            foreach (var point in path)
            {
                bounds.left = Math.Min(bounds.left, point.X);
                bounds.right = Math.Max(bounds.right, point.X);
                bounds.bottom = Math.Min(bounds.bottom, point.Y);
                bounds.top = Math.Max(bounds.top, point.Y);
            }
        }
        return bounds;
    }

    public override NetlistBuilder Get_netlist_builder()
    {
        var nb = new NetlistBuilder();

        foreach (var layer in Layers)
        {
            if (layer is CopperLayer copperLayer)
            {
                nb.Layer(copperLayer.GetMask());
            }
        }

        foreach (var via in Vias)
        {
            nb.Via(via.Path, via.Finished_hole_size, PlatingThickness);
        }

        return nb;
    }

    public override PhysicalNetlist Get_physical_netlist()
    {
        var pn = new PhysicalNetlist();
        int layerIndex = 0;

        foreach (var layer in Layers)
        {
            if (layer is CopperLayer copperLayer)
            {
                pn.RegisterPaths(copperLayer.GetMask(), layerIndex++);
            }
        }

        foreach (var via in Vias)
        {
            pn.RegisterVia(new Property.Net.Via(via.Path, via.Finished_hole_size, PlatingThickness), layerIndex);
        }

        return pn;
    }

    public override string Get_svg(bool flipped, ColorScheme colors, StringBuilder sb, string id_prefix = "")
    {
        if (flipped)
        {
            for (int i = Layers.Count - 1; i >= 0; i--)
            {
                var str = (Layers[i].ToSvg(colors, flipped, id_prefix).ToString());
                sb.Append(str);
            }
        }
        else
        {
            foreach (var layer in Layers)
            {
                var str = layer.ToSvg(colors, flipped, id_prefix).ToString();
                sb.Append(str);
            }
        }

        var finish = new LayerSvg($"{id_prefix}finish");
        finish.Add(flipped ? BottomFinish : TopFinish, colors.finish);
        sb.Append(finish.ToString());

        return sb.ToString();
    }

    public override void Read_Drill(string fname, bool plated, ref Polygons pth, ref Polygons npth)
    {
        if (string.IsNullOrEmpty(fname)) return;

        var f = Read_File(fname);
        var d = new NCDrill(f, plated);
        var l = d.GetPaths(true, false);

        if (pth.Count == 0)
            pth = l;
        else
        {
            pth.AddRange(l);
            pth = Clipper.SimplifyPolygons(pth);
        }

        l = d.GetPaths(false, true);

        if (npth.Count == 0)
            npth = l;
        else
        {
            npth.AddRange(l);
            npth = Clipper.SimplifyPolygons(npth);
        }

        Vias.AddRange(d.GetVias());
    }

    public override StringReader Read_File(string buffer)
    {
        return new StringReader(buffer);
    }

    public override Polygons Read_Gerber(string fname, bool outline = false)
    {
        if (string.IsNullOrEmpty(fname)) return [];

        var f = new StringReader(fname);
        var g = new Gerber(f);

        var result = outline ? g.GetOutlinePaths() : g.GetPaths();

        return result;
    }

    public override void GenerateMtlFile(StringBuilder sb)
    {
        GenerateMaterial(sb, "soldermask", "0.100 0.600 0.300", 0.6f);
        GenerateMaterial(sb, "silkscreen", "0.899 0.899 0.899", 0.899f);
        GenerateMaterial(sb, "finish", "0.699 0.699 0.699", 1.0f);
        GenerateMaterial(sb, "substrate", "0.600 0.500 0.300", 1.0f);
        GenerateMaterial(sb, "copper", "0.800 0.700 0.300", 1.0f);
    }

    public override void Write_Obj(StringWriter stream, Netlist? netlist = null)
    {
        var obj = new ObjFile();
        double z = 0.0;
        int index = 0;
        var copperZ = new List<(double, double)>();
        foreach (var layer in Layers)
        {
            layer.ToObj(obj, index++, z, "");
            if (layer is CopperLayer)
            {
                copperZ.Add((z, z + layer.Thickness));
            }
            z += layer.Thickness;
        }

        if (netlist != null)
        {
            RenderCopper(obj, netlist.connectedNetlist, copperZ);
        }
        else
        {
            RenderCopper(obj, Get_physical_netlist(), copperZ);
        }

        obj.ToFile(stream);
    }

    public override void Write_Svg(StringBuilder stream, bool flipped, double scale, ColorScheme colors)
    {
        var bounds = Get_Bounds();

        var width = bounds.right - bounds.left + FormatHelper.FromMM(20.0);
        var height = bounds.top - bounds.bottom + FormatHelper.FromMM(20.0);

        stream.AppendLine($"<svg viewBox=\"0 0 {FormatHelper.ToMM(width)} {FormatHelper.ToMM(height)}\" " +
        $"width=\"{FormatHelper.ToMM(width) * scale}\" height=\"{FormatHelper.ToMM(height) * scale}\" " +
        $"xmlns=\"http://www.w3.org/2000/svg\">");

        var tx = FormatHelper.FromMM(10.0) - (flipped ? -bounds.right : bounds.left);
        var ty = FormatHelper.FromMM(10.0) + bounds.top;

        stream.AppendLine($"<g transform=\"translate({FormatHelper.ToMM(tx)} {FormatHelper.ToMM(ty)}) " +
                          $"scale({(flipped ? "-1" : "1")} -1)\" filter=\"drop-shadow(0 0 1 rgba(0, 0, 0, 0.2))\">");

        stream.Append(Get_svg(flipped, colors, stream, ""));
        stream.AppendLine("</g>");
        stream.AppendLine("</svg>");
    }



    private void RenderCircle(IntPoint center, long diameter, Polygon output)
    {
        double epsilon = Format.GetMaxDeviation();
        double r = diameter * 0.5;
        double x = (r > epsilon) ? (1.0 - epsilon / r) : 0.0;
        double th = Math.Acos(2.0 * x * x - 1.0) + 1e-3;
        int nVertices = (int)Math.Ceiling(2.0 * Math.PI / th);
        if (nVertices < 3) nVertices = 3;

        output.Clear();
        output.Capacity = nVertices;

        for (int i = 0; i < nVertices; i++)
        {
            double a = 2.0 * Math.PI * i / nVertices;
            long xPos = (long)Math.Round(Math.Cos(a) * r);
            long yPos = (long)Math.Round(Math.Sin(a) * r);
            output.Add(new IntPoint(center.X + xPos, center.Y + yPos));
        }
    }

    private void RenderCopper(ObjFile obj, PhysicalNetlist netlist, List<(double, double)> copperZ)
    {

        int nameCounter = 1;
        foreach (var net in netlist.nets)
        {
            string name = net.logicalNets.Count != 0 ?
                $"{net.logicalNets.First().name}_{nameCounter}" :
                $"net_{nameCounter}";

            nameCounter++;
            var ob = obj.AddObject(name, "copper");

            var vias = new List<Via>
            {
                Capacity = net.vias.Count
            };
            foreach (var via in net.vias)
            {
                var center = via.GetCoordinate();
                var diameter = via.finishedHoleSize;
                int lowerLayer = via.GetLowerLayer(copperZ.Count);
                int upperLayer = via.GetUpperLayer(copperZ.Count);

                var inner = new Polygon();
                RenderCircle(center, diameter, inner);
                ob.AddRing(inner, copperZ[lowerLayer].Item1, copperZ[upperLayer].Item2);

                var outer = new Polygon();
                RenderCircle(center, diameter + 2 * via.platingThickness, outer);
                for (int layer = lowerLayer; layer < upperLayer; layer++)
                {
                    ob.AddRing(outer, copperZ[layer].Item2, copperZ[layer + 1].Item1);
                }

                vias.Add(new Via(center, inner, outer, lowerLayer, upperLayer));
            }

            foreach (var shape in net.shapes)
            {
                var zs = copperZ[shape.layer];

                ob.AddRing(shape.outline, zs.Item1, zs.Item2);
                foreach (var path in shape.holes)
                {
                    ob.AddRing(path, zs.Item1, zs.Item2);
                }

                int layer = shape.layer;
                for (int side = 0; side < 2; side++)
                {
                    double z = side == 1 ? zs.Item2 : zs.Item1;

                    var holes = new Polygons(shape.holes);
                    foreach (var via in vias)
                    {
                        if (layer < via.Lower_layer || layer > via.Upper_layer || !shape.Contains(via.Center))
                            continue;

                        holes.Add((layer == via.Lower_layer && side == 0) || (layer == via.Upper_layer && side == 1)
                            ? via.Inner : via.Outer);
                    }

                    ob.AddSurface(shape.outline, holes, z);
                }
            }
        }
    }

    private readonly struct Via(IntPoint center, Polygon inner, Polygon outer, int lower_layer, int upper_layer)
    {
        public IntPoint Center { get; } = center;
        public Polygon Inner { get; } = inner;
        public Polygon Outer { get; } = outer;
        public int Lower_layer { get; } = lower_layer;
        public int Upper_layer { get; } = upper_layer;
    };

}
