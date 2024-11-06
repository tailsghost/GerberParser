using Clipper2Lib;
using GerberParser.Core.OBJECT;
using GerberParser.Property.Obj;
using GerberParser.Vertex;

namespace GerberParser.Abstracts.Object;

public abstract class ObjectBase
{

    protected ObjectBase(ObjFile owner, string name, string material)
    {
        Owner = owner;
        Name = name;
        Material = material;
    }

    public ObjFile Owner {  get; }

    public string Name { get; }

    public string Material { get; }

    public List<List<Corner>> Faces { get; }

    public abstract void AddFace(List<Vertex3> vertices);

    public abstract void AddSurface(Paths64 polygon, double z);

    public abstract void AddSurface(Path64 polygon, Paths64 holes, double z);

    public abstract void AddRing(Path64 outline, double z1, double z2);

    public abstract void AddSheet(Paths64 polygons, double z1, double z2);

    public abstract void PolyNodesToSurfaces(PolyTree64 rootNode, double z);
}
