using System;
using System.Linq.Expressions;

namespace Idioc
{
    class LambdaExpressionGenerator : ExpressionGenerator
    {
        private readonly Func<object> lambda;

        public LambdaExpressionGenerator(Func<object> lambda)
        {
            this.lambda = lambda;
        }

        protected override Expression CreateExpression(Type type)
        {
            // Extract body from lambda and 
            var lambdaTarget =
                lambda.Target != null ?
                                          Expression.Constant(lambda.Target) :
                                                                                 null;

            return Expression.Convert(Expression.Call(lambdaTarget, lambda.Method), type);
        }
    }
}