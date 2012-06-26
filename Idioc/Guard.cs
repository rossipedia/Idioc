// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Guard.cs" company="Bryan Ross">
//   Bryan Ross
//   Notice: This class was shamelessly lifted from the Unity project @  http://unity.codeplex.com/
// </copyright>
// <summary>
//   Defines the Guard type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Idioc
{
    using System;

    /// <summary>
    /// Static Guard class for precondition checks
    /// </summary>
    internal static class Guard
    {
        /// <summary>
        /// Checks an argument for null
        /// </summary>
        /// <param name="value">
        /// The value of the argument.
        /// </param>
        /// <param name="name">
        /// The name of the argument.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is null
        /// </exception>
        public static void ArgumentNotNull(object value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}