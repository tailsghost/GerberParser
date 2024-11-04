using Clipper2Lib;

namespace GerberParser.Property.Drill;

public class Via
{
    public Via(Path64 path, long finished_hole_size)
    {
        Get_Path = path;
        Finished_hole_size = finished_hole_size;
    }

    public Path64 Get_Path { get; }

    public long Finished_hole_size { get; }
}
