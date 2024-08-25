using GerberParser.Core.Colors;

namespace GerberParser.Core.PCB;

public struct ColorScheme
{
   public Color soldermask { get; }

    public Color silkscreen { get; }

    public Color finish {  get; }

    public Color substrate { get; }

    public Color copper { get; }

    public ColorScheme()
    {
        soldermask = ColorConstants.MASK_GREEN;
        silkscreen = ColorConstants.SILK_WHITE;
        finish = ColorConstants.FINISH_TIN;
        substrate = ColorConstants.SUBSTRATE;
        copper = ColorConstants.COPPER;
    }
}
