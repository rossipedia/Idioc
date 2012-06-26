// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Container.cs" company="Bryan Ross">
//   (c) 2012 Bryan Ross
// </copyright>
// <summary>
//   Defines the DependencyResolutionException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc
{
    using System;
    using System.Collections.Generic;

    using Idioc.Exceptions;
    using Idioc.ExpressionGenerators;
    using Idioc.InstanceProviders;

    /// <summary>
    /// The Dependency-Injection Container
    /// </summary>
    public class Container
    {
        /// <summary>
        /// Maintains the mapping of requested types to their respective <see cref="TypeRegistration"/>.
        /// </summary>
        private readonly Dictionary<Type, TypeRegistration> registrations = new Dictionary<Type, TypeRegistration>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Container"/> class.
        /// </summary>
        public Container()
        {
            this.DependencyResolver = this.ResolveDependencyExpression;
        }

        /// <summary>
        /// Gets or sets DependencyResolver used to resolved dependencies.
        /// Allows consumers to customize dependency resolution behavior.
        /// </summary>
        public EventHandler<ExpressionGeneratingEventArgs> DependencyResolver { get; set; }

        /// <summary>
        /// Registers an abstract type for later resolution.  
        /// </summary>
        /// <param name="abstractType">
        /// The abstract type. This does not have to be a class marked as abstract. It is purely a naming convention.
        /// A concrete type may be supplied here.
        /// </param>
        /// <param name="registration">
        /// The <see cref="TypeRegistration"/> used for resolving the requested type.
        /// </param>
        public void Register(Type abstractType, TypeRegistration registration)
        {
            Guard.ArgumentNotNull(abstractType, "abstractType");
            Guard.ArgumentNotNull(registration, "registration");

            this.registrations.Add(abstractType, registration);
        }

        /// <summary>
        /// Resolves a type and it's dependencies to an instance.
        /// </summary>
        /// <param name="type">
        /// The type requested for resolution.
        /// </param>
        /// <returns>
        /// An instance of the requested type.
        /// </returns>
        /// <exception cref="TypeNotRegisteredException">
        /// Thrown when the requested type has not been registered with the container.
        /// </exception>
        public object Resolve(Type type)
        {
            Guard.ArgumentNotNull(type, "type");

            if (this.IsRegistered(type))
            {
                return this.GetRegistration(type).GetInstance();
            }

            throw new TypeNotRegisteredException(type);
        }

        /// <summary>
        /// The default type resolution handler.
        /// </summary>
        /// <param name="sender">
        /// The sender. This is the <see cref="ExpressionGenerator"/> that is currently building
        /// an expression for a requested type. 
        /// </param>
        /// <param name="args">
        /// The args. The <see cref="ExpressionGeneratingEventArgs.DependencyType"/> property contains the type that 
        /// needs to be resolved.
        /// </param>
        public void ResolveDependencyExpression(object sender, ExpressionGeneratingEventArgs args)
        {
            if (this.IsRegistered(args.DependencyType))
            {
                args.Expression = this.GetRegistration(args.DependencyType).Expression;
            }

            // Not setting args.Expression will cause a DependencyResolution error
        }

        /// <summary>
        /// Checks to see if the supplied type has been registered with this container.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// True if the type has been registered, false if it has not.
        /// </returns>
        private bool IsRegistered(Type type)
        {
            return this.registrations.ContainsKey(type);
        }

        /// <summary>
        /// Returns the <see cref="TypeRegistration"/> object for the given type.
        /// </summary>
        /// <param name="forType">
        /// The for type.
        /// </param>
        /// <returns>
        /// The <see cref="TypeRegistration"/> object for the given type.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the type has not been registered with this container
        /// </exception>
        private TypeRegistration GetRegistration(Type forType)
        {
            return this.registrations[forType];
        }

        /// <summary>
        /// A static class that contains some default IInstanceProviderFactory implementations that can be used by consumers
        /// </summary>
        public static class InstanceProviderFactories
        {
            /// <summary>
            /// The default Transient provider factory
            /// </summary>
            private static readonly Lazy<IInstanceProviderFactory> TransientFactory = new Lazy<IInstanceProviderFactory>(() => new TransientInstanceProviderFactory());

            /// <summary>
            /// The default Single provider factory
            /// </summary>
            private static readonly Lazy<IInstanceProviderFactory> SingleFactory = new Lazy<IInstanceProviderFactory>(() => new SingleInstanceProviderFactory());

            /// <summary>
            /// Gets default TransientProviderFactory, lazily-constructed
            /// </summary>
            public static IInstanceProviderFactory Transient
            {
                get
                {
                    return TransientFactory.Value;
                }
            }

            /// <summary>
            /// Gets default SingleProviderFactory, lazily-constructed
            /// </summary>
            public static IInstanceProviderFactory Single
            {
                get
                {
                    return SingleFactory.Value;
                }
            }
        }
    }
}
