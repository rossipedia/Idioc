// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionGeneratingEventArgs.cs" company="Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the ExpressionGeneratingEventArgs type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.ExpressionGenerators
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Event args for the <see cref="IDependencyVisitor.DependencyExpressionGenerating"/> event.
    /// </summary>
    public class ExpressionGeneratingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionGeneratingEventArgs"/> class.
        /// </summary>
        /// <param name="dependencyType">
        /// The dependency type.
        /// </param>
        public ExpressionGeneratingEventArgs(Type dependencyType)
        {
            this.DependencyType = dependencyType;
        }

        /// <summary>
        /// Gets the Type that needs to be resolved.
        /// </summary>
        public Type DependencyType { get; private set; }

        /// <summary>
        /// Gets or sets the expression to use to resolve the dependency.
        /// </summary>
        public Expression Expression { get; set; }
    }
}