using System;
using System.Linq.Expressions;

namespace Idioc
{
    class SingleInstanceProvider : IInstanceProvider
    {
        readonly Type type;
        readonly IExpressionGenerator generator;
        readonly Lazy<object> instance;
        readonly Lazy<Expression> expression; 

        public SingleInstanceProvider(Type type, IExpressionGenerator generator)
        {
            this.type = type;
            this.generator = generator;
            instance = new Lazy<object>(CreateInstance);
            expression = new Lazy<Expression>(() => this.generator.GenerateExpression(this.type));
        }

        object CreateInstance()
        {
            var func = Expression.Lambda<Func<object>>(expression.Value).Compile();
            return func();
        }

        public object GetInstance() { return instance.Value; }
        public Expression Expression { get { return expression.Value;  } }
    }
}