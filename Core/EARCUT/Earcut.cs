using GerberParser.Vertex;

namespace GerberParser.Core.EARCUT;

public class Earcut
{
    public List<int> Indices { get; protected set; } = new List<int>();
    public int Vertices { get; protected set; } = 0;
    protected bool Hashing { get; set; }
    protected double MinX { get; set; }
    protected double MaxX { get; set; }
    protected double MinY { get; set; }
    protected double MaxY { get; set; }
    protected double InvSize { get; set; } = 0;

    protected ObjectPool<Node<int>> Nodes { get; set; } = new();

    public void Process(IEnumerable<IEnumerable<Vertex2>> points)
    {
        Indices.Clear();
        Vertices = 0;

        if (points == null || !points.Any()) return;

        double x;
        double y;
        int threshold = 80;
        ulong len = 0;

        var polygonList = points.Select(p => p.ToList()).ToList();

        foreach (var polygon in polygonList)
        {
            if (threshold >= 0)
            {
                threshold -= polygon.Count;
            }
            len += (ulong)polygon.Count;
        }

        Nodes.Reset((int)(len * 3 / 2));
        Indices.Capacity = (int)(len + (ulong)polygonList.First().Count);

        var outerNode = LinkedList(polygonList.First(), true);
        if (outerNode == null) return;

        if (polygonList.Count > 1)
        {
            outerNode = EliminateHoles(polygonList, outerNode);
        }

        bool hashing = threshold < 0;
        if (hashing)
        {
            var p = outerNode.Next;
            MinX = MaxX = p.x;
            MinY = MaxY = p.y;

            do
            {
                x = p.x;
                y = p.y;
                MinX = Math.Min(MinX, x);
                MinY = Math.Min(MinY, y);
                MaxX = Math.Max(MaxX, x);
                MaxY = Math.Max(MaxY, y);
                p = p.Next;
            } while (p != outerNode);

            InvSize = Math.Max(MaxX - MinX, MaxY - MinY);
            InvSize = InvSize != 0 ? 1.0 / InvSize : 0.0;
        }

        EarcutLinked(outerNode, 0);

        Nodes.Clear();
    }

    public Node<int> LinkedList(List<Vertex2> points, bool clockwise)
    {
        double sum = 0;
        int len = points.Count;
        Node<int> last = null;

        for (int i = 0, j = len > 0 ? len - 1 : 0; i < len; j = i++)
        {
            var p1 = points[i];
            var p2 = points[j];

            double p10 = p1.X;
            double p11 = p1.Y;
            double p20 = p2.X;
            double p21 = p2.Y;

            sum += (p20 - p10) * (p11 + p21);
        }

        if (clockwise == sum > 0)
        {
            for (int i = 0; i < len; i++)
                last = InsertNode(i, points[i], last);
        }
        else
        {
            for (int i = len; i-- > 0;)
                last = InsertNode(i, points[i], last);
        }

        if (last != null && Equals(last, last.Next))
        {
            RemoveNode(last);
            last = last.Next;
        }

        return last;
    }

    public Node<int> FilterPoints(Node<int> start, Node<int> end = null)
    {
        if (end == null) end = start;

        Node<int> p = start;
        bool again;

        do
        {
            again = false;

            if (!p.Steiner && (Equals(p, p.Next) || Area(p.Prev, p, p.Next) == 0))
            {
                RemoveNode(p);
                p = end = p.Prev;

                if (p == p.Next) break;
                again = true;
            }
            else
            {
                p = p.Next;
            }
        } while (again || p != end);

        return end;
    }

    public void EarcutLinked(Node<int> ear, int pass)
    {
        if (ear == null) return;

        if (pass == 0 && Hashing) IndexCurve(ear);

        Node<int> stop = ear;
        Node<int> prev, next;

        while (ear.Prev != ear.Next)
        {
            prev = ear.Prev;
            next = ear.Next;

            if (Hashing ? IsEarHashed(ear) : IsEar(ear))
            {
                Indices.Append(prev.i);
                Indices.Append(ear.i);
                Indices.Append(next.i);

                RemoveNode(ear);

                ear = next.Next;
                stop = next.Next;

                continue;
            }

            ear = next;

            if (ear == stop)
            {
                if (pass == 0) EarcutLinked(FilterPoints(ear), 1);
                else if (pass == 1)
                {
                    ear = CureLocalIntersections(ear);
                    EarcutLinked(ear, 2);
                }
                else if (pass == 2)
                {
                    SplitEarcut(ear);
                }

                break;
            }
        }
    }

    public bool IsEar(Node<int> ear)
    {
        Node<int> a = ear.Prev;
        Node<int> b = ear;
        Node<int> c = ear.Next;

        if (Area(a, b, c) >= 0) return false;

        Node<int> p = ear.Next.Next;
        while (p != ear.Prev)
        {
            if (PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) && Area(p.Prev, p, p.Next) >= 0)
                return false;
            p = p.Next;
        }

        return true;
    }

    public bool IsEarHashed(Node<int> ear)
    {
        Node<int> a = ear.Prev;
        Node<int> b = ear;
        Node<int> c = ear.Next;

        if (Area(a, b, c) >= 0) return false;

        double minTX = Math.Min(a.x, Math.Min(b.x, c.x));
        double minTY = Math.Min(a.y, Math.Min(b.y, c.y));
        double maxTX = Math.Max(a.x, Math.Max(b.x, c.x));
        double maxTY = Math.Max(a.y, Math.Max(b.y, c.y));

        int minZ = ZOrder(minTX, minTY);
        int maxZ = ZOrder(maxTX, maxTY);

        Node<int> p = ear.NextZ;
        while (p != null && p.Z <= maxZ)
        {
            if (p != ear.Prev && p != ear.Next &&
                PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) && Area(p.Prev, p, p.Next) >= 0)
                return false;
            p = p.NextZ;
        }

        p = ear.PrevZ;
        while (p != null && p.Z >= minZ)
        {
            if (p != ear.Prev && p != ear.Next &&
                PointInTriangle(a.x, a.y, b.x, b.y, c.x, c.y, p.x, p.y) && Area(p.Prev, p, p.Next) >= 0)
                return false;
            p = p.PrevZ;
        }

        return true;
    }

    public Node<int> CureLocalIntersections(Node<int> start)
    {
        Node<int> p = start;
        do
        {
            Node<int> a = p.Prev;
            Node<int> b = p.Next.Next;

            if (!Equals(a, b) && Intersects(a, p, p.Next, b) && LocallyInside(a, b) && LocallyInside(b, a))
            {
                Indices.Add(a.i);
                Indices.Add(p.i);
                Indices.Add(b.i);

                RemoveNode(p);
                RemoveNode(p.Next);

                p = start = b;
            }
            p = p.Next;
        } while (p != start);

        return p;
    }

    public void SplitEarcut(Node<int> start)
    {
        Node<int> a = start;
        do
        {
            Node<int> b = a.Next.Next;
            while (b != a.Prev)
            {
                if (a.i != b.i && IsValidDiagonal(a, b))
                {
                    Node<int> c = SplitPolygon(a, b);

                    a = FilterPoints(a, a.Next);
                    c = FilterPoints(c, c.Next);

                    EarcutLinked(a, 0);
                    EarcutLinked(c, 0);
                    return;
                }
                b = b.Next;
            }
            a = a.Next;
        } while (a != start);
    }

    public Node<int> EliminateHoles(List<List<Vertex2>> points, Node<int> outerNode)
    {
        int len = points.Count;

        List<Node<int>> queue = new List<Node<int>>();
        for (int i = 1; i < len; i++)
        {
            var holePoints = points[i];
            Node<int> list = LinkedList(holePoints, false);

            if (list != null)
            {
                if (list == list.Next) list.Steiner = true;
                queue.Add(GetLeftmost(list));
            }
        }

        queue.Sort((a, b) => a.x.CompareTo(b.x));

        foreach (var hole in queue)
        {
            EliminateHole(hole, outerNode);
            outerNode = FilterPoints(outerNode, outerNode.Next);
        }

        return outerNode;
    }

    private void EliminateHole(Node<int> hole, Node<int> outerNode)
    {
        outerNode = FindHoleBridge(hole, outerNode);
        if (outerNode != null)
        {
            Node<int> b = SplitPolygon(outerNode, hole);
            FilterPoints(b, b.Next);
        }
    }

    public Node<int> FindHoleBridge(Node<int> hole, Node<int> outerNode)
    {
        Node<int> p = outerNode;
        double hx = hole.x;
        double hy = hole.y;
        double qx = double.NegativeInfinity;
        Node<int> m = null;

        do
        {
            if (hy <= p.y && hy >= p.Next.y && p.Next.y != p.y)
            {
                double x = p.x + (hy - p.y) * (p.Next.x - p.x) / (p.Next.y - p.y);
                if (x <= hx && x > qx)
                {
                    qx = x;
                    if (x == hx)
                    {
                        if (hy == p.y) return p;
                        if (hy == p.Next.y) return p.Next;
                    }
                    m = p.x < p.Next.x ? p : p.Next;
                }
            }
            p = p.Next;
        } while (p != outerNode);

        if (m == null) return null;

        if (hx == qx) return m.Prev;

        Node<int> stop = m;
        double tanMin = double.PositiveInfinity;
        double tanCur;

        p = m.Next;
        double mx = m.x;
        double my = m.y;

        while (p != stop)
        {
            if (hx >= p.x && p.x >= mx && hx != p.x &&
                PointInTriangle(hy < my ? hx : qx, hy, mx, my, hy < my ? qx : hx, hy, p.x, p.y))
            {
                tanCur = Math.Abs(hy - p.y) / (hx - p.x);

                if ((tanCur < tanMin || tanCur == tanMin && p.x > m.x) && LocallyInside(p, hole))
                {
                    m = p;
                    tanMin = tanCur;
                }
            }
            p = p.Next;
        }

        return m;
    }

    private void IndexCurve(Node<int> start)
    {
        Node<int> p = start;

        do
        {
            p.Z = p.Z != 0 ? p.Z : ZOrder(p.x, p.y);
            p.PrevZ = p.Prev;
            p.NextZ = p.Next;
            p = p.Next;
        } while (p != start);

        p.PrevZ.NextZ = null;
        p.PrevZ = null;

        SortLinked(p);
    }

    private Node<int> SortLinked(Node<int> list)
    {
        Node<int> p;
        Node<int> q;
        Node<int> e;
        Node<int> tail;
        int numMerges, pSize, qSize;
        int inSize = 1;

        for (; ; )
        {
            p = list;
            list = null;
            tail = null;
            numMerges = 0;

            while (p != null)
            {
                numMerges++;
                q = p;
                pSize = 0;
                for (int i = 0; i < inSize; i++)
                {
                    pSize++;
                    q = q.NextZ;
                    if (q == null) break;
                }

                qSize = inSize;

                while (pSize > 0 || qSize > 0 && q != null)
                {
                    if (pSize == 0)
                    {
                        e = q;
                        q = q.NextZ;
                        qSize--;
                    }
                    else if (qSize == 0 || q == null)
                    {
                        e = p;
                        p = p.NextZ;
                        pSize--;
                    }
                    else if (p.Z <= q.Z)
                    {
                        e = p;
                        p = p.NextZ;
                        pSize--;
                    }
                    else
                    {
                        e = q;
                        q = q.NextZ;
                        qSize--;
                    }

                    if (tail != null) tail.NextZ = e;
                    else list = e;

                    e.PrevZ = tail;
                    tail = e;
                }

                p = q;
            }

            tail.NextZ = null;

            if (numMerges <= 1) return list;

            inSize *= 2;
        }
    }

    private int ZOrder(double x_, double y_)
    {
        int x = (int)(32767.0 * (x_ - MinX) * InvSize);
        int y = (int)(32767.0 * (y_ - MinY) * InvSize);

        x = (x | x << 8) & 0x00FF00FF;
        x = (x | x << 4) & 0x0F0F0F0F;
        x = (x | x << 2) & 0x33333333;
        x = (x | x << 1) & 0x55555555;

        y = (y | y << 8) & 0x00FF00FF;
        y = (y | y << 4) & 0x0F0F0F0F;
        y = (y | y << 2) & 0x33333333;
        y = (y | y << 1) & 0x55555555;

        return x | y << 1;
    }

    private Node<int> GetLeftmost(Node<int> start)
    {
        Node<int> p = start;
        Node<int> leftmost = start;
        do
        {
            if (p.x < leftmost.x) leftmost = p;
            p = p.Next;
        } while (p != start);

        return leftmost;
    }

    private bool PointInTriangle(double ax, double ay, double bx, double by, double cx, double cy, double px, double py)
    {
        return (cx - px) * (ay - py) - (ax - px) * (cy - py) >= 0 &&
               (ax - px) * (by - py) - (bx - px) * (ay - py) >= 0 &&
               (bx - px) * (cy - py) - (cx - px) * (by - py) >= 0;
    }

    private bool IsValidDiagonal(Node<int> a, Node<int> b)
    {
        return a.Next.i != b.i && a.Prev.i != b.i && !IntersectsPolygon(a, b) &&
               LocallyInside(a, b) && LocallyInside(b, a) && MiddleInside(a, b);
    }

    private double Area(Node<int> p, Node<int> q, Node<int> r)
    {
        return (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
    }

    public bool EqualsEarcut(Node<int> p1, Node<int> p2)
    {
        return p1.x == p2.x && p1.y == p2.y;
    }

    private bool Intersects(Node<int> p1, Node<int> q1, Node<int> p2, Node<int> q2)
    {
        if (EqualsEarcut(p1, q1) && EqualsEarcut(p2, q2) || EqualsEarcut(p1, q2) && EqualsEarcut(p2, q1)) return true;
        return Area(p1, q1, p2) > 0 != Area(p1, q1, q2) > 0 &&
               Area(p2, q2, p1) > 0 != Area(p2, q2, q1) > 0;
    }

    private bool IntersectsPolygon(Node<int> a, Node<int> b)
    {
        Node<int> p = a;
        do
        {
            if (p.i != a.i && p.Next.i != a.i && p.i != b.i && p.Next.i != b.i &&
                Intersects(p, p.Next, a, b)) return true;
            p = p.Next;
        } while (p != a);

        return false;
    }

    private bool LocallyInside(Node<int> a, Node<int> b)
    {
        return Area(a.Prev, a, a.Next) < 0
            ? Area(a, b, a.Next) >= 0 && Area(a, a.Prev, b) >= 0
            : Area(a, b, a.Prev) < 0 || Area(a, a.Next, b) < 0;
    }

    private bool MiddleInside(Node<int> a, Node<int> b)
    {
        Node<int> p = a;
        bool inside = false;
        double px = (a.x + b.x) / 2;
        double py = (a.y + b.y) / 2;
        do
        {
            if (p.y > py != p.Next.y > py && p.Next.y != p.y &&
                px < (p.Next.x - p.x) * (py - p.y) / (p.Next.y - p.y) + p.x)
            {
                inside = !inside;
            }
            p = p.Next;
        } while (p != a);

        return inside;
    }

    private Node<int> SplitPolygon(Node<int> a, Node<int> b)
    {
        Node<int> a2 = new Node<int>(a.i, a.x, a.y);
        Node<int> b2 = new Node<int>(b.i, b.x, b.y);
        Node<int> an = a.Next;
        Node<int> bp = b.Prev;

        a.Next = b;
        b.Prev = a;

        a2.Next = an;
        an.Prev = a2;

        b2.Next = a2;
        a2.Prev = b2;

        bp.Next = b2;
        b2.Prev = bp;

        return b2;
    }

    private Node<int> InsertNode(int i, Vertex2 pt, Node<int> last)
    {
        var p = new Node<int>(i, pt.X, pt.Y);

        if (last == null)
        {
            p.Prev = p;
            p.Next = p;
        }
        else
        {
            p.Next = last.Next;
            p.Prev = last;
            last.Next.Prev = p;
            last.Next = p;
        }

        return p;
    }

    private void RemoveNode(Node<int> node)
    {
        if (node.Next == node)
        {
            Nodes.Clear();
        }
        else
        {
            node.Prev.Next = node.Next;
            node.Next.Prev = node.Prev;
        }
    }
}
