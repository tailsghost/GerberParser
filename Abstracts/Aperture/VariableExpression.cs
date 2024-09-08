namespace GerberParser.Abstracts.Aperture;

public abstract class VariableExpression : Expression
{
    protected int index;

    public VariableExpression(int index)
    {
        this.index = index;
    }

    public abstract override double Eval(Dictionary<int, double> vars);

    public abstract override string Debug();
}
