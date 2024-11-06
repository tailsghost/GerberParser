using Clipper2Lib;
using GerberParser.Constants;
using GerberParser.Core.Coord;
using GerberParser.Core.Svg;
using System.Text;

namespace GerberParser.Abstracts.SVG;

public abstract class LayerSvgBase
{
    protected LayerSvgBase(string identifier, Attributes attr)
    {
        Identifier = identifier;
        Attr = attr;
    }

    private string Identifier {  get; }

    private Attributes Attr { get; }

    protected StringBuilder Data = new();

    protected ConcreteFormat Format = new();

    public abstract void Add(Paths64 paths, Color? color, AttributesBase attr = null);

    public abstract void Add(string svgData);

    public abstract LayerSvgBase AppendSvgData(string svgData);

    public abstract StreamWriter WriteTo(StreamWriter stream);
}
