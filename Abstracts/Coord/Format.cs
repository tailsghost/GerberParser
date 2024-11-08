using ClipperLib;

namespace GerberParser.Abstracts.Coord;

public abstract class FormatBase
{
    protected bool fmtConfigured;
    protected int nInt;
    protected int nDec;
    protected bool unitConfigured;
    protected bool addTrailingZeros;
    protected double factor;
    protected bool used;
    protected double miterLimit;
    protected double maxDeviation;
    protected FormatBase(double maxDeviation = 0.005, double miterLimit = 1.0)
    {
        this.maxDeviation = maxDeviation;
        this.miterLimit = miterLimit;
        this.addTrailingZeros = false;
        this.unitConfigured = false;
        this.fmtConfigured = false;
        this.used = false;
    }

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
