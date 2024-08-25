using GerberParser.Vertex;

namespace GerberParser.Abstracts.Object;

public abstract class Corner
{
    public ulong Vertex_index {  get; set; }

    public ulong Uv_coordinate_index { get; set; }
}
