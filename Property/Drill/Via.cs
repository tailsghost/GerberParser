using Clipper2Lib;

namespace GerberParser.Property.Drill;

public class Via
{
    public Via(Path64 path, long finished_hole_size)
    {
        Path = path;
        Finished_hole_size = finished_hole_size;
    }

    public Path64 Path { get; }

    public long Finished_hole_size { get; }
}
