namespace GerberParser.Property.Net;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using ClipperLib;

public class Netlist
{
    public int numLayers { get; set; }
    public PhysicalNetlist connectedNetlist { get; set; }
    public PhysicalNetlist clearanceNetlist { get; set; }
    public List<string> builderViolations { get; set; } = new List<string>();
    public Dictionary<string, LogicalNet> logicalNets { get; set; } = new Dictionary<string, LogicalNet>();

    private double PointToLineDistanceSqr(IntPoint point, IntPoint a, IntPoint b)
    {
        double ax = (double)a.X;
        double ay = (double)a.Y;
        double bx = (double)b.X;
        double by = (double)b.Y;
        double px = (double)point.X;
        double py = (double)point.Y;

        double A = px - ax;
        double B = py - ay;
        double C = bx - ax;
        double D = by - ay;

        double dot = A * C + B * D;
        double lenSq = C * C + D * D;
        double param = dot / lenSq;

        double xx, yy;

        if (param < 0 || (ax == bx && ay == by))
        {
            xx = ax;
            yy = ay;
        }
        else if (param > 1)
        {
            xx = bx;
            yy = by;
        }
        else
        {
            xx = ax + param * C;
            yy = ay + param * D;
        }

        double dx = px - xx;
        double dy = py - yy;

        return dx * dx + dy * dy;
    }

    private double PointToPathDistanceSqr(IntPoint point, Polygon p, bool closed)
    {
        double rSqrMin = double.PositiveInfinity;

        if (closed)
        {
            rSqrMin = Math.Min(rSqrMin, PointToLineDistanceSqr(point, p[0], p[p.Count - 1]));
        }
        for (int i = 1; i < p.Count; i++)
        {
            rSqrMin = Math.Min(rSqrMin, PointToLineDistanceSqr(point, p[i - 1], p[i]));
        }
        return rSqrMin;
    }

    public double ComputeAnnularRing(Via via, PhysicalNet net)
    {
        double rSqrMin = double.PositiveInfinity;

        foreach (var vc in via.path)
        {
            foreach (var shape in net.shapes)
            {
                rSqrMin = Math.Min(rSqrMin, PointToPathDistanceSqr(vc, shape.outline, true));
                foreach (var hole in shape.holes)
                {
                    rSqrMin = Math.Min(rSqrMin, PointToPathDistanceSqr(vc, hole, true));
                }
            }
        }

        if (via.path.Count > 1)
        {
            foreach (var shape in net.shapes)
            {
                foreach (var pt in shape.outline)
                {
                    rSqrMin = Math.Min(rSqrMin, PointToPathDistanceSqr(pt, via.path, false));
                }
                foreach (var hole in shape.holes)
                {
                    foreach (var pt in hole)
                    {
                        rSqrMin = Math.Min(rSqrMin, PointToPathDistanceSqr(pt, via.path, false));
                    }
                }
            }
        }

        return Math.Sqrt(rSqrMin) - (double)via.finishedHoleSize / 2;
    }


    public List<string> PerformDRC(int annularRing)
    {
        var violations = new List<string>(builderViolations);

        foreach (var entry in logicalNets)
        {
            var net = entry.Value;
            var pnets = net.connectedNets;

            if (!pnets.Any())
            {
                violations.Add($"logical net {net.name} is completely unrouted");
            }
            else if (pnets.Count > 1)
            {
                violations.Add($"logical net {net.name} is divided up into {pnets.Count} islands");
            }
        }

        var shortsReported = new HashSet<(string, string)>();

        foreach (var net in connectedNetlist.nets)
        {
            var lnets = net.logicalNets;
            if (lnets.Count < 2) continue;

            foreach (var a in lnets)
            {
                string netA = a.name;
                foreach (var b in lnets)
                {
                    string netB = b.name;
                    if (netA == netB || shortsReported.Contains((netA, netB))) continue;

                    shortsReported.Add((netA, netB));
                    shortsReported.Add((netB, netA));

                    violations.Add($"logical nets {netA} and {netB} are short-circuited");
                }
            }
        }

        foreach (var net in clearanceNetlist.nets)
        {
            var lnets = net.logicalNets;
            if (lnets.Count < 2) continue;

            foreach (var a in lnets)
            {
                string netA = a.name;
                foreach (var b in lnets)
                {
                    string netB = b.name;
                    if (netA == netB || shortsReported.Contains((netA, netB))) continue;

                    shortsReported.Add((netA, netB));
                    shortsReported.Add((netB, netA));

                    violations.Add($"clearance violation between {netA} and {netB}");
                }
            }
        }

        foreach (var net in connectedNetlist.nets)
        {
            foreach (var via in net.vias)
            {
                double viaAnnularRing = ComputeAnnularRing(via, net);

                if (viaAnnularRing < annularRing)
                {
                    violations.Add($"via at coordinate ({via.GetCoordinate().X}, {via.GetCoordinate().Y}) has annular ring " +
                                   $"{viaAnnularRing}, less than the minimum {annularRing}");
                }
            }
        }

        return violations;
    }
}
