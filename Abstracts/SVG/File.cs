using Clipper2Lib;
using GerberParser.Abstracts.PCB;
using GerberParser.Core.Coord;

namespace GerberParser.Abstracts.SVG;

public abstract class FileBase
{
    public StringWriter Writer { get; set; } = new();
    public StringWriter StringWriter { get; set; } = new();
    public bool IsStringWriter { get; }
    public abstract void Add(Layer layer);
    public abstract void Add(string svgData);
    public abstract void Close();
    public abstract string GetString();
}
