// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LambdaExpressionGenerator.cs" company="Bryan Ross">
//   (c) 2012 Bryan Ross
// </copyright>
// <summary>
//   Defines the LambdaExpressionGenerator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.ExpressionGenerators
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Generates an expression for a type using a supplied Func object.
    /// </summary>
    public class LambdaExpressionGenerator : ExpressionGenerator
    {
        /// <summary>
        /// The Func object to use for generating the expression
        /// </summary>
        private readonly Func<object> lambda;

        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaExpressionGenerator"/> class.
        /// </summary>
        /// <param name="lambda">
        /// The lambda.
        /// </param>
        public LambdaExpressionGenerator(Func<object> lambda)
        {
            this.lambda = lambda;
        }

        /// <summary>
        /// Generates the expression
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// An expression representing a single call to the lambda
        /// </returns>
        protected override Expression CreateExpression(Type type)
        {
            // Expression.Call requires an instance if a non-static method
            // otherwise the instance expression must be null
            var lambdaTarget =
                this.lambda.Target != null ?
                Expression.Constant(this.lambda.Target) :
                null;

            return Expression.Convert(Expression.Call(lambdaTarget, this.lambda.Method), type);
        }
    }
}