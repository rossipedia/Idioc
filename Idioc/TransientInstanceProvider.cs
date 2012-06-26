using System;
using System.Linq.Expressions;

namespace Idioc
{
    class TransientInstanceProvider : IInstanceProvider
    {
        readonly Type type;
        readonly IExpressionGenerator generator;
        readonly Lazy<Func<object>> func;
        readonly Lazy<Expression> expression; 

        public TransientInstanceProvider(Type type, IExpressionGenerator expressionGenerator)
        {
            this.type = type;
            generator = expressionGenerator;
            func = new Lazy<Func<object>>(CreateObjectFunc);
            expression = new Lazy<Expression>(() => generator.GenerateExpression(this.type));
        }

        Func<object> CreateObjectFunc()
        {
            return Expression.Lambda<Func<object>>(expression.Value).Compile();
        }

        public object GetInstance()
        {
            return func.Value();
        }

        public Expression Expression { get { return expression.Value; } }
    }
}