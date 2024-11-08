using GerberParser.Constants;
using GerberParser.Core.Svg;
using System.Text;

namespace GerberParser.Abstracts.SVG;

using Polygons = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

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

    public abstract void Add(Polygons paths, Color? color, AttributesBase attr = null);

    public abstract void Add(string svgData);

    public abstract LayerSvgBase AppendSvgData(string svgData);

    public abstract StreamWriter WriteTo(StreamWriter stream);
}
