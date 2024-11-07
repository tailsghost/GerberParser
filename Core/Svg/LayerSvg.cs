using Clipper2Lib;
using GerberParser.Abstracts.SVG;
using GerberParser.Constants;
using GerberParser.Core.Coord;

namespace GerberParser.Core.Svg;

public class LayerSvg : LayerSvgBase
{

    public LayerSvg(string identifier, Attributes attr = null) :base(identifier, attr)
    {
        Data.Append($"<g id=\"{identifier}\" {attr}>\n");
    }

    public override void Add(Paths64 paths, Color? color, AttributesBase attr = null)
    {
        if (color.Value.A == 0.0) return;

        Data.Append($"<path fill=\"rgb({(int)(color.Value.R * 255)},{(int)(color.Value.G * 255)},{(int)(color.Value.B * 255)})\"");
        if (color.Value.A < 1.0)
        {
            Data.Append($" fill-opacity=\"{color.Value.A}\"");
        }
        Data.Append(attr);
        Data.Append(" d=\"");

        foreach (var path in paths)
        {
            Data.Append($"M {FormatHelper.ToMM(path.Last().X)} {FormatHelper.ToMM(path.Last().Y)} ");
            foreach (var coord in path)
            {
                Data.Append($"L {FormatHelper.ToMM(coord.X)} {FormatHelper.ToMM(coord.Y)} ");
            }
        }
        Data.Append("\"/>\n");
    }

    public override void Add(string svgData)
    {
        Data.Append(svgData).Append("\n");
    }

    public override LayerSvgBase AppendSvgData(string svgData)
    {
        throw new NotImplementedException();
    }

    public override StreamWriter WriteTo(StreamWriter stream)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return Data.ToString() + "</g>\n";
    }
}
