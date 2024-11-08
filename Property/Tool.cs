namespace GerberParser.Property;

public class Tool

{
    public long diameter { get; }

    public bool plated { get; }

    public Tool(long diameter, bool plated)
    {
        this.diameter = diameter;
        this.plated = plated;
    }

}
