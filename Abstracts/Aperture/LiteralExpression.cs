namespace GerberParser.Abstracts.Aperture;

public abstract class LiteralExpression : Expression
{
    protected double value;

    public LiteralExpression(double value)
    {
        this.value = value;
    }

    public abstract override double Eval(Dictionary<int, double> vars);

    public abstract override string Debug();
}
