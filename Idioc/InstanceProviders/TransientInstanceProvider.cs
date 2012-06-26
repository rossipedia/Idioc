// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TransientInstanceProvider.cs" company="Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the TransientInstanceProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.InstanceProviders
{
    using System;
    using System.Linq.Expressions;

    using Idioc.ExpressionGenerators;

    /// <summary>
    /// Provides Transient instances
    /// </summary>
    public class TransientInstanceProvider : IInstanceProvider
    {
        /// <summary>
        /// The type of instance to return
        /// </summary>
        private readonly Type type;

        /// <summary>
        /// The <see cref="IExpressionGenerator"/> used to generate the 
        /// expression that, when evaluated, returns an instance of <see cref="type"/>.
        /// </summary>
        private readonly IExpressionGenerator generator;

        /// <summary>
        /// Lazy-construct a <see cref="Func{T}"/> delegate that will return 
        /// an instance of <see cref="type"/>.
        /// </summary>
        private readonly Lazy<Func<object>> func;

        /// <summary>
        /// The expression used to provide instances of <see cref="type"/>.
        /// </summary>
        private readonly Lazy<Expression> expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientInstanceProvider"/> class.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="expressionGenerator">
        /// The expression generator.
        /// </param>
        public TransientInstanceProvider(Type type, IExpressionGenerator expressionGenerator)
        {
            this.type = type;
            this.generator = expressionGenerator;
            this.func = new Lazy<Func<object>>(this.CreateObjectFunc);
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
        /// Implementation of <see cref="IInstanceProvider.GetInstance"/>.
        /// </summary>
        /// <returns>
        /// An object instance provided by <see cref="func"/>
        /// </returns>
        public object GetInstance()
        {
            // Optimize for ConstantExpression
            return this.expression.Value is ConstantExpression
                       ? (this.expression.Value as ConstantExpression).Value
                       : this.func.Value();
        }

        /// <summary>
        /// Creates the function used to produce instances of <see cref="type"/>
        /// </summary>
        /// <returns>
        /// A <see cref="Func{T}"/> that when called will return instances of <see cref="type"/>
        /// </returns>
        private Func<object> CreateObjectFunc()
        {
            return Expression.Lambda<Func<object>>(this.expression.Value).Compile();
        }
    }

}