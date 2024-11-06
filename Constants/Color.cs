namespace GerberParser.Constants;


public struct Color
{
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }
}

public static class ColorConstants
{
    public static Color NONE { get; } = new Color { A = 0.0f, B = 0.0f, G = 0.0f, R = 0.0f };

    public static Color BLACK { get; } = new Color { A = 0.0f, B = 0.0f, G = 0.0f, R = 1.0f };

    public static Color COPPER { get; } = new Color { A = 0.8f, B = 0.7f, G = 0.3f, R = 1.0f };

    public static Color FINISH_TIN { get; } = new Color { A = 0.7f, B = 0.7f, G = 0.7f, R = 1.0f };

    public static Color SUBSTRATE { get; } = new Color { A = 0.6f, B = 0.5f, G = 0.3f, R = 0.95f };

    public static Color MASK_GREEN { get; } = new Color { A = 0.1f, B = 0.6f, G = 0.3f, R = 0.6f };

    public static Color MASK_WHITE { get; } = new Color { A = 0.9f, B = 0.9f, G = 0.9f, R = 0.9f };

    public static Color SILK_WHITE { get; } = new Color { A = 0.9f, B = 0.9f, G = 0.9f, R = 0.9f };

    public static Color SILK_BLACK { get; } = new Color { A = 0.1f, B = 0.1f, G = 0.1f, R = 0.9f };
}