using Clipper2Lib;
using GerberParser.Abstracts.NcDrill;
using GerberParser.Property.Net;

namespace GerberParser.Abstracts.NetList;

public abstract class NetlistBuilderBase
{
    public List<Paths64> Layers { get; }

    public List<Via> Vias { get; }

    public Dictionary<string, LogicalNet> LogicalNets { get; }

    public List<ConnectionPoint> ConnectionPoints { get; }

    public abstract NetlistBuilderBase Layer(Paths64 paths);

    public abstract NetlistBuilderBase Via(Path64 path, long finished_hole_size, long plating_thickness, int lower_layer = 0,int upper_layer = -1);

    public abstract NetlistBuilderBase Net(Point64 point, int layer, string net_name);

    public abstract Netlist Build(long clearance);
}
