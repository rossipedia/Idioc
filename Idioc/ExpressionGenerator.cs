// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionGenerator.cs" company="Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the ExpressionGenerator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using Idioc.ExpressionGenerators;

    /// <summary>
    /// Base class for generating Expressions that will be used
    /// to produce instancies for dependency resolution
    /// </summary>
    public abstract class ExpressionGenerator : IDependencyVisitor, IExpressionGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionGenerator"/> class.
        /// </summary>
        protected ExpressionGenerator()
        {
            this.DependencyExpressionGenerating = (sender, args) => { };
            this.ExpressionGenerated = (sender, args) => { };
        }

        /// <summary>
        /// The event that is raised when an expression is generated for a type.
        /// Consumers can modify the expression and the new expression will be used
        /// in instead.
        /// </summary>
        public event EventHandler<ExpressionGeneratedEventArgs> ExpressionGenerated;

        /// <summary>
        /// The event that is raised when a dependency is found, and an expression
        /// needs to be generated for it.
        /// </summary>
        public event EventHandler<ExpressionGeneratingEventArgs> DependencyExpressionGenerating;

        /// <summary>
        /// Implementation of <see cref="IExpressionGenerator.GenerateExpression"/>. Calls
        /// <see cref="CreateExpression"/> and raises the <see cref="ExpressionGenerated"/> event
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// A <see cref="System.Linq.Expressions.Expression"/> that when evaluated 
        /// will return an instance of the given type, taking into account any
        /// event handlers that may have modified the expression
        /// </returns>
        public Expression GenerateExpression(Type type)
        {
            var expression = this.CreateExpression(type);
            var args = new ExpressionGeneratedEventArgs(type, expression);
            this.OnExpressionGenerated(args);
            while (args.Expression.CanReduce)
            {
                args.Expression = args.Expression.Reduce();
            }

            return args.Expression;
        }

        /// <summary>
        /// Creates the expression that will be used. This method must
        /// be overriden in derived classes.
        /// </summary>
        /// <param name="type">
        /// The type to produce an expression for.
        /// </param>
        /// <returns>
        /// A <see cref="System.Linq.Expressions.Expression"/> that when evaluated 
        /// will return an instance of the given type
        /// </returns>
        protected abstract Expression CreateExpression(Type type);

        /// <summary>
        /// Raises the <see cref="DependencyExpressionGenerating"/> event.
        /// </summary>
        /// <param name="args">
        /// The event args for this event. 
        /// </param>
        protected virtual void OnExpressionGenerating(ExpressionGeneratingEventArgs args)
        {
            var handler = this.DependencyExpressionGenerating;
            handler(this, args);
        }

        /// <summary>
        /// Raises the <see cref="ExpressionGenerated"/> event.
        /// </summary>
        /// <param name="args">
        /// The event args for this event.
        /// </param>
        protected virtual void OnExpressionGenerated(ExpressionGeneratedEventArgs args)
        {
            var handler = this.ExpressionGenerated;
            handler(this, args);
        }

        /// <summary>
        /// Creates Dependency expressions for the given type. This returns an empty enumerable for ConstantExpressions
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// An empty enumerable.
        /// </returns>
        protected virtual IEnumerable<Expression> CreateDependencyExpressions(Type type)
        {
            yield break;
        }
    }
}