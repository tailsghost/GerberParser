using ClipperLib;
using GerberParser.Helpers;

namespace GerberParser.Property.Net;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;

public class Via
{
    public Polygon path { get; }

    public long finishedHoleSize { get; }

    public long platingThickness { get; }

    public int lowerLayer { get; }

    public int upperLayer { get; }

    public Via(
        Polygon path,
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

    public IntPoint GetCoordinate()
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
