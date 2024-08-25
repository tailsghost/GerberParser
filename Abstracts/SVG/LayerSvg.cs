using Clipper2Lib;
using GerberParser.Core.Colors;
using System.Text;

namespace GerberParser.Abstracts.SVG;

public abstract class LayerSvg
{
    private StringBuilder data;

    public abstract void Add(Paths64 paths, Color? color, Attributes attr = null);

    public abstract void Add(string svgData);

    public abstract LayerSvg AppendSvgData(string svgData);

    public abstract StreamWriter WriteTo(StreamWriter stream);
}
