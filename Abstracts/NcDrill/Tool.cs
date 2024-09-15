namespace GerberParser.Abstracts.NcDrill;

public abstract class Tool
{
    protected long diameter { get; set; }

    protected bool plated { get; set; }

    protected Tool(int diameter, bool plated)
    {
        this.diameter = diameter;
        this.plated = plated;
    }

}
