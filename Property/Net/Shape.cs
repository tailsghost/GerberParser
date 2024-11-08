
using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using ClipperLib;

namespace GerberParser.Property.Net;

public class Shape
{
    public Polygon outline { get; }

    public Polygons holes { get; }

    public IntRect boundingBox { get; set; }

    public int layer { get; }

    public Shape(Polygon outline, Polygons holes, int layer)
    {
        this.outline = outline;
        this.holes = holes;
        this.layer = layer;
        CalculateBoundingBox(outline);
    }

    public bool Contains(IntPoint point)
    {
        if (point.X < boundingBox.left || point.X > boundingBox.right ||
            point.Y < boundingBox.bottom || point.Y > boundingBox.top)
            return false;

        if (Clipper.PointInPolygon(point, outline) == 0)
            return false;

        foreach (var hole in holes)
        {
            if (Clipper.PointInPolygon(point, hole) == 1)
                return false;
        }

        return true;
    }


    public void CalculateBoundingBox(Polygon outline)
    {
        boundingBox = new IntRect()
        {
            left = outline[0].X,
            right = outline[0].X,
            bottom = outline[0].Y,
            top = outline[0].Y
        };

        foreach (var coord in outline)
        {
            boundingBox = new IntRect
            {
                left = Math.Min(boundingBox.left, coord.X),
                bottom = Math.Min(boundingBox.bottom, coord.Y),
                right = Math.Max(boundingBox.right, coord.X),
                top = Math.Max(boundingBox.top, coord.Y),
            };
        }
    }
}
