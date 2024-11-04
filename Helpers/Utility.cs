namespace GerberParser.Helpers;

public static class Utility
{
    public static int ResolveLayerIndex(int layer, int numLayers)
    {
        int actualLayer = layer;
        if (actualLayer < 0)
        {
            actualLayer += numLayers;
        }
        if (actualLayer < 0 || actualLayer >= numLayers)
        {
            throw new ArgumentOutOfRangeException(nameof(layer), $"Layer index {layer} is out of range");
        }
        return actualLayer;
    }
}
