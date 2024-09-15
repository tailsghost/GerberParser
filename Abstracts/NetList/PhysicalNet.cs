using Clipper2Lib;
using GerberParser.Abstracts.NcDrill;

namespace GerberParser.Abstracts.NetList;

public abstract class PhysicalNet
{
    protected List<Shape> shapes {  get; init; }

    protected List<Via> vias { get; init; }

    protected HashSet<LogicalNet> logicalNets { get; init; }

    public PhysicalNet(Shape shape)
    {
        shapes = new List<Shape> { shape };
        vias = new List<Via>();
        logicalNets = new HashSet<LogicalNet>();
    }

    public abstract void AddVia(Via via);

    public abstract void MergeWith(PhysicalNet net);

    public abstract bool Contains(Point64 point, int layer);

    public abstract void AssignLogical(LogicalNet logicalNet);
}
