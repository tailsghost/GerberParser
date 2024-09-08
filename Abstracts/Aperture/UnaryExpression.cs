namespace GerberParser.Abstracts.Aperture;

public abstract class UnaryExpression : Expression
{
    private char oper;
    private Expression expr;

    public UnaryExpression(char oper, Expression expr)
    {
        this.oper = oper;
        this.expr = expr;
    }

    public abstract override double Eval(Dictionary<int, double> vars);

    public abstract override string Debug();
}
