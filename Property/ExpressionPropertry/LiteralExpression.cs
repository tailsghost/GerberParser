using GerberParser.Abstracts.Aperture;

namespace GerberParser.Property.ExpressionPropertry;

public class LiteralExpression : Expression
{
    private readonly double value;

    public LiteralExpression(double value)
    {
        this.value = value;
    }

    public override double Eval(Dictionary<int, double> vars)
    {
        return value;
    }

    public override string Debug()
    {
        return value.ToString();
    }
}
