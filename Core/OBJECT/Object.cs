using Clipper2Lib;
using GerberParser.Abstracts.Object;
using GerberParser.Core.EARCUT;
using GerberParser.Property.Obj;
using GerberParser.Vertex;

namespace GerberParser.Core.OBJECT;

public class Object : ObjectBase
{

    public Object(ObjFile owner, string name, string material)
        : base(owner, name, material)
    {
    }

    public override void AddFace(List<Vertex3> vertices)
    {
        if (vertices.Count < 3)
        {
            throw new ArgumentException("A face needs at least 3 corners");
        }
        var corners = vertices.Select(vertex => new Corner(vertex, Owner)).ToList();
        Faces.Add(corners);
    }

    public override void AddRing(Path64 outline, double z1, double z2)
    {
        if (outline.Count < 3)
        {
            throw new ArgumentException("An outline needs at least 3 coordinates");
        }
        var x1 = outline[^1].X;
        var y1 = outline[^1].Y;
        foreach (var coord in outline)
        {
            var x2 = coord.X;
            var y2 = coord.Y;
            AddFace(new List<Vertex3>
                {
                    new Vertex3(x1, y1, z1),
                    new Vertex3(x2, y2, z1),
                    new Vertex3(x2, y2, z2)
                });
            AddFace(new List<Vertex3>
                {
                    new Vertex3(x1, y1, z2),
                    new Vertex3(x1, y1, z1),
                    new Vertex3(x2, y2, z2)
                });
            x1 = x2;
            y1 = y2;
        }
    }

    public override void AddSheet(Paths64 polygons, double z1, double z2)
    {
        Clipper64 clipper = new Clipper64();
        clipper.PreserveCollinear = true;
        clipper.AddSubject(polygons);

        PolyTree64 solutionClosed = new PolyTree64();
        Paths64 solutionOpen = new Paths64();

        clipper.Execute(ClipType.Union, FillRule.NonZero, solutionClosed, solutionOpen);


        PolyNodesToSurfaces(solutionClosed, z1);
        PolyNodesToSurfaces(solutionClosed, z2);


        foreach (var path in solutionOpen)
        {
            AddRing(path, z1, z2);
        }
    }

    public override void AddSurface(Paths64 polygon, double z)
    {
        Clipper64 clipper = new Clipper64();
        clipper.PreserveCollinear = true;
        clipper.AddSubject(polygon);

        PolyTree64 solutionClosed = new PolyTree64();
        Paths64 solutionOpen = new Paths64();

        clipper.Execute(ClipType.Union, FillRule.NonZero, solutionClosed, solutionOpen);

        PolyNodesToSurfaces(solutionClosed, z);

    }

    public override void AddSurface(Path64 polygon, Paths64 holes, double z)
    {
        var shapeData = new List<List<Vertex2>>(1 + holes.Count);
        var vertexData = new List<Vertex2>();
        shapeData.Add(polygon.Select(coord => new Vertex2(coord.X, coord.Y)).ToList());
        vertexData.AddRange(shapeData[0]);

        foreach (var hole in holes)
        {
            var holeData = hole.Select(coord => new Vertex2(coord.X, coord.Y)).ToList();
            shapeData.Add(holeData);
            vertexData.AddRange(holeData);
        }

        var indices = Earcut(shapeData);
        for (int i = 0; i < indices.Count; i += 3)
        {
            AddFace(new List<Vertex3>
                {
                    new Vertex3(vertexData[indices[i]].X, vertexData[indices[i]].Y, z),
                    new Vertex3(vertexData[indices[i + 1]].X, vertexData[indices[i + 1]].Y, z),
                    new Vertex3(vertexData[indices[i + 2]].X, vertexData[indices[i + 2]].Y, z)
                });
        }
    }

    public override void PolyNodesToSurfaces(PolyTree64 rootNode, double z)
    {
        Stack<PolyPath64> stack = new Stack<PolyPath64>();
        stack.Push(rootNode);

        while (stack.Count > 0)
        {
            PolyPath64 node = stack.Pop();

            if (node.IsHole)
            {
                throw new InvalidOperationException("Shape is a hole?");
            }

            Paths64 holes = new Paths64();

            for (int i = 0; i < node.Count; i++)
            {
                PolyPath64 childNode = node[i];

                if (!childNode.IsHole)
                {
                    throw new InvalidOperationException("Hole is not a hole?");
                }

                holes.Add(childNode.Polygon);

                stack.Push(childNode);
            }

            AddSurface(node.Polygon, holes, z);
        }
    }

    private List<int> Earcut(List<List<Vertex2>> poly)
    {
        var earcut = new Earcut();
        earcut.Process(poly);
        return earcut.Indices;
    }
}