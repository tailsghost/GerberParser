using Clipper2Lib;
using GerberParser.Constants;
using GerberParser.Core.Coord;
using GerberParser.Core.NETLIST;
using GerberParser.Property.Net;
using GerberParser.Property.PCB;
using System.Text;

namespace GerberParser.Abstracts.PCB;

public abstract class CircuitBoardBase
{
    public Paths64 BoardOutLine { get; } = new();

    public Paths64 BoardShape { get; } = new();

    public Paths64 BoardShapeExclPth { get; } = new();

    public Paths64 SubstrateDielectric { get; } = new();

    public Paths64 SubstratePlating { get; } = new();

    public Paths64 BottomFinish { get; set; } = new();

    public Paths64 TopFinish { get; set; } = new();

    public List<Property.Drill.Via> Vias { get; } = new();

    public long PlatingThickness { get; }

    public List<Layer> Layers { get; } = new();

    public ulong NumSubstrateLayers { get; set; }


    protected CircuitBoardBase(string outline, List<string> drill, string drill_nonplated,
        string mill, double plating_thickness = 0.5 * COPPER_OZ.Value)
    {
        ConcreteFormat format = new ConcreteFormat();
        PlatingThickness = format.FromMM(0.5 * COPPER_OZ.Value);
        BoardOutLine = Read_Gerber(outline, true);
        Paths64 pth = new(), npth = new();
        BoardOutLine.AddRange(Read_Gerber("", true));

        foreach (var drillFile in drill)
        {
            Read_Drill(drillFile, true, pth, npth);
            if (string.IsNullOrEmpty(drill_nonplated))
            {
                Read_Drill(drillFile, false, pth, npth);
            }
        }

        var holes = Core.ClipperPath.Path.Add(pth, npth);
        BoardShape = Core.ClipperPath.Path.Subtract(BoardOutLine, holes);
        BoardShapeExclPth = Core.ClipperPath.Path.Subtract(BoardOutLine, npth);

        var pthDrill = Core.ClipperPath.Path.Offset(pth, plating_thickness, true);
        SubstrateDielectric = Core.ClipperPath.Path.Subtract(BoardOutLine, Core.ClipperPath.Path.Add(pthDrill, npth));
        SubstratePlating = Core.ClipperPath.Path.Subtract(pthDrill, pth);
    }

    protected void GenerateMaterial(StringBuilder sb, string type, string color, float transparency)
    {
        sb.AppendLine($"newmtl {type}");
        sb.AppendLine($"Kd {color}");
        sb.AppendLine($"d {transparency}");
        sb.AppendLine();
    }


    public abstract StringReader Read_File(string buffer);

    public abstract Paths64 Read_Gerber(string fname, bool outline = false);

    public abstract void Read_Drill(string fname, bool plated, Paths64 pth, Paths64 npth);

    public abstract void GenerateMtlFile(StringBuilder sb);

    public abstract void Add_Mask_Layer(string mask, string silk);

    public abstract void Add_Copper_Layer(string gerber, double thickness = COPPER_OZ.Value);

    public abstract void Add_Substrate_Layer(double thickness = 1.5);

    public abstract void Add_surface_finish();

    public abstract void Generate_mtl_file(StringBuilder stream);

    public abstract NetlistBuilder Get_netlist_builder();

    public abstract PhysicalNetlist Get_physical_netlist();

    public abstract Rect64 Get_Bounds();

    public abstract string Get_svg(bool flipped, ColorScheme colors, StringBuilder sb, string id_prefix = "");

    public abstract void Write_Svg(StringBuilder stream, bool flipped, double scale, ColorScheme? colors = null);

    public abstract void Write_Obj(StringWriter stream, Netlist netlist = null);
}
