// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContainerExtensions.Initializers.cs" company="Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the ContainerExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc
{
    using System;
    using System.Linq.Expressions;

    using Idioc.Exceptions;

    /// <summary>
    /// Extension methods for registering initializers for types
    /// </summary>
    public static partial class ContainerExtensions
    {
        /// <summary>
        /// Adds an initializer for a registered type.
        /// The type must be registered before an initializer
        /// can be added.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        /// <param name="initializer">
        /// The intializer.
        /// </param>
        /// <typeparam name="TAbstract">
        /// The registered type to add this initializer for
        /// </typeparam>
        public static void AddInitializer<TAbstract>(this Container container, Action<TAbstract> initializer)
        {
            Guard.ArgumentNotNull(container, "container");
            Guard.ArgumentNotNull(initializer, "intializer");

            if (!container.IsRegistered(typeof(TAbstract)))
            {
                throw new TypeNotRegisteredException(typeof(TAbstract));
            }

            // Build expression from initializer
            var initTarget = initializer.Target != null ? Expression.Constant(initializer.Target) : null;
            var registration = container.GetRegistration(typeof(TAbstract));

            registration.Generator.ExpressionGenerated += (sender, args) =>
            {
                // This will call expression, and pass it to the initializer
                Func<TAbstract, TAbstract> initFunc = instance =>
                {
                    initializer(instance);
                    return instance;
                };
                args.Expression = Expression.Invoke(Expression.Constant(initFunc), args.Expression);
            };
        }
    }
}
