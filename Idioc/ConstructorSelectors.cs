// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConstructorSelectors.cs" company="Bryan Ross">
//   (c) Bryan Ross
// </copyright>
// <summary>
//   Defines the ConstructorSelectors type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc
{
    using System;

    using Idioc.ExpressionGenerators;

    /// <summary>
    /// A collection of default constructor selectors
    /// </summary>
    public static class ConstructorSelectors
    {
        /// <summary>
        /// Lazily-load the MostSpecificSelector
        /// </summary>
        private static readonly Lazy<IConstructorSelector> MostSpecificSelector = new Lazy<IConstructorSelector>(() => new MostSpecificConstructorSelector());

        /// <summary>
        /// Gets the default MostSpecificSelector
        /// </summary>
        public static IConstructorSelector MostSpecific
        {
            get
            {
                return MostSpecificSelector.Value;
            }
        }
    }
}