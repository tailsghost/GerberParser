using GerberParser.Core.OBJECT;
using GerberParser.Property.Obj;
using GerberParser.Vertex;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using ClipperLib;

namespace GerberParser.Abstracts.Object;

public abstract class ObjectBase(ObjFile owner, string name, string material)
{
    public ObjFile Owner { get; } = owner;

    public string Name { get; } = name;

    public string Material { get; } = material;

    public List<List<Corner>> Faces { get; } = [];

    public abstract void AddFace(List<Vertex3> vertices);

    public abstract void AddSurface(Polygons polygons, double z);

    public abstract void AddSurface(Polygon polygon, Polygons holes, double z);

    public abstract void AddRing(Polygon outline, double z1, double z2);

    public abstract void AddSheet(Polygons polygons, double z1, double z2);

    public abstract void PolyNodesToSurfaces(List<PolyNode> nodes, double z);

    public abstract void PolyNodesToRings(List<PolyNode> nodes, double z1, double z2);
}
