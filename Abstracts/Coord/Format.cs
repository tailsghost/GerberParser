using ClipperLib;

namespace GerberParser.Abstracts.Coord;

public abstract class FormatBase(double maxDeviation = 0.005, double miterLimit = 1.0)
{
    protected bool fmtConfigured = false;
    protected int nInt;
    protected int nDec;
    protected bool unitConfigured = false;
    protected bool addTrailingZeros = false;
    protected double factor;
    protected bool used = false;
    protected double miterLimit = miterLimit;
    protected double maxDeviation = maxDeviation;

    public abstract void ConfigureFormat(int nInt, int nDec);

    public abstract void ConfigureTrailingZeros(bool addTrailingZeros);

    public abstract void ConfigureInch();

    public abstract void ConfigureMM();

    public abstract long ParseFixed(string s);

    public abstract long ParseFloat(string s);

    public abstract long ToFixed(double d);

    public abstract long GetMaxDeviation();

    public abstract long GetMiterLimit();

    public abstract ClipperOffset BuildClipperOffset();

    protected abstract void TryToReconfigure();

    protected abstract void TryToUse();
}
