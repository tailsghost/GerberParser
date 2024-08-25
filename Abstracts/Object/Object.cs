using Clipper2Lib;
using GerberParser.Vertex;

namespace GerberParser.Abstracts.Object;

public abstract class Object
{
    public abstract ObjFile Owner {  get; }

    public string Name { get; }

    public string Material { get; }

    public List<List<Corner>> faces { get; }

    public abstract void Add_face(List<Vertex3> vertices);

    public abstract void Add_Surface(Paths64 polygon, double z);

    public abstract void Add_Ring(Path64 outline, double z1, double z2);

    public abstract void Add_Sheet(Paths64 polygons, double z1, double z2);
}
