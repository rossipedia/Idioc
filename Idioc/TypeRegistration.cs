// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeRegistration.cs" company="Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the TypeRegistration type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc
{
    using System;
    using System.Linq.Expressions;

    using Idioc.ExpressionGenerators;
    using Idioc.InstanceProviders;

    /// <summary>
    /// Represents a type registration. 
    /// This class has everything it needs to know to build
    /// a resolved instance of a concrete type
    /// </summary>
    public class TypeRegistration : IInstanceProvider
    {
        /// <summary>
        /// The concrete type to resolve.
        /// </summary>
        private readonly Type concreteType;

        /// <summary>
        /// The <see cref="IInstanceProviderFactory"/> that will create
        /// an <see cref="IInstanceProvider"/> that will provide our instances
        /// </summary>
        private readonly IInstanceProviderFactory providerFactory;

        /// <summary>
        /// Lazy-construct the provider that will be used 
        /// to provide instances
        /// </summary>
        private readonly Lazy<IInstanceProvider> provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeRegistration"/> class.
        /// </summary>
        /// <param name="concreteType">
        /// The concrete type.
        /// </param>
        /// <param name="generator">
        /// The generator.
        /// </param>
        /// <param name="providerFactory">
        /// The provider factory.
        /// </param>
        public TypeRegistration(Type concreteType, IExpressionGenerator generator, IInstanceProviderFactory providerFactory)
        {
            Guard.ArgumentNotNull(concreteType, "concreteType");
            Guard.ArgumentNotNull(generator, "generator");
            Guard.ArgumentNotNull(providerFactory, "providerFactory");

            this.concreteType = concreteType;
            this.providerFactory = providerFactory;
            this.Generator = generator;

            this.provider = new Lazy<IInstanceProvider>(this.CreateProvider);
        }

        /// <summary>
        /// Gets the expression generator to use for generating
        /// the provider expression for this type
        /// </summary>
        public IExpressionGenerator Generator { get; private set; }

        /// <summary>
        /// Gets Expression.
        /// Need to use this so that we can resolve nested dependencies
        /// </summary>
        public Expression Expression
        {
            get
            {
                return this.provider.Value.Expression;
            }
        }

        /// <summary>
        /// Implementation of <see cref="IInstanceProvider"/>.
        /// </summary>
        /// <returns>
        /// A dependency-resolved instance
        /// </returns>
        public object GetInstance()
        {
            return this.provider.Value.GetInstance();
        }

        /// <summary>
        /// Creates the provider to use for instances
        /// </summary>
        /// <returns>
        /// The <see cref="IInstanceProvider"/> to use to resolve instances
        /// </returns>
        private IInstanceProvider CreateProvider()
        {
            return this.providerFactory.CreateProvider(this.concreteType, this.Generator);
        }
    }
 }