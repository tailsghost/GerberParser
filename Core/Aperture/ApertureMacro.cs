using Clipper2Lib;
using GerberParser.Abstracts.Aperture;
using GerberParser.Abstracts.APERTURE;
using GerberParser.Core.ClipperPath;
using GerberParser.Core.Coord;
using GerberParser.Core.PlotCore;

namespace GerberParser.Core.Aperture;

public class ApertureMacro : ApertureMacroBase
{
    private readonly List<List<Expression>> cmds = new List<List<Expression>>();

    public override void Append(string cmd)
    {
        if (cmd.StartsWith("$"))
        {
            var parts = cmd.Split('=');
            cmds.Add(new List<Expression> { Expression.Parse(parts[0]), Expression.Parse(parts[1]) });
        }
        else
        {
            var exprList = new List<Expression>();
            foreach (var part in cmd.Split(','))
            {
                exprList.Add(Expression.Parse(part));
            }
            cmds.Add(exprList);
        }
    }

    public override Base Build(List<string> csep, ConcreteFormat fmt)
    {
        var vars = new Dictionary<int, double>();
        for (int i = 1; i < csep.Count; i++)
        {
            vars[i] = double.Parse(csep[i]);
        }

        var plot = new Plot();

        foreach (var cmd in cmds)
        {
            var code = (int)Math.Round(cmd[0].Eval(vars));

            switch (code)
            {
                case 1: 
                    HandleCircle(cmd, vars, plot, fmt);
                    break;
                case 20: 
                    HandleVectorLine(cmd, vars, plot, fmt);
                    break;
                case 21: 
                    HandleCenterLine(cmd, vars, plot, fmt);
                    break;
                case 4: 
                    HandleOutline(cmd, vars, plot, fmt);
                    break;
                case 5: 
                    HandlePolygon(cmd, vars, plot, fmt);
                    break;
                case 6: 
                    HandleMoire(cmd, vars, plot, fmt);
                    break;
                case 7: 
                    HandleThermal(cmd, vars, plot, fmt);
                    break;
                default:
                    throw new Exception("Invalid aperture macro primitive code");
            }
        }

        return new Custom(plot);
    }

    private void HandleCircle(List<Expression> cmd, Dictionary<int, double> vars, Plot plot, ConcreteFormat fmt)
    {

        if (cmd.Count() < 5 || cmd.Count() > 6)
            throw new ArgumentException("Invalid circle command in aperture macro");

        bool exposure = cmd[1].Eval(vars) > 0.5;
        double diameter = Math.Abs(cmd[2].Eval(vars));
        double centerX = cmd[3].Eval(vars);
        double centerY = cmd[4].Eval(vars);
        double rotation = cmd.Count > 5 ? cmd[5].Eval(vars) : 0;

        var paths = new Paths64
        {
            new Path64 { new Point64(fmt.ToFixed(centerX), fmt.ToFixed(centerY)) }
        }.Render(fmt.ToFixed(diameter), false, fmt.BuildClipperOffset());

        plot.DrawPaths(paths, exposure, 0,0,false,false, rotation/ (180*Math.PI));
    }

    private void HandleVectorLine(List<Expression> cmd, Dictionary<int, double> vars, Plot plot, ConcreteFormat fmt)
    {

        if (cmd.Count() < 7 || cmd.Count() > 8)
            throw new ArgumentException("Invalid circle command in aperture macro");

        bool exposure = cmd[1].Eval(vars) > 0.5;
        double width = Math.Abs(cmd[2].Eval(vars));
        double startX = cmd[3].Eval(vars);
        double startY = cmd[4].Eval(vars);
        double endX = cmd[5].Eval(vars);
        double endY = cmd[6].Eval(vars);
        double rotation = cmd.Count > 7 ? cmd[7].Eval(vars) : 0;

        var paths = new Paths64
        {
            new Path64
            {
                new Point64(fmt.ToFixed(startX), fmt.ToFixed(startY)),
                new Point64(fmt.ToFixed(endX), fmt.ToFixed(endY))
            }
        }.Render(fmt.ToFixed(width), true, fmt.BuildClipperOffset());

        plot.DrawPaths(paths, exposure, 0,0,false,false, rotation / (180 * Math.PI));
    }

    private void HandleCenterLine(List<Expression> cmd, Dictionary<int, double> vars, Plot plot, ConcreteFormat fmt)
    {
        if (cmd.Count < 6 || cmd.Count > 7)
            throw new ArgumentException("invalid center line command in aperture macro");

        bool exposure = cmd[1].Eval(vars) > 0.5;
        double width = Math.Abs(cmd[2].Eval(vars));
        double height = Math.Abs(cmd[3].Eval(vars));
        double centerX = cmd[4].Eval(vars);
        double centerY = cmd[5].Eval(vars);
        double rotation = cmd.Count > 6 ? cmd[6].Eval(vars) : 0;

        var paths = new Paths64
        {
            new Path64
            {
                new Point64(fmt.ToFixed(centerX + width * 0.5), fmt.ToFixed(centerY + height * 0.5)),
                new Point64(fmt.ToFixed(centerX - width * 0.5), fmt.ToFixed(centerY + height * 0.5)),
                new Point64(fmt.ToFixed(centerX - width * 0.5), fmt.ToFixed(centerY - height * 0.5)),
                new Point64(fmt.ToFixed(centerX + width * 0.5), fmt.ToFixed(centerY - height * 0.5))
            }
        };

        plot.DrawPaths(paths, exposure, 0,0,false,false,rotation / (180 * Math.PI));
    }

    private void HandleOutline(List<Expression> cmd, Dictionary<int, double> vars, Plot plot, ConcreteFormat fmt)
    {

        if (cmd.Count < 3)
            throw new ArgumentException("Invalid outline command in aperture macro");

        bool exposure = cmd[1].Eval(vars) > 0.5;
        int nVertices = (int)Math.Round(cmd[2].Eval(vars));
        int rotationIndex = 5 + 2 * nVertices;
        double rotation = cmd.Count > (5 + 2 * nVertices) ? cmd.Last().Eval(vars) : 0;

        if (nVertices < 3 || cmd.Count() < rotationIndex || cmd.Count() > rotationIndex + 1)
            throw new ArgumentException("Invalid outline command in aperture macro");

        var paths = new Paths64();

        for (int i = 0; i < nVertices; i++)
        {
            double x = fmt.ToFixed(cmd[3 + 2 * i].Eval(vars));
            double y = fmt.ToFixed(cmd[4 + 2 * i].Eval(vars));
            paths.Add(new Path64 { new Point64(x, y) });
        }

        plot.DrawPaths(paths, exposure, 0,0,false,false,rotation / (180 * Math.PI), 1.0, true, FillRule.NonZero);
    }

    private void HandlePolygon(List<Expression> cmd, Dictionary<int, double> vars, Plot plot, ConcreteFormat fmt)
    {

        if (cmd.Count() < 6 || cmd.Count() > 7)
            throw new ArgumentException("Invalid polygon command in aperture macro");

        bool exposure = cmd[1].Eval(vars) > 0.5;
        int nVertices = (int)Math.Round(cmd[2].Eval(vars));
        double centerX = cmd[3].Eval(vars);
        double centerY = cmd[4].Eval(vars);
        double diameter = Math.Abs(cmd[5].Eval(vars));
        double rotation = cmd.Count > 6 ? cmd[6].Eval(vars) : 0;

        var paths = new Paths64();

        for (int i = 0; i < nVertices; i++)
        {
            double angle = ((double)i / nVertices) * 2.0 * Math.PI;
            double x = centerX + diameter * 0.5 * Math.Cos(angle);
            double y = centerY + diameter * 0.5 * Math.Sin(angle);
            paths.Add(new Path64 { new Point64(fmt.ToFixed(x), fmt.ToFixed(y))});
        }

        plot.DrawPaths(paths, exposure, 0,0,false,false,rotation/(180*Math.PI));
    }

    private void HandleMoire(List<Expression> cmd, Dictionary<int, double> vars, Plot plot, ConcreteFormat fmt)
    {

        if (cmd.Count() < 9 || cmd.Count() > 10)
            throw new ArgumentException("Invalid moire command in aperture macro");

        double centerX = cmd[1].Eval(vars);
        double centerY = cmd[2].Eval(vars);
        double diameter = Math.Abs(cmd[3].Eval(vars));
        double thickness = Math.Abs(cmd[4].Eval(vars));
        double gap = Math.Abs(cmd[5].Eval(vars));
        int maxRings = (int)Math.Round(cmd[6].Eval(vars));
        double chThickness = Math.Abs(cmd[7].Eval(vars));
        double chLength = Math.Abs(cmd[8].Eval(vars));
        double rotation = cmd.Count > 9 ? cmd[9].Eval(vars) : 0;

        var paths = new Paths64();

        for (int i = 0; i < maxRings * 2 && diameter > 0.0; i++)
        {
            var circlePaths = new Paths64
            {
                new Path64 { new Point64(fmt.ToFixed(centerX), fmt.ToFixed(centerY)) }
            }.Render(fmt.ToFixed(diameter), false, fmt.BuildClipperOffset());

            if (i % 2 != 0)
            {
                circlePaths.Reverse();
                diameter -= gap * 2.0;
            }
            else
            {
                diameter -= thickness * 2.0;
            }

            paths.AddRange(circlePaths);
        }

        if (chThickness > 0.0 && chLength > 0.0)
        {
            paths.Add(new Path64
            {
                new Point64(fmt.ToFixed(centerX + chThickness * 0.5), fmt.ToFixed(centerY + chLength * 0.5)),
                new Point64(fmt.ToFixed(centerX - chThickness * 0.5), fmt.ToFixed(centerY + chLength * 0.5)),
                new Point64(fmt.ToFixed(centerX - chThickness * 0.5), fmt.ToFixed(centerY - chLength * 0.5)),
                new Point64(fmt.ToFixed(centerX + chThickness * 0.5), fmt.ToFixed(centerY - chLength * 0.5))
            });

            paths.Add(new Path64
            {
                new Point64(fmt.ToFixed(centerX + chLength * 0.5), fmt.ToFixed(centerY + chThickness * 0.5)),
                new Point64(fmt.ToFixed(centerX - chLength * 0.5), fmt.ToFixed(centerY + chThickness * 0.5)),
                new Point64(fmt.ToFixed(centerX - chLength * 0.5), fmt.ToFixed(centerY - chThickness * 0.5)),
                new Point64(fmt.ToFixed(centerX + chLength * 0.5), fmt.ToFixed(centerY - chThickness * 0.5))
            });
        }

        //Возможно NonZero
        plot.DrawPaths(paths, true, 0,0,false,false,rotation/(180*Math.PI), 1.0, true, FillRule.Positive);
    }

    private void HandleThermal(List<Expression> cmd, Dictionary<int, double> vars, Plot plot, ConcreteFormat fmt)
    {
        if (cmd.Count() < 6 || cmd.Count() > 7)
            throw new ArgumentException("Invalid thermal command in aperture macro");

        double centerX = cmd[1].Eval(vars);
        double centerY = cmd[2].Eval(vars);
        double outer = Math.Abs(cmd[3].Eval(vars));
        double inner = Math.Abs(cmd[4].Eval(vars));
        double gap = Math.Abs(cmd[5].Eval(vars));
        double rotation = cmd.Count > 6 ? cmd[6].Eval(vars) : 0;

        var paths = new Paths64
        {
            new Path64 { new Point64(fmt.ToFixed(centerX), fmt.ToFixed(centerY)) }
        }.Render(fmt.ToFixed(outer), false, fmt.BuildClipperOffset());

        var innerPaths = new Paths64
        {
            new Path64 { new Point64(fmt.ToFixed(centerX), fmt.ToFixed(centerY)) }
        }.Render(fmt.ToFixed(inner), false, fmt.BuildClipperOffset());

        innerPaths.Reverse();
        paths.AddRange(innerPaths);

        if(gap>0.0)
        {
            paths.Add(new Path64
            {
                new Point64(fmt.ToFixed(centerX + gap * 0.5), fmt.ToFixed(centerY + outer * 0.5)),
                new Point64(fmt.ToFixed(centerY + gap * 0.5), fmt.ToFixed(centerY - outer * 0.5)),
                new Point64(fmt.ToFixed(centerY - gap * 0.5), fmt.ToFixed(centerY - outer * 0.5)),
                new Point64(fmt.ToFixed(centerY - gap * 0.5), fmt.ToFixed(centerY + outer * 0.5)),
            });
            paths.Add(new Path64
            {
                new Point64(fmt.ToFixed(centerX + outer * 0.5), fmt.ToFixed(centerY + gap * 0.5)),
                new Point64(fmt.ToFixed(centerX + outer * 0.5), fmt.ToFixed(centerY - gap * 0.5)),
                new Point64(fmt.ToFixed(centerX - outer * 0.5), fmt.ToFixed(centerY - gap * 0.5)),
                new Point64(fmt.ToFixed(centerX - outer * 0.5), fmt.ToFixed(centerY + gap * 0.5)),
            });
        }

        //Возможно NonZero
        plot.DrawPaths(paths, true, 0,0,false,false,rotation/(180*Math.PI),1.0, true, FillRule.Positive);
    }
}