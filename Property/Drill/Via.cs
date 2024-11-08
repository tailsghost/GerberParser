namespace GerberParser.Property.Drill;

using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;

public class Via
{
    public Via(Polygon path, long finished_hole_size)
    {
        Path = path;
        Finished_hole_size = finished_hole_size;
    }

    public Polygon Path { get; }

    public long Finished_hole_size { get; }
}
