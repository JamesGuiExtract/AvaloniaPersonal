using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Reflection;
using System.Linq;

namespace Extract.Utilities
{
    /// <summary>
    /// A class containing utility helper methods
    /// </summary>
    public static class UtilityMethods
    {
        /// <summary>
        /// Swaps two value types in place.
        /// </summary>
        /// <typeparam name="T">The type of objects being swapped.</typeparam>
        /// <param name="valueOne">The first value to swap.</param>
        /// <param name="valueTwo">The second value to swap.</param>
        // These values are pass by reference because we are 'swapping' them in place. The
        // result of the swap method is that the two values are swapped. In order for this
        // to be reflected after the call to this method the objects must be passed as a
        // reference.
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#")]
        public static void Swap<T>(ref T valueOne, ref T valueTwo) where T : struct
        {
            try
            {
                T c = valueOne;
                valueOne = valueTwo;
                valueTwo = c;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30146", ex);
            }
        }

        /// <summary>
        /// Alls the types that implement interface.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>An array of <see cref="Type"/> objects that implement the
        /// specified interface <paramref name="interfaceType"/>.</returns>
        public static Type[] AllTypesThatImplementInterface(Type interfaceType,
            params Assembly[] assemblies)
        {
            try
            {
                if (!interfaceType.IsInterface)
                {
                    throw new ArgumentException("Type to find must be an interface.",
                        "interfaceType");
                }
                if (assemblies == null)
                {
                    throw new ArgumentNullException("assemblies");
                }

                return assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(p => p.IsClass && interfaceType.IsAssignableFrom(p))
                    .ToArray();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31131", ex);
            }
        }
    }
}
