﻿using Clipper2Lib;
using GerberParser.Abstracts.NcDrill;
using GerberParser.Core.ClipperPath;
using GerberParser.Enums;
using GerberParser.Property;
using GerberParser.Property.Drill;
using System.Text;
using Path = GerberParser.Core.ClipperPath.Path;

namespace GerberParser.Core.NCDRILL;

internal class NCDrill : NCDrillBase
{
    public NCDrill(StreamReader s, bool defaultPlated)
    {
        parseState = ParseState.PRE_HEADER;
        plated = defaultPlated;
        fmt.ConfigureFormat(4, 3);
        fmt.ConfigureMM();
        pos = new Point64(0, 0);
        routMode = RoutMode.DRILL;

        bool terminated = false;
        var sb = new StringBuilder();

        while (!s.EndOfStream)
        {
            char c = (char)s.Read();

            if (c == '\n')
            {
                terminated = !Command(sb.ToString());
                if (terminated) break;
                sb.Clear();
            }
            else if (char.IsWhiteSpace(c))
            {
                continue;
            }
            else
            {
                sb.Append(c);
            }
        }

        if (!terminated)
        {
            throw new InvalidOperationException("unterminated NC drill file");
        }
    }

    public override Paths64 GetPaths(bool plated = true, bool unplated = true)
    {
        Paths64 paths = new Paths64();

        if (plated)
        {
            if (unplated)
            {
                var clipper = new Clipper64();
                clipper.AddSubject(plotPth.GetDark());
                clipper.AddClip(plotNpth.GetDark());
                clipper.Execute(ClipType.Union, FillRule.Positive, paths, new Paths64());
            }
            else
            {
                paths = plotPth.GetDark();
            }
        }
        else if (unplated)
        {
            paths = plotNpth.GetDark();
        }

        paths = Clipper.ReversePaths(paths);
        return paths;
    }

    protected override void AddArc(Point64 start, Point64 end, long radius, bool ccw)
    {
        double x0 = start.X;
        double y0 = start.Y;
        double x1 = end.X;
        double y1 = end.Y;
        double r = radius;

        double d = Math.Sqrt(Math.Pow(x0 - x1, 2) + Math.Pow(y0 - y1, 2));
        double e = 2.0 * r / d;
        e = (e < 1.0) ? 0.0 : Math.Sqrt(e * e - 1.0) * (ccw ? 1 : -1);

        double ax = (x0 - x1) / 2;
        double ay = (y0 - y1) / 2;
        double xc = ax + ay * e;
        double yc = ay - ax * e;

        double a0 = Math.Atan2(y0 - yc, x0 - xc);
        double a1 = Math.Atan2(y1 - yc, x1 - xc);
        if (ccw && a1 < a0) a1 += 2.0 * Math.PI;
        if (!ccw && a0 < a1) a0 += 2.0 * Math.PI;

        double epsilon = fmt.GetMaxDeviation(); 
        double f = (r > epsilon) ? (1.0 - epsilon / r) : 0.0;
        double th = Math.Acos(2.0 * f * f - 1.0) + 1e-3;
        int nVertices = (int)Math.Ceiling(Math.Abs(a1 - a0) / th);

        for (int i = 1; i <= nVertices; i++)
        {
            double f1 = (double)i / nVertices;
            double f0 = 1.0 - f1;
            double va = f0 * a0 + f1 * a1;
            double vx = xc + r * Math.Cos(va);
            double vy = yc + r * Math.Sin(va);
            path.Add(new Point64((long)Math.Round(vx), (long)Math.Round(vy)));
        }
    }

    protected override bool Command(string cmd)
    {
        if (string.IsNullOrEmpty(cmd))
        {
            return true;
        }

        if (parseState == ParseState.PRE_HEADER)
        {
            if (cmd[0] == ';') return true;

            if (cmd == "M48")
            {
                parseState = ParseState.HEADER;
                return true;
            }
        }
        else if (parseState == ParseState.HEADER)
        {
            if (cmd[0] == ';')
            {
                if (cmd.Length == 16 && cmd.StartsWith(";FILE_FORMAT=") && cmd[14] == ':')
                {
                    fmt.ConfigureFormat(int.Parse(cmd[13].ToString()), int.Parse(cmd[15].ToString()));
                }

                if (cmd == ";TYPE=PLATED") plated = true;
                if (cmd == ";TYPE=NON_PLATED") plated = false;

                return true;
            }

            if (cmd == "FMAT,2") return true;
            if (cmd == "VER,1") throw new InvalidOperationException("Version 1 excellon is not supported");

            if (cmd.StartsWith("METRIC"))
            {
                fmt.ConfigureMM();
                fmt.ConfigureTrailingZeros(cmd.EndsWith(",LZ"));
                return true;
            }
            if (cmd.StartsWith("INCH"))
            {
                fmt.ConfigureInch();
                fmt.ConfigureTrailingZeros(cmd.EndsWith(",LZ"));
                return true;
            }

            if (cmd == "%" || cmd == "M95")
            {
                parseState = ParseState.BODY;
                return true;
            }

            var paramsDict = ParseRegularCommand(cmd);

            if (paramsDict.TryGetValue('T', out var toolStr))
            {
                int toolNo = int.Parse(toolStr);
                if (!paramsDict.TryGetValue('C', out var diameter))
                    throw new InvalidOperationException("missing tool diameter in " + cmd);

                tools[toolNo] = new Tool(fmt.ParseFloat(diameter), plated);
                return true;
            }
        }
        else if (parseState == ParseState.BODY)
        {
            if (cmd[0] == ';') return true;

            var paramsDict = ParseRegularCommand(cmd);

            var startPoint = pos;
            bool coordSet = false;
            if (paramsDict.TryGetValue('X', out var xStr))
            {
                pos.X = fmt.ParseFixed(xStr);
                coordSet = true;
            }
            if (paramsDict.TryGetValue('Y', out var yStr))
            {
                pos.Y = fmt.ParseFixed(yStr);
                coordSet = true;
            }
            var endPoint = pos;

            if (paramsDict.TryGetValue('T', out var tStr))
            {
                int t = int.Parse(tStr);
                if (routMode == RoutMode.ROUT_TOOL_DOWN)
                    throw new InvalidOperationException("unexpected tool change; tool is down");

                if (t == 0)
                {
                    tool = null;
                    return true;
                }
                if (!tools.TryGetValue(t, out tool))
                    throw new InvalidOperationException("attempting to change to undefined tool: " + t);
                return true;
            }

            if (paramsDict.TryGetValue('G', out var gStr))
            {
                int g = int.Parse(gStr);

                switch (g)
                {
                    case 0:
                        routMode = RoutMode.ROUT_TOOL_UP;
                        return true;

                    case 1:
                        if (routMode == RoutMode.ROUT_TOOL_DOWN) path.Add(endPoint);
                        return true;

                    case 2:
                    case 3:
                        bool ccw = g == 3;
                        if (routMode == RoutMode.ROUT_TOOL_DOWN)
                        {
                            if (!paramsDict.TryGetValue('A', out var aStr))
                                throw new InvalidOperationException("arc radius is missing for G0" + g);
                            AddArc(startPoint, endPoint, fmt.ParseFixed(aStr), ccw);
                        }
                        return true;

                    case 5:
                        if (routMode == RoutMode.ROUT_TOOL_DOWN)
                            throw new InvalidOperationException("unexpected G05; cannot exit route mode with tool down");
                        routMode = RoutMode.DRILL;
                        return true;

                    case 85:
                        var subCmd = cmd[..cmd.IndexOf('G')];
                        paramsDict = ParseRegularCommand(subCmd);
                        if (paramsDict.TryGetValue('X', out xStr)) pos.X = fmt.ParseFixed(xStr);
                        if (paramsDict.TryGetValue('Y', out yStr)) pos.Y = fmt.ParseFixed(yStr);
                        startPoint = pos;

                        paramsDict = ParseRegularCommand(cmd[(cmd.IndexOf('G')..)]);
                        if (paramsDict.TryGetValue('X', out xStr)) pos.X = fmt.ParseFixed(xStr);
                        if (paramsDict.TryGetValue('Y', out yStr)) pos.Y = fmt.ParseFixed(yStr);
                        endPoint = pos;

                        if (routMode != RoutMode.DRILL)
                            throw new InvalidOperationException("unexpected G85 in rout mode");

                        path.Add(startPoint);
                        path.Add(endPoint);
                        CommitPath();
                        return true;

                    case 90:
                        return true;

                    default:
                        throw new InvalidOperationException("unsupported G command: " + cmd);
                }
            }

            if (paramsDict.TryGetValue('M', out var mStr))
            {
                int m = int.Parse(mStr);

                switch (m)
                {
                    case 15:
                        if (routMode == RoutMode.ROUT_TOOL_DOWN)
                            throw new InvalidOperationException("unexpected M15; tool already down");
                        if (routMode == RoutMode.DRILL)
                            throw new InvalidOperationException("unexpected M15; not in rout mode");

                        routMode = RoutMode.ROUT_TOOL_DOWN;
                        path.Add(endPoint);
                        return true;

                    case 16:
                        if (routMode == RoutMode.ROUT_TOOL_UP)
                            throw new InvalidOperationException("unexpected M16; tool already up");
                        if (routMode == RoutMode.DRILL)
                            throw new InvalidOperationException("unexpected M16; not in rout mode");

                        routMode = RoutMode.ROUT_TOOL_UP;
                        CommitPath();
                        return true;

                    case 17:
                        return true;

                    case 30:
                        if (routMode == RoutMode.ROUT_TOOL_DOWN)
                            throw new InvalidOperationException("end of file with routing tool down");
                        return false;
                }
            }

            if (coordSet)
            {
                path.Add(endPoint);
                CommitPath();
                return true;
            }
        }

        throw new InvalidOperationException("unknown/unexpected command: " + cmd);
    }

    protected override void CommitPath()
    {
        Paths64 point64s = new Paths64();
        point64s.Add(path);

        if (tool == null)
        {
            throw new InvalidOperationException("tool use before any tool is selected");
        }

        if (tool.plated)
        {
            plotPth.DrawPaths(Path.Render(point64s, tool.diameter, false, fmt.BuildClipperOffset()));
            vias.Add(new Via(path, tool.diameter));
        }
        else
        {
            plotNpth.DrawPaths(Path.Render(point64s, tool.diameter, false, fmt.BuildClipperOffset()));
        }

        path.Clear();
    }

    protected override Dictionary<char, string> ParseRegularCommand(string cmd)
    {
        var parameters = new Dictionary<char, string>();
        char code = '\0';
        int start = 0;

        for (int i = 0; i <= cmd.Length; i++)
        {
            char c = i < cmd.Length ? cmd[i] : '\0';

            if (i == cmd.Length || char.IsLetter(c))
            {
                if (i > start)
                {
                    parameters[code] = cmd.Substring(start, i - start);
                }
                code = c;
                start = i + 1;
            }
        }

        return parameters;
    }
}