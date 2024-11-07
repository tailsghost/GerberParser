namespace GerberParser.Core.Coord;

public static class FormatHelper
{
    public static long FromMM(double i)
    {

        var result = Math.Round(i * 1e10);
        var resultLong = (long)result;

        return resultLong;
    }

    public static double ToMM(long i)
    {
        var result = i / 1e10;
        return result;
    }
}
