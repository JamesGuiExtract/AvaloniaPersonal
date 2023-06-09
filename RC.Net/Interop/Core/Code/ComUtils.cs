﻿using System;
using UCLID_COMUTILSLib;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Interop
{
    /// <summary>
    /// Utility methods for interacting with Extract COM objects.
    /// </summary>
    public static class ComUtilities
    {
        /// <summary>
        /// Indicates whether the specified COM object supports configuration.
        /// </summary>
        /// <param name="objectToConfigure">The object.</param>
        /// <returns> <see langword="true"/> if the COM object is configurable; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object")]
        public static bool IsComObjectConfigurable(object objectToConfigure)
        {
            try
            {
                if (objectToConfigure is IConfigurableObject)
                {
                    return true;
                }
                else if (objectToConfigure is ISpecifyPropertyPages)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33498");
            }
        }

        /// <summary>
        /// Displays the configuration dialog for the specified COM object.
        /// </summary>
        /// <param name="objectToConfigure">The object to configure.</param>
        /// <returns>An copy of <see paramref="objectToConfigure"/> with the new settings if the
        /// user okays the configuration dialog or <see langword="null"/> if the user cancels the
        /// configuration.</returns>
        [CLSCompliant(false)]
        // objectCopy is not really cast to ICategorizedComponent multiple times. 
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public static T ConfigureComObject<T>(T objectToConfigure) where T : class
        {
            try
            {
                ICopyableObject copyThis = (ICopyableObject)objectToConfigure;
                T objectCopy = (T)(object)copyThis.Clone();
                IConfigurableObject configurableCopy = objectCopy as IConfigurableObject;

                if (configurableCopy != null)
                {
                    if (configurableCopy.RunConfiguration())
                    {
                        return objectCopy;
                    }
                }
                else if (objectCopy is ISpecifyPropertyPages)
                {
                    ICategorizedComponent categorizedObject = objectCopy as ICategorizedComponent;
                    string name = (categorizedObject == null)
                        ? "Object"
                        : categorizedObject.GetComponentDescription();

                    ObjectPropertiesUI configurationScreen = new ObjectPropertiesUIClass();
                    if (configurationScreen.DisplayProperties1(objectCopy, name + " settings"))
                    {
                        return objectCopy;
                    }
                }
                else
                {
                    ICategorizedComponent categorizedObject = objectCopy as ICategorizedComponent;
                    string name = (categorizedObject == null)
                        ? "Object"
                        : categorizedObject.GetComponentDescription();

                    throw new ExtractException("ELI33493", "Cannot configure " + name + ".");
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33492");
            }
        }
    }
}
