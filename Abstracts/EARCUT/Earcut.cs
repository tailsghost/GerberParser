using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberParser.Abstracts.EARCUT;

public abstract class Earcut<N> where N : class
{
    public List<N> Indices { get; protected set; } = new List<N>();
    public int Vertices { get; protected set; } = 0;

    public abstract void Execute(IEnumerable<IEnumerable<(double x, double y)>> polygons);

    protected abstract Node LinkedList(IEnumerable<(double x, double y)> points, bool clockwise);
    protected abstract Node? FilterPoints(Node start, Node? end = null);
    protected abstract void EarcutLinked(Node ear, int pass = 0);
    protected abstract bool IsEar(Node ear);
    protected abstract bool IsEarHashed(Node ear);
    protected abstract Node? CureLocalIntersections(Node start);
    protected abstract void SplitEarcut(Node start);
    protected abstract Node EliminateHoles(IEnumerable<IEnumerable<(double x, double y)>> points, Node outerNode);
    protected abstract void EliminateHole(Node hole, Node outerNode);
    protected abstract Node FindHoleBridge(Node hole, Node outerNode);
    protected abstract void IndexCurve(Node start);
    protected abstract Node SortLinked(Node list);
    protected abstract int ZOrder(double x, double y);
    protected abstract Node GetLeftmost(Node start);
    protected abstract bool PointInTriangle(double ax, double ay, double bx, double by, double cx, double cy, double px, double py);
    protected abstract bool IsValidDiagonal(Node a, Node b);
    protected abstract double Area(Node p, Node q, Node r);
    protected abstract bool Equals(Node p1, Node p2);
    protected abstract bool Intersects(Node p1, Node q1, Node p2, Node q2);
    protected abstract bool IntersectsPolygon(Node a, Node b);
    protected abstract bool LocallyInside(Node a, Node b);
    protected abstract bool MiddleInside(Node a, Node b);
    protected abstract Node SplitPolygon(Node a, Node b);
    protected abstract Node InsertNode(int i, (double x, double y) p, Node last);
    protected abstract void RemoveNode(Node p);

    protected bool Hashing { get; set; }
    protected double MinX { get; set; }
    protected double MaxX { get; set; }
    protected double MinY { get; set; }
    protected double MaxY { get; set; }
    protected double InvSize { get; set; } = 0;

    protected ObjectPool<Node> Nodes { get; set; } = new();

    public void Process(IEnumerable<IEnumerable<(double x, double y)>> points)
    {
        Indices.Clear();
        Vertices = 0;

        if (points == null || !points.Any()) return;

        double x;
        double y;
        int threshold = 80;
        ulong len = 0;

        foreach (var polygon in points)
        {
            if (threshold >= 0)
            {
                threshold -= polygon.Count();
            }
            len += (ulong)polygon.Count();
        }

        Nodes.Reset((int)(len * 3 / 2));
        Indices.Capacity = (int)(len + (ulong)points.First().Count());

        var outerNode = LinkedList(points.First(), true);
        if (outerNode == null) return;

        if (points.Count() > 1)
        {
            outerNode = EliminateHoles(points, outerNode);
        }

        bool hashing = threshold < 0;
        if (hashing)
        {
            var p = outerNode.Next;
            MinX = MaxX = p.X;
            MinY = MaxY = p.Y;

            do
            {
                x = p.X;
                y = p.Y;
                MinX = Math.Min(MinX, x);
                MinY = Math.Min(MinY, y);
                MaxX = Math.Max(MaxX, x);
                MaxY = Math.Max(MaxY, y);
                p = p.Next;
            } while (p != outerNode);

            InvSize = Math.Max(MaxX - MinX, MaxY - MinY);
            InvSize = InvSize != 0 ? 1.0 / InvSize : 0.0;
        }

        EarcutLinked(outerNode);

        Nodes.Clear();
    }
}
