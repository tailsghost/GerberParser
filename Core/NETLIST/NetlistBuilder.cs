using Clipper2Lib;
using GerberParser.Abstracts.NetList;
using GerberParser.Core.Coord;
using GerberParser.Property.Net;

namespace GerberParser.Core.NETLIST;

public class NetlistBuilder : NetlistBuilderBase
{

    ConcreteFormat format = new();

    public override Netlist Build(long clearance)
    {

        var nl = new Netlist();

        nl.numLayers = 0;
        foreach (var paths in Layers)
        {
            nl.connectedNetlist.RegisterPaths(paths, nl.numLayers);
            var extendedPaths = ClipperPath.Path.Offset(paths, (double)clearance / 2, false);
            nl.clearanceNetlist.RegisterPaths(extendedPaths, nl.numLayers);
            nl.numLayers++;
        }

        foreach (var via in Vias)
        {
            if (!nl.connectedNetlist.RegisterVia(via, nl.numLayers))
            {
                nl.builderViolations.Add($"via at coordinate ({format.ToMM(via.GetCoordinate().X)}, {format.ToMM(via.GetCoordinate().Y)}) is not connected to copper on one or more layers");
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
                nl.builderViolations.Add($"connection at coordinate ({format.ToMM(connection.coordinate.X)}, " +
                    $"{format.ToMM(connection.coordinate.Y)}) on layer {connection.GetLayer(nl.numLayers)} should be connected to logical net " +
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

    public override NetlistBuilderBase Layer(Paths64 paths)
    {
        Layers.Add(paths);
        return this;
    }

    public override NetlistBuilderBase Net(Point64 point, int layer, string net_name)
    {
        if (!LogicalNets.TryGetValue(net_name, out var logicalNet))
        {
            logicalNet = new LogicalNet(net_name);
            LogicalNets[net_name] = logicalNet;
        }
        ConnectionPoints.Add(new ConnectionPoint(point, layer, logicalNet));
        return this;
    }

    public override NetlistBuilderBase Via(Path64 path, long finished_hole_size, long plating_thickness, int lower_layer = 0, int upper_layer = -1)
    {
        Vias.Add(new Via(path, finished_hole_size, plating_thickness, lower_layer, upper_layer));
        return this;
    }
}
