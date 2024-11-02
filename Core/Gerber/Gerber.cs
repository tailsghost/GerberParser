using Clipper2Lib;
using GerberParser.Abstracts.Aperture;
using GerberParser.Abstracts.GERBER;
using GerberParser.Abstracts.PLOT;
using GerberParser.Core.Aperture;
using GerberParser.Core.ClipperPath;
using GerberParser.Core.PlotCore;
using GerberParser.Enums;
using GerberParser.Helpers;
using Path = GerberParser.Core.ClipperPath.Path;

namespace GerberParser.Core.Gerber;

public class Gerber : GerberBase
{
    public Gerber(Stream stream) : base(stream)
    {
    }

    public override Paths64 GetOutlinePaths()
    {
        double epsilon = 0.001;

        if (outlineConstructed) return outline;

        var pointMap = new Dictionary<(double, double), List<int>>();
        var points = new HashSet<List<int>>();
        var endPoints = new List<(List<int>, List<int>)>();
        double eps = fmt.GetMaxDeviation();
        double epsSqr = eps * eps;

        foreach (var path in outline)
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

        var paths = new Paths64();

        while (points.Count > 0)
        {
            var cur = points.First();
            if (cur.Count != 2)
            {
                points.Remove(cur);
                continue;
            }

            bool isLoop = true;
            var path = new Path64();
            int startIdx = cur.First();
            int curIdx = cur.Last();

            while (true)
            {
                points.Remove(cur);
                var endpts = endPoints[curIdx];
                var section = outline[curIdx];

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
                if (Clipper.Area(path) < 0)
                {
                    Clipper.ReversePath(path);
                }
                paths.Add(path);
            }
        }

        //Необходимо использовать в дальнейшем Execute для EvenOdd
        paths = Clipper.SimplifyPaths(paths, epsilon, true);
        outlineConstructed = true;
        outline = paths;
        return outline;
    }


    public override Paths64 GetPaths()
    {
        return plotStack.Peek().GetDark();
    }

    protected override bool Command(string cmd, bool isAttrib)
    {
        if (amBuilder != null)
        {
            amBuilder.Append(cmd);
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
                    throw new Exception("Invalid aperture definition: " + cmd);

                int index = int.Parse(cmd.Substring(3, cmd.Length - 3));
                if (index < 10)
                    throw new Exception("Aperture index out of range: " + cmd);

                var csep = new List<string>(cmd.Substring(cmd.IndexOf(",") + 1).Split(new[] { ',', 'X' }));
                if (csep.Count == 0)
                    throw new Exception("Invalid aperture definition: " + cmd);

                switch (csep[0])
                {
                    case "C":
                        apertures[index] = new Circle(csep, fmt);
                        break;
                    case "R":
                        apertures[index] = new Aperture.Rectangle(csep, fmt);
                        break;
                    case "O":
                        apertures[index] = new Aperture.Obround(csep, fmt);
                        break;
                    case "P":
                        apertures[index] = new Aperture.Polygon(csep, fmt);
                        break;
                    default:
                        if (!apertureMacros.TryGetValue(csep[0], out var macro))
                            throw new Exception("Unsupported aperture type: " + csep[0]);
                        apertures[index] = macro.Build(csep, fmt);
                        break;
                }
                return true;
            }

            if (cmd.StartsWith("AM"))
            {
                var name = cmd.Substring(2);
                amBuilder = new ApertureMacro();
                apertureMacros[name] = amBuilder;
                return true;
            }

            if (cmd.StartsWith("AB"))
            {
                if (cmd == "AB")
                {
                    if (plotStack.Count <= 1)
                        throw new Exception("Unmatched aperture block close command");
                    plotStack.Pop();
                }
                else
                {
                    int index = int.Parse(cmd.Substring(3));
                    if (index < 10)
                        throw new Exception("Aperture index out of range: " + cmd);

                    var plot = new Plot();
                    plotStack.Push(plot);
                    apertures[index] = new Aperture.Custom(plot);
                }
                return true;
            }

            if (cmd.StartsWith("LP"))
            {
                if (cmd.Length != 3 || (cmd[2] != 'C' && cmd[2] != 'D'))
                    throw new Exception("Invalid polarity command: " + cmd);

                polarity = cmd[2] == 'D';
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
                if (!apertures.TryGetValue(int.Parse(apCmd.Substring(1)), out aperture))
                    throw new Exception("Undefined aperture selected");
                return true;
            }

            if (cmd.StartsWith("X") || cmd.StartsWith("Y") || cmd.StartsWith("I") || cmd.StartsWith("D"))
            {
                var parameters = new Dictionary<char, long> { { 'X', pos.X }, { 'Y', pos.Y }, { 'I', 0 }, { 'J', 0 } };
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
                        Interpolate(new Point64 { X = parameters['X'], Y = parameters['Y'] }, new Point64 { X = parameters['I'], Y = parameters['J'] });
                        pos.X = parameters['X'];
                        pos.Y = parameters['Y'];
                        break;
                    case 2:
                        if (regionMode) CommitRegion();
                        pos.X = parameters['X'];
                        pos.Y = parameters['Y'];
                        break;
                    case 3:
                        if (regionMode) throw new Exception("Cannot flash in region mode");
                        pos.X = parameters['X'];
                        pos.Y = parameters['Y'];
                        DrawAperture();
                        break;
                    default:
                        throw new Exception("Invalid draw/move command: " + d);
                }
                return true;
            }

            if (cmd == "G36")
            {
                if (regionMode) throw new Exception("Already in region mode");
                regionMode = true;
                return true;
            }
            if (cmd == "G37")
            {
                if (!regionMode) throw new Exception("Not in region mode");
                CommitRegion();
                regionMode = false;
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
        if (regionAccum.Count < 3)
        {
            throw new InvalidOperationException("Encountered region with less than 3 vertices");
        }

        if (Clipper.Area(regionAccum) < 0)
        {
            regionAccum.Reverse();
        }

        plotStack.Peek().DrawPaths(new List<Path64> { regionAccum }, polarity);
        regionAccum.Clear();
    }

    protected override void DrawAperture()
    {
        if (aperture == null)
        {
            throw new InvalidOperationException("Flash command before aperture set");
        }
        plotStack.Peek().DrawPlot(
            aperture.plot, polarity, pos.X, pos.Y, apMirrorX, apMirrorY, apRotate, apScale);
    }

    protected override void EndAttrib()
    {
        throw new NotImplementedException();
    }

    protected override void Interpolate(Point64 dest, Point64 center)
    {
        Path64 path;

        if (imode == InterpolationMode.UNDEFINED)
        {
            throw new InvalidOperationException("Interpolate command before mode set");
        }
        else if (imode == InterpolationMode.LINEAR)
        {
            path = new Path64 { pos, dest };
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
                h = new CircularInterpolationHelper(pos, dest, new Point64(pos.X + center.X, pos.Y + center.Y), ccw, true);
            }
            else
            {
                for (int k = 0; k < 4; k++)
                {
                    var h2 = new CircularInterpolationHelper(
                        pos, dest,
                        new Point64(
                            pos.X + ((k & 1) == 1 ? center.X : -center.X),
                            pos.Y + ((k & 2) == 2 ? center.Y : -center.Y)
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

        if (polarity && plotStack.Count == 1)
        {
            outline.Add(path);
        }

        if (regionMode)
        {
            regionAccum.AddRange(path.Skip(1));
            return;
        }

        if (aperture == null)
        {
            throw new InvalidOperationException("Interpolate command before aperture set");
        }

        if (!aperture.IsSimpleCircle(out long? diameter))
        {
            throw new InvalidOperationException("Only simple circle apertures without a hole are supported for interpolation");
        }

        double thickness = (double)(diameter * apScale);
        if (thickness == 0) return;

        Paths64 paths = Path.Render(new Paths64 { path }, thickness, false, fmt.BuildClipperOffset());
        plotStack.Peek().DrawPaths(paths, polarity);
    }
}
