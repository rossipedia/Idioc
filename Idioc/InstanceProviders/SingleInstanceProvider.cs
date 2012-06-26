// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleInstanceProvider.cs" company="Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the SingleInstanceProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.InstanceProviders
{
    using System;
    using System.Linq.Expressions;

    using Idioc.ExpressionGenerators;

    /// <summary>
    /// Provides a single instance of an object
    /// </summary>
    public class SingleInstanceProvider : IInstanceProvider
    {
        /// <summary>
        /// The type of instance to provide
        /// </summary>
        private readonly Type type;

        /// <summary>
        /// The expression generator used to create the expression
        /// that, when evaluated, returns an instance of <see cref="type"/>
        /// </summary>
        private readonly IExpressionGenerator generator;

        /// <summary>
        /// Use a Lazy initializer to store a reference to the object.
        /// </summary>
        private readonly Lazy<object> instance;

        /// <summary>
        /// The expression that was created by <see cref="generator"/>
        /// </summary>
        private readonly Lazy<Expression> expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleInstanceProvider"/> class.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="generator">
        /// The generator.
        /// </param>
        public SingleInstanceProvider(Type type, IExpressionGenerator generator)
        {
            this.type = type;
            this.generator = generator;
            this.instance = new Lazy<object>(this.CreateInstance);
            this.expression = new Lazy<Expression>(() => this.generator.GenerateExpression(this.type));
        }

        /// <summary>
        /// Gets Expression.
        /// </summary>
        public Expression Expression
        {
            get
            {
                return this.expression.Value;
            }
        }

        /// <summary>
        /// Gets the instance that this provider has stored.
        /// </summary>
        /// <returns>
        /// The object instance
        /// </returns>
        public object GetInstance()
        {
            return this.instance.Value;
        }

        /// <summary>
        /// Performs the actual instance creation.
        /// </summary>
        /// <returns>
        /// The instance.
        /// </returns>
        private object CreateInstance()
        {
            // Optimize for ConstantExpression
            if (this.expression.Value is ConstantExpression)
            {
                return (this.expression.Value as ConstantExpression).Value;
            }

            var compiled = Expression.Lambda<Func<object>>(this.expression.Value).Compile();
            return compiled();
        }
    }
}