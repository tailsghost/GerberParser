using Clipper2Lib;
using GerberParser.Abstracts.Aperture;

namespace GerberParser.Property.Net;

public class Shape
{
    public Path64 outline { get; }

    public List<Path64> holes { get; }

    public Rect64 boundingBox { get; set; }

    public int layer { get; }

    public Shape(Path64 outline, List<Path64> holes, int layer)
    {
        this.outline = outline;
        this.holes = holes;
        this.layer = layer;
        CalculateBoundingBox(outline);
    }

    public bool Contains(Point64 point)
    {
        if (point.X < boundingBox.left || point.X > boundingBox.right ||
            point.Y < boundingBox.bottom || point.Y > boundingBox.top)
            return false;

        if (Clipper.PointInPolygon(point, outline) == 0)
            return false;

        foreach (var hole in holes)
        {
            if (Clipper.PointInPolygon(point, hole) == (PointInPolygonResult)1)
                return false;
        }

        return true;
    }


    public void CalculateBoundingBox(Path64 outline)
    {
        boundingBox = new Rect64()
        {
            left = outline[0].X,
            right = outline[0].X,
            bottom = outline[0].Y,
            top = outline[0].Y
        };

        foreach (var coord in outline)
        {
            boundingBox = new Rect64
            {
                left = Math.Min(boundingBox.left, coord.X),
                bottom = Math.Min(boundingBox.bottom, coord.Y),
                right = Math.Max(boundingBox.right, coord.X),
                top = Math.Max(boundingBox.top, coord.Y),
            };
        }
    }
}
