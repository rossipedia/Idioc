// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IInstanceProviderFactory.cs" company="Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the IInstanceProviderFactory interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.InstanceProviders
{
    using System;

    using Idioc.ExpressionGenerators;

    /// <summary>
    /// Abstract factory interface for IInstanceProvider
    /// </summary>
    public interface IInstanceProviderFactory
    {
        /// <summary>
        /// Returns an <see cref="IInstanceProvider"/> for a given type
        /// using a given <see cref="IExpressionGenerator"/>.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="generator">
        /// The generator.
        /// </param>
        /// <returns>
        /// An <see cref="IInstanceProvider"/> that can be used to provide 
        /// instances of the given type.
        /// </returns>
        IInstanceProvider CreateProvider(Type type, IExpressionGenerator generator);
    }
}