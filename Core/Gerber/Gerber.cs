using GerberParser.Abstracts.GERBER;
using GerberParser.Core.Aperture;
using GerberParser.Core.ClipperPath;
using GerberParser.Core.PlotCore;
using GerberParser.Enums;
using GerberParser.Helpers;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;
using Polygon = System.Collections.Generic.List<ClipperLib.IntPoint>;
using ClipperLib;

namespace GerberParser.Core.GERBER;

public class Gerber : GerberBase
{
    public Gerber(StringReader stream) : base(stream)
    {
    }

    public override Polygons GetOutlinePaths()
    {
        //Метод полностью протестирован, на 100% работает!!!

        if (OutlineConstructed) return Outlines;

        var pointMap = new Dictionary<(double, double), List<int>>();
        var points = new HashSet<List<int>>();
        var endPoints = new List<(List<int>, List<int>)>();
        double eps = fmt.GetMaxDeviation();
        double epsSqr = eps * eps;

        foreach (var path in Outlines)
        {
            var endpts = (first: (List<int>)null, second: (List<int>)null);

            for (int endpt = 0; endpt < 2; endpt++)
            {
                var cFix = endpt == 0 ? path.First() : path.Last();
                var c = ((double)cFix.X, (double)cFix.Y);

                if (!pointMap.TryGetValue(c, out var existingList))
                {
                    var nearest = pointMap.Where(kvp =>
                        Math.Pow(c.Item1 - kvp.Key.Item1, 2) + Math.Pow(c.Item2 - kvp.Key.Item2, 2) < epsSqr)
                        .OrderBy(kvp =>
                            Math.Pow(c.Item1 - kvp.Key.Item1, 2) + Math.Pow(c.Item2 - kvp.Key.Item2, 2))
                        .FirstOrDefault();

                    if (nearest.Equals(default(KeyValuePair<(double, double), List<int>>)))
                    {
                        var ep = new List<int>();
                        pointMap[c] = ep;
                        points.Add(ep);
                        existingList = ep;
                    }
                    else
                    {
                        existingList = nearest.Value;
                    }
                }

                existingList.Add(endPoints.Count);
                if (endpt == 0) endpts.first = existingList;
                else endpts.second = existingList;
            }

            endPoints.Add(endpts);
        }

        var paths = new Polygons();

        while (points.Count > 0)
        {
            var cur = points.First();
            if (cur.Count != 2)
            {
                points.Remove(cur);
                continue;
            }

            bool isLoop = true;
            var path = new Polygon();
            int startIdx = cur.First();
            int curIdx = cur.Last();

            while (true)
            {
                points.Remove(cur);
                var endpts = endPoints[curIdx];
                var section = Outlines[curIdx];

                if (endpts.Item1 == cur)
                {
                    path.AddRange(section.Take(section.Count - 1));
                    cur = endpts.Item2;
                }
                else if (endpts.Item2 == cur)
                {
                    var reversedSection = section.ToList();
                    reversedSection.Reverse();
                    path.AddRange(reversedSection.Take(reversedSection.Count - 1));
                    cur = endpts.Item1;
                }
                else
                {
                    throw new Exception("Unexpected path error");
                }

                if (curIdx == startIdx) break;
                if (cur.Count != 2)
                {
                    isLoop = false;
                    break;
                }

                curIdx = cur[0] == curIdx ? cur[1] : cur[0];
            }

            if (isLoop)
            {
                var pathsPoly = new Polygons();
                pathsPoly.Add(path);
                if (Clipper.Area(path) < 0)
                {
                    Clipper.ReversePaths(pathsPoly);
                }
                paths.Add(pathsPoly[0]);
            }
        }
        paths = Clipper.SimplifyPolygons(paths);

        OutlineConstructed = true;
        Outlines = paths;
        return Outlines;
    }


    public override Polygons GetPaths()
    {
        var resultPeek = PlotStack.Peek();
        var result = resultPeek.GetDark();

        return result;
    }

    protected override bool Command(string cmd, bool isAttrib)
    {
        if (AmBuilder != null)
        {
            AmBuilder.Append(cmd);
            return true;
        }
        else if (isAttrib)
        {
            if (cmd.StartsWith("FS"))
            {
                if (cmd.Length != 10 || cmd.Substring(2, 3) != "LAX" || cmd[7] != 'Y' || cmd.Substring(5, 2) != cmd.Substring(8, 2))
                    throw new Exception("Invalid or deprecated and unsupported format specification: " + cmd);

                fmt.ConfigureFormat(int.Parse(cmd.Substring(5, 1)), int.Parse(cmd.Substring(6, 1)));
                return true;
            }

            if (cmd.StartsWith("MO"))
            {
                if (cmd.Substring(2, 2) == "IN")
                    fmt.ConfigureInch();
                else if (cmd.Substring(2, 2) == "MM")
                    fmt.ConfigureMM();
                else
                    throw new Exception("Invalid unit specification: " + cmd);

                return true;
            }

            if (cmd.StartsWith("AD"))
            {
                if (cmd.Length < 3 || cmd[2] != 'D')
                {
                    throw new Exception("Invalid aperture definition: " + cmd);
                }

                int i = 3;
                int start = i;

                while (i < cmd.Length && char.IsDigit(cmd[i]))
                {
                    i++;
                }

                if (!int.TryParse(cmd.Substring(start, i - start), out int index) || index < 10)
                {
                    throw new Exception("Aperture index out of range: " + cmd);
                }

                List<string> csep = new List<string>();
                start = i;

                while (i < cmd.Length)
                {
                    if (cmd[i] == ',' || (csep.Count > 0 && cmd[i] == 'X'))
                    {
                        csep.Add(cmd.Substring(start, i - start));
                        start = i + 1;
                    }
                    i++;
                }
                csep.Add(cmd.Substring(start, i - start));

                if (csep.Count == 0)
                {
                    throw new Exception("Invalid aperture definition: " + cmd);
                }

                switch (csep[0])
                {
                    case "C":
                        Apertures[index] = new Circle(csep, fmt);
                        break;
                    case "R":
                        Apertures[index] = new Rectangle(csep, fmt);
                        break;
                    case "O":
                        Apertures[index] = new Obround(csep, fmt);
                        break;
                    case "P":
                        Apertures[index] = new Aperture.Polygon(csep, fmt);
                        break;
                    default:
                        if (!ApertureMacros.TryGetValue(csep[0], out var macro))
                        {
                            throw new Exception("Unsupported aperture type: " + csep[0]);
                        }
                        Apertures[index] = macro.Build(csep, fmt);
                        break;
                }
                return true;
            }

            if (cmd.StartsWith("AM"))
            {
                var name = cmd.Substring(2);
                AmBuilder = new ApertureMacro();
                ApertureMacros[name] = AmBuilder;
                return true;
            }

            if (cmd.StartsWith("AB"))
            {
                if (cmd == "AB")
                {
                    if (PlotStack.Count <= 1)
                        throw new Exception("Unmatched aperture block close command");
                    PlotStack.Pop();
                }
                else
                {
                    int index = int.Parse(cmd.Substring(3));
                    if (index < 10)
                        throw new Exception("Aperture index out of range: " + cmd);

                    var plot = new Plot();
                    PlotStack.Push(plot);
                    Apertures[index] = new Custom(plot);
                }
                return true;
            }

            if (cmd.StartsWith("LP"))
            {
                if (cmd.Length != 3 || (cmd[2] != 'C' && cmd[2] != 'D'))
                    throw new Exception("Invalid polarity command: " + cmd);

                Polarity = cmd[2] == 'D';
                return true;
            }

            switch (cmd)
            {
                case "LMN":
                    apMirrorX = false;
                    apMirrorY = false;
                    return true;
                case "LMX":
                    apMirrorX = true;
                    apMirrorY = false;
                    return true;
                case "LMY":
                    apMirrorX = false;
                    apMirrorY = true;
                    return true;
                case "LMXY":
                    apMirrorX = true;
                    apMirrorY = true;
                    return true;
                default:
                    if (cmd.StartsWith("LR"))
                    {
                        apRotate = double.Parse(cmd.Substring(2)) * Math.PI / 180.0;
                        return true;
                    }
                    if (cmd.StartsWith("LS"))
                    {
                        apScale = double.Parse(cmd.Substring(2));
                        return true;
                    }
                    break;
            }
        }
        else
        {
            if (cmd.StartsWith("G04") || cmd == "G54" || cmd == "G55")
                return true;

            switch (cmd)
            {
                case "G01":
                    imode = InterpolationMode.LINEAR;
                    return true;
                case "G02":
                    imode = InterpolationMode.CIRCULAR_CW;
                    return true;
                case "G03":
                    imode = InterpolationMode.CIRCULAR_CCW;
                    return true;
                case "G74":
                    qmode = QuadrantMode.SINGLE;
                    return true;
                case "G75":
                    qmode = QuadrantMode.MULTI;
                    return true;
                default:
                    break;
            }

            string apCmd = cmd;
            if (cmd.StartsWith("G54D") || cmd.StartsWith("G55D"))
                apCmd = apCmd.Substring(3);

            if (apCmd.StartsWith("D") && !apCmd.StartsWith("D0"))
            {
                if (!Apertures.TryGetValue(int.Parse(apCmd.Substring(1)), out Aperture))
                    throw new Exception("Undefined aperture selected");
                return true;
            }

            if (cmd.StartsWith("X") || cmd.StartsWith("Y") || cmd.StartsWith("I") || cmd.StartsWith("D"))
            {
                var parameters = new Dictionary<char, long> { { 'X', Pos.X }, { 'Y', Pos.Y }, { 'I', 0 }, { 'J', 0 } };
                int d = -1;
                char code = ' ';
                int start = 0;

                for (int i = 0; i <= cmd.Length; i++)
                {
                    char c = (i < cmd.Length) ? cmd[i] : 'Z';
                    if (i == cmd.Length || char.IsLetter(c))
                    {
                        if (code == 'D')
                            d = int.Parse(cmd.Substring(start, i - start));
                        else if (code != ' ')
                            parameters[code] = fmt.ParseFixed(cmd.Substring(start, i - start));

                        code = c;
                        start = i + 1;
                    }
                }

                switch (d)
                {
                    case 1:
                        Interpolate(new IntPoint { X = parameters['X'], Y = parameters['Y'] }, new IntPoint { X = parameters['I'], Y = parameters['J'] });
                        Pos.X = parameters['X'];
                        Pos.Y = parameters['Y'];
                        break;
                    case 2:
                        if (RegionMode) CommitRegion();
                        Pos.X = parameters['X'];
                        Pos.Y = parameters['Y'];
                        break;
                    case 3:
                        if (RegionMode) throw new Exception("Cannot flash in region mode");
                        Pos.X = parameters['X'];
                        Pos.Y = parameters['Y'];
                        DrawAperture();
                        break;
                    default:
                        throw new Exception("Invalid draw/move command: " + d);
                }
                return true;
            }

            if (cmd == "G36")
            {
                if (RegionMode) throw new Exception("Already in region mode");
                RegionMode = true;
                return true;
            }
            if (cmd == "G37")
            {
                if (!RegionMode) throw new Exception("Not in region mode");
                CommitRegion();
                RegionMode = false;
                return true;
            }

            if (cmd == "G70")
            {
                fmt.ConfigureInch();
                return true;
            }
            if (cmd == "G71")
            {
                fmt.ConfigureMM();
                return true;
            }
            if (cmd == "G90" || cmd == "G91")
            {
                if (cmd == "G91") throw new Exception("Incremental mode is not supported");
                return true;
            }

            if (cmd == "M00" || cmd == "M01" || cmd == "M02")
                return false;
        }
        return false;
    }

    protected override void CommitRegion()
    {

        if (RegionAccum.Count < 3)
            return;
        

        if (Clipper.Area(RegionAccum) < 0)
        {
            RegionAccum.Reverse();
        }

        PlotStack.Peek().DrawPaths(new Polygons { RegionAccum }, Polarity);
        RegionAccum.Clear();
    }

    protected override void DrawAperture()
    {
        if (Aperture == null)
        {
            throw new InvalidOperationException("Flash command before aperture set");
        }
        PlotStack.Peek().DrawPlot(
            Aperture.Plot, Polarity, Pos.X, Pos.Y, apMirrorX, apMirrorY, apRotate, apScale);
    }

    protected override void EndAttrib()
    {
        AmBuilder = null;
    }

    protected override void Interpolate(IntPoint dest, IntPoint center)
    {
        Polygon path;

        if (imode == InterpolationMode.UNDEFINED)
        {
            throw new InvalidOperationException("Interpolate command before mode set");
        }
        else if (imode == InterpolationMode.LINEAR)
        {
            path = new Polygon { Pos, dest };
        }
        else
        {
            CircularInterpolationHelper h = null;

            bool ccw = imode == InterpolationMode.CIRCULAR_CCW;

            if (qmode == QuadrantMode.UNDEFINED)
            {
                throw new InvalidOperationException("Arc command before quadrant mode set");
            }
            else if (qmode == QuadrantMode.MULTI)
            {
                h = new CircularInterpolationHelper(Pos, dest, new IntPoint(Pos.X + center.X, Pos.Y + center.Y), ccw, true);
            }
            else
            {
                for (int k = 0; k < 4; k++)
                {
                    var h2 = new CircularInterpolationHelper(
                        Pos, dest,
                        new IntPoint(
                            Pos.X + ((k & 1) == 1 ? center.X : -center.X),
                            Pos.Y + ((k & 2) == 2 ? center.Y : -center.Y)
                        ),
                        ccw, false
                    );
                    if (h2.IsSingleQuadrant())
                    {
                        if (h == null || h.Error() > h2.Error())
                        {
                            h = h2;
                        }
                    }
                }
            }

            if (h == null)
            {
                throw new InvalidOperationException("Failed to make circular interpolation");
            }
            path = h.ToPath(fmt.GetMaxDeviation());
        }

        if (Polarity && PlotStack.Count == 1)
        {
            Outlines.Add(path);
        }

        if (RegionMode)
        {
            RegionAccum.AddRange(path.Skip(1));
            return;
        }

        if (Aperture == null)
        {
            throw new InvalidOperationException("Interpolate command before aperture set");
        }

        if (!Aperture.IsSimpleCircle(out long? diameter))
        {
            throw new InvalidOperationException("Only simple circle apertures without a hole are supported for interpolation");
        }

        double thickness = (double)(diameter * apScale);
        if (thickness == 0) return;

        Polygons paths = new Polygons { path }.Render(thickness, false, fmt.BuildClipperOffset());
        PlotStack.Peek().DrawPaths(paths, Polarity);
    }
}
