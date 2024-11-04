using Clipper2Lib;
using GerberParser.Abstracts.PCB;
using GerberParser.Helpers;

namespace GerberParser.Property.Net;

public class Via
{
    public Path64 path { get; }

    public long finishedHoleSize { get; }

    public long platingThickness { get; }

    public int lowerLayer { get; }

    public int upperLayer { get; }

    public Via(
        Path64 path,
        long finishedHoleSize,
        long platingThickness,
        int lowerLayer = 0,
        int upperLayer = -1)
    {
        this.path = path;
        this.finishedHoleSize = finishedHoleSize;
        this.platingThickness = platingThickness;
        this.lowerLayer = lowerLayer;
        this.upperLayer = upperLayer;
    }

    public Point64 GetCoordinate()
    {
        return path[0];
    }

    public long GetSubstrateHoleSize()
    {
        return finishedHoleSize + 2 * platingThickness;
    }

    public int GetLowerLayer(int numLayers)
    {
        return Utility.ResolveLayerIndex(lowerLayer, numLayers);
    }

    public int GetUpperLayer(int numLayers)
    {
        return Utility.ResolveLayerIndex(upperLayer, numLayers);
    }
}
