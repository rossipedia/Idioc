// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IInstanceProvider.cs" company="Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the IInstanceProvider interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.InstanceProviders
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides an instance when asked for one.
    /// </summary>
    public interface IInstanceProvider
    {
        /// <summary>
        /// Gets Expression used to provide the instance.
        /// </summary>
        Expression Expression { get; }

        /// <summary>
        /// Returns an object instance.
        /// </summary>
        /// <returns>
        /// An instance of an object.
        /// </returns>
        object GetInstance();
    }
}