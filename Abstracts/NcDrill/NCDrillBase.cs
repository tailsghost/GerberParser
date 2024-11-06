using Clipper2Lib;
using GerberParser.Core.Coord;
using GerberParser.Core.PlotCore;
using GerberParser.Enums;
using GerberParser.Property;
using GerberParser.Property.Drill;

namespace GerberParser.Abstracts.NcDrill;


public abstract class NCDrillBase
{
    protected ConcreteFormat fmt = new();

    protected ParseState parseState;

    protected bool plated;

    protected Dictionary<long, Tool> Tools = new();

    protected Tool Tool;

    protected RoutMode RoutMode;

    protected Point64 Pos;

    protected Path64 Path = new();

    protected Plot PlotPth = new();

    protected Plot PlotNpth = new();

    protected List<Via> Vias = new();
    protected abstract void CommitPath();
    protected abstract void AddArc(Point64 start, Point64 end, long radius, bool ccw);
    protected abstract Dictionary<char, string> ParseRegularCommand(string cmd);
    protected abstract bool Command(string cmd);
    public abstract Paths64 GetPaths(bool plated = true, bool unplated = true);
    public List<Via> GetVias()
    {
        return Vias;
    }
}

