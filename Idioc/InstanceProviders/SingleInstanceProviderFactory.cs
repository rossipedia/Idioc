// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SingleInstanceProviderFactory.cs" company="Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the SingleInstanceProviderFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.InstanceProviders
{
    using System;

    using Idioc.ExpressionGenerators;

    /// <summary>
    /// Factory for <see cref="SingleInstanceProvider"/>
    /// </summary>
    public class SingleInstanceProviderFactory : IInstanceProviderFactory
    {
        /// <summary>
        /// Creates a <see cref="SingleInstanceProvider"/> for a given type and generator
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="generator">
        /// The generator.
        /// </param>
        /// <returns>
        /// A new <see cref="SingleInstanceProvider"/> for the given type and generator
        /// </returns>
        public IInstanceProvider CreateProvider(Type type, IExpressionGenerator generator)
        {
            return new SingleInstanceProvider(type, generator);
        }
    }
}