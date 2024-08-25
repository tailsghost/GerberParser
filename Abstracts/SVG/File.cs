using Clipper2Lib;
using GerberParser.Abstracts.PCB;

namespace GerberParser.Abstracts.SVG;

public abstract class File
{
    public StreamWriter Writer { get; }
    public StringWriter StringWriter { get; }
    public bool IsStringWriter { get; }
    public abstract void Add(Layer layer);
    public abstract void Add(string svgData);
    public abstract void Close();
    public abstract string GetString();
}
