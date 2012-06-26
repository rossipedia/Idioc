// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DependencyResolutionException.cs" company="Bryan Ross">
//   (c) 2012 Bryan Ross
// </copyright>
// <summary>
//   Defines the DependencyResolutionException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.Exceptions
{
    using System;

    /// <summary>
    /// The exception thrown when a dependency can not be resolved
    /// </summary>
    [Serializable]
    public class DependencyResolutionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyResolutionException"/> class.
        /// </summary>
        /// <param name="dependencyType">
        /// The dependency type.
        /// </param>
        public DependencyResolutionException(Type dependencyType)
            : base(string.Format("Could not resolve dependency for type: {0}", dependencyType.FullName))
        {
        }
    }
}