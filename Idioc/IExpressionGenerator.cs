using System;
using System.Linq.Expressions;

namespace Idioc
{
    public interface IExpressionGenerator
    {
        Expression GenerateExpression(Type type);
        event EventHandler<ExpressionGeneratedEventArgs> ExpressionGenerated;
    }
}