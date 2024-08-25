using GerberParser.Helpers;
using GerberParser.Vertex;

namespace GerberParser.Abstracts.Object;

public abstract class ObjFile
{
    internal Indexed<Vertex3> Vertices { get; } = new Indexed<Vertex3>();

    internal Indexed<Vertex2> UvCoordinates { get; } = new Indexed<Vertex2>();

    private List<Object> _objects = new();

    public abstract Object AddObject(string name, string material);

    public abstract void ToFile(StringWriter stream);
}
