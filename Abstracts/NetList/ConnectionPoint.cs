using Clipper2Lib;

namespace GerberParser.Abstracts.NetList;

public abstract class ConnectionPoint
{
    private Point64 coordinate {  get; }

    private int layer {  get; }

    private LogicalNet LogicalNet { get; }
}
