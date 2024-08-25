namespace GerberParser.Vertex;

public struct Vertex2
{
    public double X { get; }
    public double Y { get; }

    public Vertex2(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double this[int index]
    {
        get
        {
            return index switch
            {
                0 => X,
                1 => Y,
                _ => throw new IndexOutOfRangeException("Index must be 0 or 1.")
            };
        }
    }

    public double[] ToArray()
    {
        return new[] { X, Y };
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}
