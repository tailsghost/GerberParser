﻿using Clipper2Lib;
using GerberParser.Abstracts.PCB;
using GerberParser.Abstracts.SVG;
using GerberParser.Core.Coord;

namespace GerberParser.Core.Svg;

public class File : FileBase
{
    public File(StringWriter stream, Rect64 bounds, double scale)
    {
        StringWriter = stream;
        WriteSvgHeader(bounds, scale);
    }

    private void WriteSvgHeader(Rect64 bounds, double scale)
    {
        var minX = FormatHelper.ToMM(bounds.left);
        var minY = FormatHelper.ToMM(bounds.top);
        var width = FormatHelper.ToMM(bounds.right - bounds.left);
        var height = FormatHelper.ToMM(bounds.bottom - bounds.top);

        Writer.WriteLine($"<svg viewBox=\"{minX} {minY} {width} " +
            $"{height}\" width=\"{width * scale}\" height=\"{height * scale}\" " +
            $"xmlns=\"http://www.w3.org/2000/svg\">");
    }

    ~File()
    {
        Close();
    }

    public override void Add(Layer layer)
    {
        Writer.Write(layer.ToString());
    }

    public override void Add(string svgData)
    {
        Writer.Write(svgData);
    }

    public override void Close()
    {
        if (Writer != null)
        {
            Writer.WriteLine("</svg>");
            Writer.Close();
            Writer = null;
        }
    }

    public override string GetString()
    {
       return Writer.ToString();
    }
}
