using ClipperLib;
using GerberParser.Abstracts.NetList;
using GerberParser.Core.ClipperPath;
using GerberParser.Core.Coord;
using GerberParser.Property.Net;

namespace GerberParser.Core.NETLIST;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;

public class NetlistBuilder : NetlistBuilderBase
{
    public override Netlist Build(long clearance)
    {

        var nl = new Netlist();

        nl.numLayers = 0;
        foreach (var paths in Layers)
        {
            nl.connectedNetlist.RegisterPaths(paths, nl.numLayers);
            var extendedPaths = paths.Offset((double)clearance / 2, false, new ConcreteFormat().BuildClipperOffset());
            nl.clearanceNetlist.RegisterPaths(extendedPaths, nl.numLayers);
            nl.numLayers++;
        }

        foreach (var via in Vias)
        {
            if (!nl.connectedNetlist.RegisterVia(via, nl.numLayers))
            {
                nl.builderViolations.Add($"via at coordinate ({FormatHelper.ToMM(via.GetCoordinate().X)}, " +
                    $"{FormatHelper.ToMM(via.GetCoordinate().Y)}) is not connected to copper on one or more layers");
            }
            nl.clearanceNetlist.RegisterVia(via, nl.numLayers);
        }

        nl.logicalNets = new Dictionary<string, LogicalNet>(LogicalNets);

        foreach (var connection in ConnectionPoints)
        {
            var coord = connection.coordinate;
            var layer = connection.GetLayer(nl.numLayers);
            var logicalNet = connection.LogicalNet;
            var connectedNet = nl.connectedNetlist.FindNet(coord, layer);

            if (connectedNet == null)
            {
                nl.builderViolations.Add($"connection at coordinate ({FormatHelper.ToMM(connection.coordinate.X)}, " +
                    $"{FormatHelper.ToMM(connection.coordinate.Y)}) on layer {connection.GetLayer(nl.numLayers)} should be connected to logical net " +
                    $"{logicalNet.name}, but there is no copper here");
                continue;
            }

            var clearanceNet = nl.clearanceNetlist.FindNet(coord, layer);
            if (clearanceNet == null)
            {
                throw new InvalidOperationException("point maps to connected netlist but not to clearance netlist");
            }

            connectedNet.AssignLogical(logicalNet);
            clearanceNet.AssignLogical(logicalNet);
            logicalNet.AssignPhysical(connectedNet, clearanceNet);
        }

        return nl;
    }

    public override NetlistBuilderBase Layer(Polygons paths)
    {
        Layers.Add(paths);
        return this;
    }

    public override NetlistBuilderBase Net(IntPoint point, int layer, string net_name)
    {
        if (!LogicalNets.TryGetValue(net_name, out var logicalNet))
        {
            logicalNet = new LogicalNet(net_name);
            LogicalNets[net_name] = logicalNet;
        }
        ConnectionPoints.Add(new ConnectionPoint(point, layer, logicalNet));
        return this;
    }

    public override NetlistBuilderBase Via(Polygon path, long finished_hole_size, long plating_thickness, int lower_layer = 0, int upper_layer = -1)
    {
        Vias.Add(new Via(path, finished_hole_size, plating_thickness, lower_layer, upper_layer));
        return this;
    }
}
