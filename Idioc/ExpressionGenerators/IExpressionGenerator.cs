// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExpressionGenerator.cs" company="Bryan Ross">
//   (c) 2012 Bryan Ross
// </copyright>
// <summary>
//   Defines the DependencyResolutionException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.ExpressionGenerators
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// The Expresion generator interface
    /// </summary>
    public interface IExpressionGenerator
    {
        /// <summary>
        /// This event is raised after an expression is generated.
        /// Consumers can modify the <see cref="ExpressionGeneratedEventArgs.Expression" /> property
        /// to customize the expression generated
        /// </summary>
        event EventHandler<ExpressionGeneratedEventArgs> ExpressionGenerated;

        /// <summary>
        /// This method is used to generate an expression for the given type.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// A <see cref="System.Linq.Expressions.Expression"/> that can be used to return an
        /// instance of the supplied type.
        /// </returns>
        Expression GenerateExpression(Type type);
    }
}