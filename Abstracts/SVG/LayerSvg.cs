using GerberParser.Constants;
using GerberParser.Core.Svg;
using System.Text;

namespace GerberParser.Abstracts.SVG;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

public abstract class LayerSvgBase(string identifier, Attributes attr)
{
    private string Identifier { get; } = identifier;

    private Attributes Attr { get; } = attr;

    protected StringBuilder Data = new();

    public abstract void Add(Polygons paths, Color? color, AttributesBase? attr = null);

    public abstract void Add(string svgData);

    public abstract LayerSvgBase AppendSvgData(string svgData);

    public abstract StreamWriter WriteTo(StreamWriter stream);
}
