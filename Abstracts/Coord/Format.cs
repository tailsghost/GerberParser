﻿namespace GerberParser.Abstracts.Coord;

public abstract class Format
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
    protected Format(double maxDeviation = 0.005, double miterLimit = 1.0)
    {
        this.maxDeviation = maxDeviation;
        this.miterLimit = miterLimit;
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

    public abstract object BuildClipperOffset();

    public  abstract long FromMM(double i);

    public  abstract double ToMM(long i);

    protected abstract void TryToReconfigure();

    protected abstract void TryToUse();
}