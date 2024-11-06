using System.Text;

namespace GerberParser.Abstracts.SVG;

public abstract class AttributesBase
{
    public StringBuilder Data { get; } = new();
    public abstract AttributesBase With(string key, string value);
    public abstract void WriteTo(StreamWriter stream);
}
