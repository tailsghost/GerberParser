using ClipperLib;

namespace GerberParser.Property.Net;

public class PhysicalNet
{
    public List<Shape> shapes { get; init; }

    public List<Via> vias { get; init; }

    public HashSet<LogicalNet> logicalNets { get; init; }

    public PhysicalNet(Shape shape)
    {
        shapes = new List<Shape> { shape };
        vias = new List<Via>();
        logicalNets = new HashSet<LogicalNet>();
    }

    public void AddVia(Via via)
    {
        vias.Add(via);
    }

    public void MergeWith(PhysicalNet net)
    {
        foreach (var shape in net.shapes)
        {
            shapes.Add(shape);
        }

        foreach (var via in net.vias)
        {
            vias.Add(via);
        }

        foreach (var logicalNet in net.logicalNets)
        {
            logicalNets.Add(logicalNet);
        }
    }

    public bool Contains(IntPoint point, int layer)
    {
        return shapes.Any(shape => shape.layer == layer && shape.Contains(point));
    }

    public void AssignLogical(LogicalNet logicalNet)
    {
        logicalNets.Add(logicalNet);
    }
}
