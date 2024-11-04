using Clipper2Lib;
using GerberParser.Abstracts.PCB;

namespace GerberParser.Property.Net;

public class ConnectionPoint
{
    public Point64 coordinate { get; }

    public int layer { get; }

    public LogicalNet LogicalNet { get; }

    public ConnectionPoint(Point64 coordinate, int layer, LogicalNet net)
    {
        this.layer = layer;
        this.coordinate = coordinate;
        LogicalNet = net;
    }

    public int GetLayer(int numLayers) => ResolveLayerIndex(layer, numLayers);

    private int ResolveLayerIndex(int layer, int numLayers)
    {
        if (layer < 0 || layer >= numLayers)
        {
            throw new ArgumentOutOfRangeException(nameof(layer), $"Layer index {layer} is out of range. It should be between 0 and {numLayers - 1}.");
        }
        return layer;
    }
}
