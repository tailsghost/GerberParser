namespace GerberParser.Abstracts.Aperture;

public abstract class BinaryExpression : Expression
{
    private char oper;
    private Expression lhs;
    private Expression rhs;

    public BinaryExpression(char oper, Expression lhs, Expression rhs)
    {
        this.oper = oper;
        this.lhs = lhs;
        this.rhs = rhs;
    }

    public abstract override double Eval(Dictionary<int, double> vars);

    public abstract override string Debug();
}
