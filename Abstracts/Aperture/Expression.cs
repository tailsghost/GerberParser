using System.Linq.Expressions;

namespace GerberParser.Abstracts.Aperture;

public abstract class Expression
{
    public abstract double Eval(Dictionary<int, double> vars);

    public abstract char GetToken();

    public abstract string Debug();

    public abstract Expression Parse(string expr);
}