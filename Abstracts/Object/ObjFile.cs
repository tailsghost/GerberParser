using GerberParser.Helpers;
using GerberParser.Vertex;

namespace GerberParser.Abstracts.OBJECT;

public abstract class ObjFileBase
{
    protected Indexed<Vertex3> Vertices { get; } = new Indexed<Vertex3>();

    protected Indexed<Vertex2> UvCoordinates { get; } = new Indexed<Vertex2>();

    protected List<Core.OBJECT.Object> Objects = new();

    public abstract Core.OBJECT.Object AddObject(string name, string material);

    public abstract void ToFile(StringWriter stream);
}
