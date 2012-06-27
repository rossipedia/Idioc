// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConstructorSelector.cs" company="Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the IConstructorSelector interface
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.ExpressionGenerators
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Selects a constructor.
    /// </summary>
    public interface IConstructorSelector
    {
        /// <summary>
        /// Selects a constructor
        /// </summary>
        /// <param name="type">
        /// The type to select a constructor from
        /// </param>
        /// <returns>
        /// The selected constructor
        /// </returns>
        ConstructorInfo SelectConstructor(Type type);
    }
}