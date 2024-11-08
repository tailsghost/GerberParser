using GerberParser.Abstracts.Object;
using GerberParser.Core.EARCUT;
using GerberParser.Property.Obj;
using GerberParser.Vertex;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using ClipperLib;

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

    public override void AddRing(Polygon outline, double z1, double z2)
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

    public override void PolyNodesToRings(List<PolyNode> nodes, double z1, double z2)
    {
        foreach (var node in nodes)
        {
            AddRing(node.Contour, z1, z2);

            PolyNodesToRings(node.Childs, z1,z2);
        }
    }


    public override void AddSheet(Polygons polygons, double z1, double z2)
    {
        var clipper = new Clipper();
        clipper.PreserveCollinear = true;
        clipper.AddPaths(polygons, PolyType.ptSubject, true);

        PolyTree solutionClosed = new();

        clipper.Execute(ClipType.ctUnion, solutionClosed);


        PolyNodesToSurfaces(solutionClosed.Childs, z1);
        PolyNodesToSurfaces(solutionClosed.Childs, z2);

        PolyNodesToRings(solutionClosed.Childs, z1, z2);
    }

    public override void AddSurface(Polygons polygon, double z)
    {
        var clipper = new Clipper();
        clipper.StrictlySimple = true;
        clipper.AddPaths(polygon, PolyType.ptSubject, true);

        PolyTree solutionClosed = new();

        clipper.Execute(ClipType.ctUnion, solutionClosed);

        PolyNodesToSurfaces(solutionClosed.Childs, z);

    }

    public override void AddSurface(Polygon polygon, Polygons holes, double z)
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

    public override void PolyNodesToSurfaces(List<PolyNode> rootNode, double z)
    {
        
        foreach(var node in rootNode)
        {
            if (node.IsHole)
                throw new ArgumentException("Shape is a hole?");

            Polygons holes = new Polygons();

            foreach(var hole in node.Childs)
            {
                if(!hole.IsHole)
                    throw new ArgumentException("Hole is not a hole?");

                holes.Add(hole.Contour);
                PolyNodesToSurfaces(hole.Childs, z);
            }

            AddSurface(node.Contour, holes, z);

        }
    }

    private List<int> Earcut(List<List<Vertex2>> poly)
    {
        var earcut = new Earcut();
        earcut.Process(poly);
        return earcut.Indices;
    }
}