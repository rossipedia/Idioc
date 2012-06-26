using System;
using System.Linq.Expressions;

namespace Idioc
{
    public class ExpressionGeneratedEventArgs : EventArgs
    {
        public Type Type { get; private set; }
        public Expression Expression { get; set; }

        public ExpressionGeneratedEventArgs(Type type, Expression expression)
        {
            Type = type;
            Expression = expression;
        }
    }
}