using Clipper2Lib;

namespace GerberParser.Abstracts.NetList;

public abstract class Via
{
    protected Path64 path {  get; set; }

    protected int finishedHoleSize { get; set; }

    protected int platingThickness { get; set; }

    protected int lowerLayer { get; set; }

    protected int upperLayer { get; set; }

    protected Via(
        Path64 path,
        int finishedHoleSize,
        int platingThickness,
        int lowerLayer = 0,
        int upperLayer = -1)
    {
        this.path = path;
        this.finishedHoleSize = finishedHoleSize;
        this.platingThickness = platingThickness;
        this.lowerLayer = lowerLayer;
        this.upperLayer = upperLayer;
    }
}
