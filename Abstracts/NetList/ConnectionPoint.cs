using Clipper2Lib;

namespace GerberParser.Abstracts.NetList;

public abstract class ConnectionPoint
{
    private Point64 coordinate {  get; }

    private int layer {  get; }

    private LogicalNet LogicalNet { get; }

    protected ConnectionPoint(Point64 coordinate, int layer, LogicalNet net)
    {
        this.layer = layer;
        this.coordinate = coordinate;
        this.LogicalNet = net;
    }
}
