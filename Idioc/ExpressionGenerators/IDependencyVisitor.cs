// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDependencyVisitor.cs" company="Bryan Ross">
//   (c) 2012 Bryan Ross
// </copyright>
// <summary>
//   Defines the DependencyResolutionException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.ExpressionGenerators
{
    using System;

    /// <summary>
    /// This interface is used to visit dependencies of a given type
    /// </summary>
    public interface IDependencyVisitor
    {
        /// <summary>
        /// This event is raised when dependencies for a given type are discovered.
        /// Each dependency is visited, allowing consumers to resolve them by
        /// supplying an expression that can be used to return an instance of the
        /// dependency type.
        /// </summary>
        event EventHandler<ExpressionGeneratingEventArgs> DependencyExpressionGenerating;
    }
}