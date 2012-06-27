// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConstantExpressionGenerator.cs" company="Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the ConstantExpressionGenerator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.ExpressionGenerators
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Generates a <see cref="ConstantExpression"/> given an instance of an object.
    /// </summary>
    public class ConstantExpressionGenerator : ExpressionGenerator
    {
        /// <summary>
        /// The instance to return.
        /// </summary>
        private readonly object instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantExpressionGenerator"/> class.
        /// </summary>
        /// <param name="instance">
        /// The instance.
        /// </param>
        public ConstantExpressionGenerator(object instance)
        {
            this.instance = instance;
        }

        /// <summary>
        /// Creates the ConstantExpression representing the object this generator was created with.
        /// </summary>
        /// <param name="type">
        /// The type to convert the instance to
        /// </param>
        /// <returns>
        /// A <see cref="ConstantExpression"/> representing the object this generator represents.
        /// </returns>
        protected override Expression CreateExpression(Type type)
        {
            return Expression.Constant(this.instance, type);
        }
    }
}