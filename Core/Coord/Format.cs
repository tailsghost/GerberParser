using ClipperLib;
using GerberParser.Abstracts.Coord;
using System.Globalization;

namespace GerberParser.Core.Coord;

public class ConcreteFormat : FormatBase
{
    public ConcreteFormat(double maxDeviation = 0.005, double miterLimit = 1.0)
        : base(maxDeviation, miterLimit)
    {
    }

    public override void ConfigureFormat(int nInt, int nDec)
    {
        TryToReconfigure();
        fmtConfigured = true;
        this.nInt = nInt;
        this.nDec = nDec;
    }

    public override void ConfigureTrailingZeros(bool addTrailingZeros)
    {
        TryToReconfigure();
        this.addTrailingZeros = addTrailingZeros;
    }

    public override void ConfigureInch()
    {
        TryToReconfigure();
        unitConfigured = true;
        factor = 25.4;
    }

    public override void ConfigureMM()
    {
        TryToReconfigure();
        unitConfigured = true;
        factor = 1.0;
    }

    public override long ParseFixed(string s)
    {
        TryToUse();
        if (s.Contains('.'))
        {
            return ParseFloat(s);
        }

        int addZeros = 10 - nDec;
        if (factor == 25.4)
        {
            addZeros = 9 - nDec;
        }
        else if (factor != 1.0)
        {
            throw new InvalidOperationException("Unknown conversion factor");
        }

        if (addTrailingZeros)
        {
            int digits = s.Length;
            if (s[0] == '-' || s[0] == '+')
            {
                digits--;
            }
            if (digits < nInt + nDec)
            {
                addZeros += nInt + nDec - digits;
            }
        }

        string paddedString = s + new string('0', addZeros);
        long val = long.Parse(paddedString);
        if (factor == 25.4)
        {
            val *= 254;
        }
        return val;
    }

    public override long ParseFloat(string s)
    {
        var result = double.Parse(s, CultureInfo.InvariantCulture);
        return ToFixed(result);
    }

    public override long ToFixed(double d)
    {
        TryToUse();
        var result = Math.Round(d * factor * 1e10);
        var result1 = (long)result;
        return result1;
    }

    public override long GetMaxDeviation()
    {
        return FormatHelper.FromMM(maxDeviation);
    }

    public override long GetMiterLimit()
    {
        return FormatHelper.FromMM(miterLimit);
    }

    public override ClipperOffset BuildClipperOffset()
    {
        return new ClipperOffset(GetMiterLimit(), GetMaxDeviation());
    }

    protected override void TryToReconfigure()
    {
        if (used)
        {
            throw new InvalidOperationException(
                "Cannot reconfigure coordinate format after coordinates have already been interpreted."
            );
        }
    }

    protected override void TryToUse()
    {
        if (!fmtConfigured)
        {
            throw new InvalidOperationException(
                "Cannot convert coordinates before coordinate format is configured."
            );
        }
        if (!unitConfigured)
        {
            throw new InvalidOperationException(
                "Cannot convert coordinates before unit is configured."
            );
        }

        used = true;
    }
}
