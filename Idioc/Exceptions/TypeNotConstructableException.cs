// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeNotConstructableException.cs" company="Bryan Ross">
//   (c) 2012 Bryan Ross
// </copyright>
// <summary>
//   Defines the TypeNotConstructableException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.Exceptions
{
    using System;

    /// <summary>
    /// The exception thrown when a type cannot be constructed
    /// </summary>
    [Serializable]
    public class TypeNotConstructableException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotConstructableException"/> class.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        public TypeNotConstructableException(Type type) 
            : base(string.Format("The type {0} is not constructable. Either it is abstract, an interface, or has no public constructors", type.FullName))
        {
            // noop
        }
    }
}