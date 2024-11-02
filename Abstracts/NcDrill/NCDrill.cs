using Clipper2Lib;
using GerberParser.Abstracts.Coord;
using GerberParser.Core.PlotCore;
using GerberParser.Enums;

namespace GerberParser.Abstracts.NcDrill;


public abstract class NCDrill
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

    protected NCDrill(Stream s, bool defaultPlated = true)
    {
    }
    protected abstract void CommitPath();
    protected abstract void AddArc(Point64 start, Point64 end, long radius, bool ccw);
    protected abstract Dictionary<char, string> ParseRegularCommand(string cmd);
    protected abstract bool Command(string cmd);
    public abstract Paths64 GetPaths(bool plated = true, bool unplated = true);
    public abstract List<Via> GetVias();
}

