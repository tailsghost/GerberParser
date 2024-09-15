using Clipper2Lib;
using GerberParser.Abstracts.Aperture;
using GerberParser.Abstracts.APERTURE;
using GerberParser.Abstracts.Coord;
using GerberParser.Abstracts.PLOT;
using GerberParser.Enums;

namespace GerberParser.Abstracts.GERBER;

public abstract class Gerber
{
    protected Dictionary<int, Base> apertures = new Dictionary<int, Base>();

    protected Dictionary<string, ApertureMacro> apertureMacros = new Dictionary<string, ApertureMacro>();

    protected ApertureMacro amBuilder;

    protected Stack<Plot> plotStack = new Stack<Plot>();

    protected Format fmt;

    protected InterpolationMode imode;

    protected QuadrantMode qmode;

    protected Base aperture;

    protected Point64 pos;

    protected bool polarity;

    protected bool apMirrorX;

    protected bool apMirrorY;

    protected double apRotate;

    protected double apScale;

    protected bool regionMode;

    protected Path64 regionAccum = new();

    protected Paths64 outline = new();

    protected bool outlineConstructed;

    protected abstract void DrawAperture();

    protected abstract void Interpolate(Point64 dest, Point64 center);

    protected abstract void CommitRegion();

    protected abstract bool Command(string cmd, bool isAttrib);

    protected abstract void EndAttrib();

    protected Gerber(Stream stream)
    {
    }

    public abstract Paths64 GetPaths();
    public abstract Paths64 GetOutlinePaths();
}
