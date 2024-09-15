namespace GerberParser.Abstracts.EARCUT;

public class Node<N>
{
    public N i { get; }
    public double x { get; }
    public double y { get; }

    public Node<N>? Prev { get; set; }
    public Node<N>? Next { get; set; }

    public int Z { get; set; }

    public Node<N>? PrevZ { get; set; }
    public Node<N>? NextZ { get; set; }

    public bool Steiner { get; set; }

    public Node(N index, double x_, double y_)
    {
        i = index;
        x = x_;
        y = y_;
        Prev = null;
        Next = null;
        Z = 0;
        PrevZ = null;
        NextZ = null;
        Steiner = false;
    }

    public Node()
    {
        
    }
}