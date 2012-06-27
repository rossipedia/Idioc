// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConstructorExpressionGenerator.cs" company="Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the ConstructorExpressionGenerator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.ExpressionGenerators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Idioc.Exceptions;

    /// <summary>
    /// Generates a <see cref="NewExpression"/> for the given type, using a given IConstructorSelector 
    /// </summary>
    public class ConstructorExpressionGenerator : ExpressionGenerator
    {
        /// <summary>
        /// Used to select which constructor to use
        /// </summary>
        private readonly IConstructorSelector selector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstructorExpressionGenerator"/> class.
        /// </summary>
        /// <param name="selector">
        /// The selector.
        /// </param>
        public ConstructorExpressionGenerator(IConstructorSelector selector)
        {
            this.selector = selector;
        }

        /// <summary>
        /// Creates the Expression
        /// </summary> 
        /// <param name="type">
        /// The type to create the expression from
        /// </param>
        /// <returns>
        /// A <see cref="NewExpression"/> representing the selected constructor
        /// with appropriate dependencies resolved.
        /// </returns>
        /// <exception cref="TypeNotConstructableException">
        /// Thrown when a suitable constructor cannot be found
        /// </exception>
        protected override Expression CreateExpression(Type type)
        {
            Guard.ArgumentNotNull(type, "Type");

            var constructor = this.selector.SelectConstructor(type);
            return Expression.New(constructor, this.CreateDependencyExpressions(type));
        }

        /// <summary>
        /// Returns the dependencies for the given type. For 
        /// ConstructorExpressionGenerator, dependencies are 
        /// the constructor parameters
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{Expression}"/> representing the type's dependencies
        /// </returns>
        protected override IEnumerable<Expression> CreateDependencyExpressions(Type type)
        {
            return this.GetParametersForConstructor(type).Select(parameter =>
            {
                var eventArgs = new ExpressionGeneratingEventArgs(parameter.ParameterType);
                this.OnExpressionGenerating(eventArgs);
                if (eventArgs.Expression == null)
                {
                    throw new DependencyResolutionException(eventArgs.DependencyType);
                }

                return eventArgs.Expression;
            });
        }

        /// <summary>
        /// Gets the parameters for the constructor of the given type,
        /// using the selector this instance was created with
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{ParameterInfo}"/> representing the constructor parameters
        /// </returns>
        /// <exception cref="TypeNotConstructableException">
        /// No suitable constructor could be found.
        /// </exception>
        private IEnumerable<ParameterInfo> GetParametersForConstructor(Type type)
        {
            var constructor = this.selector.SelectConstructor(type);
            if (constructor == null)
            {
                throw new TypeNotConstructableException(type);
            }

            return constructor.GetParameters();
        }
    }
}