// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MostSpecificConstructorSelector.cs" company="(c) Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the MostSpecificConstructorSelector type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.ExpressionGenerators
{
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Selects the most-specific (largest number of parameters) constructor for a type
    /// </summary>
    public class MostSpecificConstructorSelector : IConstructorSelector
    {
        /// <summary>
        /// Selects the constructor with the largest number of parameters specified
        /// </summary>
        /// <param name="type">
        /// The type to select the constructor for.
        /// </param>
        /// <returns>
        /// The selected constructor, or null if no constructor is available.
        /// </returns>
        public ConstructorInfo SelectConstructor(Type type)
        {
            var constructors = from constructor in type.GetConstructors()
                               orderby constructor.GetParameters().Length descending
                               select constructor;

            return constructors.FirstOrDefault();
        }
    }
}