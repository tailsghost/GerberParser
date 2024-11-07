using Clipper2Lib;
using GerberParser.Abstracts.APERTURE;
using GerberParser.Core.Aperture;
using GerberParser.Core.Coord;
using GerberParser.Core.PlotCore;
using GerberParser.Enums;
using System.Text;

namespace GerberParser.Abstracts.GERBER;

public abstract class GerberBase
{
    protected Dictionary<int, Base> Apertures = new Dictionary<int, Base>();

    protected Dictionary<string, ApertureMacro> ApertureMacros = new Dictionary<string, ApertureMacro>();

    protected ApertureMacro AmBuilder;

    protected Stack<Plot> PlotStack = new Stack<Plot>();

    protected ConcreteFormat fmt = new();

    protected InterpolationMode imode;

    protected QuadrantMode qmode;

    protected Base Aperture;

    protected Point64 Pos;

    protected bool Polarity;

    protected bool apMirrorX;

    protected bool apMirrorY;

    protected double apRotate;

    protected double apScale;

    protected bool RegionMode;

    protected Path64 RegionAccum = new();

    protected Paths64 Outlines = new();

    protected bool OutlineConstructed;

    protected abstract void DrawAperture();

    protected abstract void Interpolate(Point64 dest, Point64 center);

    protected abstract void CommitRegion();

    protected abstract bool Command(string cmd, bool isAttrib);

    protected abstract void EndAttrib();

    protected GerberBase(StringReader stream)
    {
        imode = InterpolationMode.UNDEFINED;
        qmode = QuadrantMode.UNDEFINED;
        Pos = new Point64(0, 0);
        Polarity = true;
        apMirrorX = false;
        apMirrorY = false;
        apRotate = 0.0;
        apScale = 1.0;
        PlotStack = new Stack<Plot>();
        PlotStack.Push(new Plot());
        RegionMode = false;
        OutlineConstructed = false;

        bool terminated = false;
        bool is_attrib = false;
        var ss = new StringBuilder();

        while (stream.Peek() != -1)
        {
            char c = (char)stream.Read();
            if (char.IsWhiteSpace(c))
            {
                continue;
            }
            else if (c == '%')
            {
                if (ss.Length > 0) throw new InvalidOperationException("attribute mid-command");
                if (is_attrib) EndAttrib();
                is_attrib = !is_attrib;
            }
            else if (c == '*')
            {
                if (ss.Length == 0) throw new InvalidOperationException("empty command");
                if (!Command(ss.ToString(), is_attrib))
                {
                    terminated = true;
                    break;
                }
                ss.Clear();
            }
            else
            {
                ss.Append(c);
            }
        }
        if (is_attrib)
        {
            throw new InvalidOperationException("unterminated attribute");
        }
        if (!terminated)
        {
            throw new InvalidOperationException("unterminated gerber file");
        }
        if (PlotStack.Count != 1)
        {
            throw new InvalidOperationException("unterminated block aperture");
        }
        if (RegionMode)
        {
            throw new InvalidOperationException("unterminated region block");
        }
    }

    public abstract Paths64 GetPaths();
    public abstract Paths64 GetOutlinePaths();
}
