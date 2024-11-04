using Clipper2Lib;
using GerberParser.Abstracts.Coord;
using GerberParser.Core.PlotCore;
using GerberParser.Enums;
using GerberParser.Property;
using GerberParser.Property.Drill;

namespace GerberParser.Abstracts.NcDrill;


public abstract class NCDrillBase
{
    protected FormatBase fmt;

    protected ParseState parseState;

    protected bool plated;

    protected Dictionary<long, Tool> tools;

    protected Tool tool;

    protected RoutMode routMode;

    protected Point64 pos;

    protected Path64 path;

    protected Plot plotPth;

    protected Plot plotNpth;

    protected List<Via> vias;
    protected abstract void CommitPath();
    protected abstract void AddArc(Point64 start, Point64 end, long radius, bool ccw);
    protected abstract Dictionary<char, string> ParseRegularCommand(string cmd);
    protected abstract bool Command(string cmd);
    public abstract Paths64 GetPaths(bool plated = true, bool unplated = true);
    protected List<Via> GetVias()
    {
        return vias;
    }
}

