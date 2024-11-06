using GerberParser.Abstracts.SVG;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GerberParser.Core.Svg;

public class Attributes : AttributesBase
{
    public override AttributesBase With(string key, string value)
    {
        Data.Append($" {key}=\"{value}\"");
        return this;
    }

    public override void WriteTo(StreamWriter stream)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return Data.ToString();
    }
}
