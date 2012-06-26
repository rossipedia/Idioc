// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TransientInstanceProviderFactory.cs" company="Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the TransientInstanceProviderFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.InstanceProviders
{
    using System;

    using Idioc.ExpressionGenerators;

    /// <summary>
    /// Factory for <see cref="TransientInstanceProvider"/>s
    /// </summary>
    public class TransientInstanceProviderFactory : IInstanceProviderFactory
    {
        /// <summary>
        /// Implementation of <see cref="IInstanceProviderFactory.CreateProvider"/>
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="generator">
        /// The generator.
        /// </param>
        /// <returns>
        /// A new <see cref="TransientInstanceProvider"/> for the given type and generator
        /// </returns>
        public IInstanceProvider CreateProvider(Type type, IExpressionGenerator generator)
        {
            return new TransientInstanceProvider(type, generator);
        }
    }
}