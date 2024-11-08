using GerberParser.Helpers;
using GerberParser.Vertex;

namespace GerberParser.Abstracts.OBJECT;

public abstract class ObjFileBase
{
    protected Indexed<Vertex3> Vertices { get; } = [];

    protected Indexed<Vertex2> UvCoordinates { get; } = [];

    protected List<Core.OBJECT.Object> Objects = [];

    public abstract Core.OBJECT.Object AddObject(string name, string material);

    public abstract void ToFile(StringWriter stream);
}
