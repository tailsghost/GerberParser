using System.Text;

namespace GerberParser.Abstracts.SVG;

public abstract class Attributes
{
    public StringBuilder Data { get; }
    public abstract Attributes With(string key, string value);
    public abstract void WriteTo(StreamWriter stream);
}
