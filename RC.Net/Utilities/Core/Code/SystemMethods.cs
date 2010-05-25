using Extract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;

namespace Extract.Utilities
{
    /// <summary>
    /// A utility class of system methods.
    /// </summary>
    public static class SystemMethods
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
        internal static IEnumerable<ManagementObject> GetWmiObjects(string wmiClass)
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
        internal static object GetWmiProperty(ManagementBaseObject wmiObject, string propertyName)
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
                        return GetWmiProperty(childObject,
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

        /// <summary>
        /// Checks to see if the specified process is running, returns <see langword="true"/> if
        /// the process is running and <see langword="false"/> if it is not.
        /// </summary>
        /// <param name="processId">The process ID to look for.</param>
        /// <returns><see langword="true"/> if the process is running and
        /// <see langword="false"/> if it is not running.</returns>
        public static bool IsProcessRunning(int processId)
        {
            return IsProcessRunning(null, processId);
        }

        /// <summary>
        /// Checks to see if the specified process is running, returns <see langword="true"/> if
        /// the process is running and <see langword="false"/> if it is not.
        /// </summary>
        /// <param name="processId">The process ID to look for.</param>
        /// <param name="processName">The name of the process to look for.</param>
        /// <returns><see langword="true"/> if the process is running and
        /// <see langword="false"/> if it is not running.</returns>
        public static bool IsProcessRunning(string processName, int processId)
        {
            try
            {
                try
                {
                    using (Process process = Process.GetProcessById(processId))
                    {

                        // If a process name is specified, return whether the
                        // process names are equal
                        if (!string.IsNullOrEmpty(processName))
                        {
                            return process.ProcessName.Equals(processName,
                                StringComparison.OrdinalIgnoreCase);
                        }

                        // Found the process so it is running
                        return true;
                    }
                }
                catch
                {
                    // Exception indicates that the specified process ID is not running
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28771", ex);
            }
        }

        /// <summary>
        /// Gets the process name for the specified process ID.
        /// </summary>
        /// <param name="processId">The process ID to search for.</param>
        /// <returns>The name for the specified process ID.</returns>
        /// <exception cref="ExtractException">If the specified process ID is
        /// not currently running.</exception>
        public static string GetProcessName(int processId)
        {
            try
            {
                using (Process process = Process.GetProcessById(processId))
                {
                    return process.ProcessName;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29807", ex);
            }
        }

        /// <summary>
        /// Gets the current process's ID.
        /// </summary>
        /// <returns>The ID for the current process.</returns>
        public static int GetCurrentProcessId()
        {
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    return process.Id;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30136", ex);
            }
        }
    }
}
