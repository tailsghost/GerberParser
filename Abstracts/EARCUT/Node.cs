namespace GerberParser.Abstracts.EARCUT;

public class Node
{
    public int Index { get; }
    public double X { get; }
    public double Y { get; }
    public Node? Prev { get; set; }
    public Node? Next { get; set; }
    public int Z { get; set; }
    public Node? PrevZ { get; set; }
    public Node? NextZ { get; set; }
    public bool IsSteiner { get; set; }

    public Node(int index, double x, double y)
    {
        Index = index;
        X = x;
        Y = y;
        Prev = null;
        Next = null;
        Z = 0;
        PrevZ = null;
        NextZ = null;
        IsSteiner = false;
    }

    public Node()
    {
    }
}