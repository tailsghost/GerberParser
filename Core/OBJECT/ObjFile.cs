using GerberParser.Abstracts.OBJECT;

namespace GerberParser.Core.OBJECT;

public class ObjFile : ObjFileBase
{
    public override Object AddObject(string name, string material)
    {
        var obj = new Object(this, name, material);
        Objects.Add(obj);
        return obj;
    }

    public override void ToFile(StringWriter stream)
    {
        foreach (var vertex in Vertices)
        {
            stream.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");
        }

        var uMin = UvCoordinates.Min(uv => uv.X);
        var uMax = UvCoordinates.Max(uv => uv.X);
        var vMin = UvCoordinates.Min(uv => uv.Y);
        var vMax = UvCoordinates.Max(uv => uv.Y);

        var uScale = 1.0 / (uMax - uMin);
        var vScale = 1.0 / (vMax - vMin);
        foreach (var uv in UvCoordinates)
        {
            stream.WriteLine($"vt {(uv.X - uMin) * uScale} {(uv.Y - vMin) * vScale}");
        }

        foreach (var obj in Objects)
        {
            stream.WriteLine($"g {obj.Name}");
            stream.WriteLine($"usemtl {obj.Material}");
            foreach (var face in obj.Faces)
            {
                stream.Write("f");
                foreach (var corner in face)
                {
                    stream.Write($" {corner.Vertex_index}/{corner.Uv_coordinate_index}");
                }
                stream.WriteLine();
            }
        }
    }
}
