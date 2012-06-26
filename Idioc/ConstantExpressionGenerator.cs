using System;
using System.Linq.Expressions;

namespace Idioc
{
    class ConstantExpressionGenerator : ExpressionGenerator
    {
        private readonly object instance;

        public ConstantExpressionGenerator(object instance)
        {
            this.instance = instance;
        }

        protected override Expression CreateExpression(Type type)
        {
            return Expression.Constant(instance, type);
        }
    }
}