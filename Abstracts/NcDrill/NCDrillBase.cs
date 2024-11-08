using ClipperLib;
using GerberParser.Core.Coord;
using GerberParser.Core.PlotCore;
using GerberParser.Enums;
using GerberParser.Property;
using GerberParser.Property.Drill;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;

namespace GerberParser.Abstracts.NcDrill;


public abstract class NCDrillBase
{
    protected ConcreteFormat fmt = new();

    protected ParseState parseState;

    protected bool plated;

    protected Dictionary<long, Tool> Tools = [];

    protected Tool? Tool;

    protected RoutMode RoutMode;

    protected IntPoint Pos;

    protected Polygon Path = [];

    protected Plot PlotPth = new();

    protected Plot PlotNpth = new();

    protected List<Via> Vias = [];
    protected abstract void CommitPath();
    protected abstract void AddArc(IntPoint start, IntPoint end, long radius, bool ccw);
    protected abstract Dictionary<char, string> ParseRegularCommand(string cmd);
    protected abstract bool Command(string cmd);
    public abstract Polygons GetPaths(bool plated = true, bool unplated = true);
    public List<Via> GetVias()
    {
        return Vias;
    }
}

