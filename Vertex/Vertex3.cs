namespace GerberParser.Vertex;

public struct Vertex3
{
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Vertex3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    public double this[int index]
    {
        get
        {
            return index switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                _ => throw new IndexOutOfRangeException("Index must be 0, 1, or 2.")
            };
        }
    }
    public double[] ToArray()
    {
        return new[] { X, Y, Z };
    }

    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }
}
