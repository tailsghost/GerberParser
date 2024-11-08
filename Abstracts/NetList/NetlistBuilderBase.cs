using ClipperLib;
using GerberParser.Abstracts.NcDrill;
using GerberParser.Property.Net;

namespace GerberParser.Abstracts.NetList;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;

public abstract class NetlistBuilderBase
{
    public List<Polygons> Layers { get; } = [];

    public List<Via> Vias { get; } = [];

    public Dictionary<string, LogicalNet> LogicalNets { get; } = [];

    public List<ConnectionPoint> ConnectionPoints { get; } = [];

    public abstract NetlistBuilderBase Layer(Polygons paths);

    public abstract NetlistBuilderBase Via(Polygon path, long finished_hole_size, long plating_thickness, int lower_layer = 0,int upper_layer = -1);

    public abstract NetlistBuilderBase Net(IntPoint point, int layer, string net_name);

    public abstract Netlist Build(long clearance);
}
