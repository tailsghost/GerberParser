namespace GerberParser.Abstracts.Aperture;

public abstract class Token : Expression
{
    private char token;

    public Token(char token)
    {
        this.token = token;
    }

    public abstract override double Eval(Dictionary<int, double> vars);

    public abstract override char GetToken();

    public abstract override string Debug();
}
