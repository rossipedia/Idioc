// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeNotRegisteredException.cs" company="Bryan Ross">
//   (c) 2012 Bryan Ross
// </copyright>
// <summary>
//   Defines the TypeNotRegisteredException type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc.Exceptions
{
    using System;

    /// <summary>
    /// The exception thrown when a type has not been registered with the container
    /// </summary>
    [Serializable]
    public class TypeNotRegisteredException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeNotRegisteredException"/> class.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        public TypeNotRegisteredException(Type type)
            : base(string.Format("The Type {0} has not been registered with the container", type.FullName))
        {
        }
    }
}