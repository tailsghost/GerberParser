using GerberParser.Constants;
using GerberParser.Core.Coord;
using GerberParser.Core.NETLIST;
using GerberParser.Property.Net;
using GerberParser.Property.PCB;
using System.Text;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using ClipperLib;

namespace GerberParser.Abstracts.PCB;

public abstract class CircuitBoardBase
{
    protected ConcreteFormat Format = new();
    public Polygons BoardOutLine { get; protected set; } = new();

    public Polygons BoardShape { get; protected set; } = new();

    public Polygons BoardShapeExclPth { get; protected set; } = new();

    public Polygons SubstrateDielectric { get; protected set; } = new();

    public Polygons SubstratePlating { get; protected set; } = new();

    public Polygons BottomFinish { get; set; } = new();

    public Polygons TopFinish { get; set; } = new();

    public List<Property.Drill.Via> Vias { get; protected set; } = new();

    public long PlatingThickness { get; protected set; }

    public List<Layer> Layers { get; protected set; } = new();

    public ulong NumSubstrateLayers { get; set; }

    protected void GenerateMaterial(StringBuilder sb, string type, string color, float transparency)
    {
        sb.AppendLine($"newmtl {type}");
        sb.AppendLine($"Kd {color}");
        sb.AppendLine($"d {transparency}");
        sb.AppendLine();
    }


    public abstract StringReader Read_File(string buffer);

    public abstract Polygons Read_Gerber(string fname, bool outline = false);

    public abstract void Read_Drill(string fname, bool plated,ref Polygons pth,ref Polygons npth);

    public abstract void GenerateMtlFile(StringBuilder sb);

    public abstract void Add_Mask_Layer(string mask, string silk);

    public abstract void Add_Copper_Layer(string gerber, double thickness = COPPER_OZ.Value);

    public abstract void Add_Substrate_Layer(double thickness = 1.5);

    public abstract void Add_surface_finish();

    public abstract void Generate_mtl_file(StringBuilder stream);

    public abstract NetlistBuilder Get_netlist_builder();

    public abstract PhysicalNetlist Get_physical_netlist();

    public abstract IntRect Get_Bounds();

    public abstract string Get_svg(bool flipped, ColorScheme colors, StringBuilder sb, string id_prefix = "");

    public abstract void Write_Svg(StringBuilder stream, bool flipped, double scale, ColorScheme? colors = null);

    public abstract void Write_Obj(StringWriter stream, Netlist netlist = null);
}
