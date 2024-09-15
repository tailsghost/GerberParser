using Clipper2Lib;

namespace GerberParser.Abstracts.NetList;

public abstract class PhysicalNetlist
{
    private List<PhysicalNet> nets { get; set; } = new();
    private bool viasAdded;

    public PhysicalNetlist() { }
    public abstract void RegisterShape(Shape shape);

    public abstract void RegisterPaths(List<Path64> paths, int layer);

    public abstract bool RegisterVia(Via via, int numLayers);

    public abstract PhysicalNet FindNet(Point64 point, int layer);
}
