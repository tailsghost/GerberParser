using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberParser.Abstracts.Aperture.ExpressionPropertry;

public class VariableExpression : Expression
{
    private readonly int index;

    public VariableExpression(int index)
    {
        this.index = index;
    }

    public override double Eval(Dictionary<int, double> vars)
    {
        return vars.ContainsKey(index) ? vars[index] : 0.0;
    }

    public override string Debug()
    {
        return "$" + index;
    }

    public int GetIndex() => index;
}
