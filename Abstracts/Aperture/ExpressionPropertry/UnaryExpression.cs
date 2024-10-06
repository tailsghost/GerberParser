﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberParser.Abstracts.Aperture.ExpressionPropertry;

public class UnaryExpression : Expression
{
    private readonly char oper;
    private readonly Expression expr;

    public UnaryExpression(char oper, Expression expr)
    {
        this.oper = oper;
        this.expr = expr;
    }

    public override double Eval(Dictionary<int, double> vars)
    {
        return oper == '+' ? expr.Eval(vars) : -expr.Eval(vars);
    }

    public override string Debug()
    {
        return oper + expr.Debug();
    }
}
