// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionGeneratedEventArgs.cs" company="Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the ExpressionGeneratedEventArgs type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.ExpressionGenerators
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Event args for the <see cref="IExpressionGenerator.ExpressionGenerated"/> event
    /// </summary>
    public class ExpressionGeneratedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionGeneratedEventArgs"/> class.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="expression">
        /// The expression.
        /// </param>
        public ExpressionGeneratedEventArgs(Type type, Expression expression)
        {
            this.Type = type;
            this.Expression = expression;
        }

        /// <summary>
        /// Gets or sets Expression.
        /// </summary>
        public Expression Expression { get; set; }

        /// <summary>
        /// Gets the Type that the expression was generated for.
        /// </summary>
        public Type Type { get; private set; }
    }
}