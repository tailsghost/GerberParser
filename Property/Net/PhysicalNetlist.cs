using ClipperLib;

namespace GerberParser.Property.Net;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

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

    public void RegisterPaths(Polygons paths, int layer)
    {
        var clipper = new Clipper();
        clipper.StrictlySimple = true;
        clipper.AddPaths(paths, PolyType.ptSubject, true);

        PolyTree tree = new();
        clipper.Execute(ClipType.ctUnion, tree);

        NodesToPhysicalNetlist(tree.Childs, layer);
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

    public PhysicalNet FindNet(IntPoint point, int layer)
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


    private void NodesToPhysicalNetlist(List<PolyNode> nodes, int layer)
    {
        foreach(var node in nodes)
        {
            if (node.IsHole)
                throw new ArgumentException("Shape is a hole?");

            Polygons holes = new();

            foreach(var hole in node.Childs)
            {
                if(!hole.IsHole)
                    throw new ArgumentException("Hole is not a hole?");

                holes.Add(hole.Contour);
                NodesToPhysicalNetlist(hole.Childs, layer);
            }

            RegisterShape(new Shape(node.Contour, holes, layer));
        }
    }
}
