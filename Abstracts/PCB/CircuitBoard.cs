using Clipper2Lib;
using GerberParser.Abstracts.NcDrill;
using GerberParser.Core.NETLIST;
using GerberParser.Core.PCB;
using GerberParser.Property.Net;
using System.Text;

namespace GerberParser.Abstracts.PCB;

public abstract class CircuitBoard
{
    public Paths64 Board_outLine { get; }

    public Paths64 Board_Shape {  get; }

    public Paths64 Board_shape_excl_pth { get; }

    public Paths64 Substrate_dielectric { get; }

    public Paths64 Substrate_plating { get; }

    public Paths64 Bottom_finish { get; }

    public Paths64 Top_finish { get; }

    public List<Property.Drill.Via> Vias { get; }

    public long plating_thickness { get; }

    public List<Layer> Layers { get; }

    public ulong Num_substrate_layers { get; }


    protected CircuitBoard(string outline, List<string> drill, string drill_nonplated,
        string mill, double plating_thickness = 0.5 * COPPER_OZ.Value){ }


    public abstract StringReader Read_File(string buffer);

    public abstract Paths64 Read_Gerber(string fname, bool outline = false);

    public abstract void Read_Drill(string fname, bool plated, Paths64 pth, Paths64 npth);

    public abstract CircuitBoard LoadPCB(Dictionary<string, List<string>> files);

    public abstract void Add_Mask_Layer(string mask, string silk);

    public abstract void Add_Copper_Layer(string gerber, double thickness = COPPER_OZ.Value);

    public abstract void Add_Substrate_Layer(double thickness = 1.5);

    public abstract void Add_surface_finish();

    public abstract void Generate_mtl_file(StringBuilder stream);

    public abstract NetlistBuilder Get_netlist_builder();

    public abstract PhysicalNetlist Get_physical_netlist();

    public abstract Rect64 Get_Bounds();

    public abstract string Get_svg(bool flipped, ColorScheme colors, string id_prefix = "");

    public abstract void Write_Svg(StringBuilder stream, bool flipped, double scale, ColorScheme? colors = null);

    public abstract void Write_Obj(StringBuilder stream, Netlist netlist = null);
}
