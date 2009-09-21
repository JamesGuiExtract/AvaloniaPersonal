using Extract;
using System;
using System.Collections.Generic;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;

namespace Extract.Utilities
{
    /// <summary>
    /// A utility class of system methods.
    /// </summary>
    class SystemMethods
    {
        /// <summary>
        /// Enumerates all <see cref="ManagementObject"/>s of the specified class.
        /// </summary>
        /// <param name="wmiClass">The class name of all resulting objects.</param>
        /// <returns>An enumeration of all <see cref="ManagementObject"/>s matching the specified
        /// criteria.
        /// <para><b>Note</b></para>
        /// The caller is responsible for disposing of all <see cref="ManagementObject"/>s returned.
        /// </returns>
        public static IEnumerable<ManagementObject> GetWMIObjects(string wmiClass)
        {
            // Obtain all instances of the specified class.
            using (ManagementClass wmiObject = new ManagementClass(wmiClass))
            {
                using (ManagementObjectCollection instances = wmiObject.GetInstances())
                {
                    foreach (ManagementObject instance in instances)
                    {
                        yield return instance;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the specified property from the specified <see cref="ManagementObject"/>
        /// </summary>
        /// <param name="wmiObject">The <see cref="ManagementObject"/> to retrieve the property from.
        /// </param>
        /// <param name="propertyName">The name of the property to retrieve. Properties on
        /// see cref="ManagementObject"/>s  can be referenced with a '.' following the child
        /// object's property name.</param>
        /// <returns></returns>
        public static object GetWMIProperty(ManagementObject wmiObject, string propertyName)
        {
            try
            {
                // If a '.' is used to specify a property on a child object, obtain the child object
                // then request the property from it.
                int nextElementPos = propertyName.IndexOf('.');
                if (nextElementPos >= 0)
                {
                    string childObjectName = propertyName.Substring(0, nextElementPos);
                    using (ManagementObject childObject =
                        new ManagementObject(wmiObject[childObjectName].ToString()))
                    {
                        return GetWMIProperty(childObject,
                            propertyName.Substring(nextElementPos + 1));
                    }
                }
                // Otherwise just return the specified property.
                else
                {
                    return wmiObject[propertyName];
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27319", ex);
            }
        }
    }
}
