using Clipper2Lib;
using System.Net;

namespace GerberParser.Property.Net;

public class PhysicalNetlist
{
    public List<PhysicalNet> nets { get; private set; } = new();
    private bool viasAdded;

    public PhysicalNetlist()
    {
        viasAdded = false;
    }
    public void RegisterShape(Shape shape)
    {
        if (viasAdded) throw new InvalidOperationException("Cannot add shapes after the first via has been added.");
        nets.Add(new PhysicalNet(shape));
    }

    public void RegisterPaths(Paths64 paths, int layer)
    {
        var clipper = new Clipper64();
        clipper.AddSubject(paths);

        Paths64 solution = new Paths64();
        clipper.Execute(ClipType.Union, FillRule.NonZero, solution);

        foreach (var path in solution)
        {
            RegisterShape(new Shape(path, new List<Path64>(), layer));
        }
    }

    public bool RegisterVia(Via via, int numLayers)
    {
        int lowerLayer = via.GetLowerLayer(numLayers);
        int upperLayer = via.GetUpperLayer(numLayers);
        if (lowerLayer >= upperLayer)
        {
            throw new InvalidOperationException("Via has null layer range or only includes one layer");
        }
        viasAdded = true;
        bool ok = true;
        PhysicalNet target = null;
        for (int layer = lowerLayer; layer <= upperLayer; layer++)
        {
            var source = FindNet(via.GetCoordinate(), layer);
            if (source == null)
            {
                ok = false;
                continue;
            }
            else if (target == null)
            {
                target = source;
            }
            else if (source == target)
            {
                continue;
            }
            else
            {
                target.MergeWith(source);
                nets.Remove(source);
            }
        }
        if (target != null)
        {
            target.AddVia(via);
        }
        return ok;
    }

    public PhysicalNet FindNet(Point64 point, int layer)
    {
        foreach (var net in nets)
        {
            if (net.Contains(point, layer))
            {
                return net;
            }
        }
        return null;
    }
}
