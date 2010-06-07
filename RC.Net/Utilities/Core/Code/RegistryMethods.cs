using Extract.Licensing;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Extract.Utilities
{
    /// <summary>
    /// Helper class containing registry manipulation methods.
    /// <para><b>Note:</b></para>
    /// The methods in this class, in most cases, will require the user to have
    /// administrative access.  These methods will throw exceptions if the user
    /// does not have the necessary privileges.
    /// </summary>
    public static class RegistryMethods
    {
        #region Fields

        /// <summary>
        /// Object name used for license validation calls.
        /// </summary>
        readonly static string _OBJECT_NAME = typeof(RegistryMethods).ToString();

        #endregion Fields

        #region Methods

        /// <overloads>
        /// Registers the specified file extension with the specified application.
        /// </overloads>
        /// <summary>
        /// Registers the specified file extension with the specified application.
        /// </summary>
        /// <param name="fileExtension">The file extension to register.
        /// Must not be <see langword="null"/> or <see cref="String.Empty"/></param>
        /// <param name="fileTypeDescription">The description for the file type.
        /// Must not be <see langword="null"/> or <see cref="String.Empty"/></param>
        /// <param name="fullPathToApplication">The full path to the executable that is
        /// associated with the specified file extension. Must not be <see langword="null"/>
        /// or <see cref="String.Empty"/>
        /// <para><b>Note:</b></para>
        /// This should be the full path including the file extension (.exe) for the
        /// executable.</param>
        /// <param name="skipKeyIfExists">Whether the association should be skipped if
        /// a key for the file already exists.</param>
        public static void RegisterFileAssociation(string fileExtension, string fileTypeDescription,
            string fullPathToApplication, bool skipKeyIfExists)
        {
            RegisterFileAssociation(fileExtension, fileTypeDescription, fullPathToApplication,
                skipKeyIfExists, 0);
        }
        
        /// <summary>
        /// Registers the specified file extension with the specified application.
        /// </summary>
        /// <param name="fileExtension">The file extension to register.
        /// Must not be <see langword="null"/> or <see cref="String.Empty"/></param>
        /// <param name="fileTypeDescription">The description for the file type.
        /// Must not be <see langword="null"/> or <see cref="String.Empty"/></param>
        /// <param name="fullPathToApplication">The full path to the executable that is
        /// associated with the specified file extension. Must not be <see langword="null"/>
        /// or <see cref="String.Empty"/>
        /// <para><b>Note:</b></para>
        /// This should be the full path including the file extension (.exe) for the
        /// executable.</param>
        /// <param name="skipKeyIfExists">Whether the association should be skipped if
        /// a key for the file already exists.</param>
        /// <param name="defaultIconIndex">The default index of the icon to associate with
        /// the files.</param>
        public static void RegisterFileAssociation(string fileExtension, string fileTypeDescription,
            string fullPathToApplication, bool skipKeyIfExists, int defaultIconIndex)
        {
            try
            {
                // Ensure that the extension, type description, and application path have been
                // specified.
                ExtractException.Assert("ELI30195",
                    "File extension, file type description, and application path "
                    + "must not be null or empty.",
                    !string.IsNullOrEmpty(fileExtension)
                    && !string.IsNullOrEmpty(fileTypeDescription)
                    && !string.IsNullOrEmpty(fullPathToApplication),
                    "File Extension", fileExtension, "File Type Description", fileTypeDescription,
                    "Application Path", fullPathToApplication);

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI30196", _OBJECT_NAME);

                // Ensure the extension starts with a '.'
                if (fileExtension[0] != '.')
                {
                    fileExtension = fileExtension.Insert(0, ".");
                }

                // Open the HKCR registry key
                RegistryKey root = Registry.ClassesRoot;

                // Check for skipping on existence and key existence
                if (skipKeyIfExists && RegistrySubkeyExists(root, fileExtension)
                    && RegistrySubkeyExists(root, fileTypeDescription))
                {
                    return;
                }

                // Create the extension file assocation and set the default value
                // to the file type description
                RegistryKey extension = root.CreateSubKey(fileExtension);
                extension.SetValue("", fileTypeDescription, RegistryValueKind.String);

                // Create the file type assocation subkey
                RegistryKey fileTypeAssocation = root.CreateSubKey(fileTypeDescription);
                fileTypeAssocation.SetValue("", fileTypeDescription, RegistryValueKind.String);

                // Add the default icon
                RegistryKey defaultIcon = fileTypeAssocation.CreateSubKey("DefaultIcon");
                defaultIcon.SetValue("", fullPathToApplication
                    + "," + defaultIconIndex.ToString(CultureInfo.InvariantCulture));

                // Create the shell open command subkey
                RegistryKey shellCommand =
                    fileTypeAssocation.CreateSubKey(@"shell\open\command");
                shellCommand.SetValue("", fullPathToApplication + " \"%1\"",
                    RegistryValueKind.String);

                // Commit all the changes
                shellCommand.Close();
                defaultIcon.Close();
                fileTypeAssocation.Close();
                extension.Close();
                root.Close();

                // Notify shell of file association change.
                NativeMethods.NotifyFileAssociationsChanged();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30192", ex);
            }
        }

        /// <summary>
        /// Unregisters the specified file association.
        /// </summary>
        /// <param name="fileExtension">The file extension to unregister.
        /// Must not be <see langword="null"/> or <see cref="String.Empty"/></param>
        /// <param name="fileTypeDescription">The description for the file type to unregister.
        /// Must not be <see langword="null"/> or <see cref="String.Empty"/></param>
        public static void UnregisterFileAssociation(string fileExtension,
            string fileTypeDescription)
        {
            try
            {
                ExtractException.Assert("ELI30197",
                    "File extension and file type desciption must not be null or empty.",
                    !string.IsNullOrEmpty(fileExtension)
                    && !string.IsNullOrEmpty(fileTypeDescription),
                    "File Extension", fileExtension, "File Type Description", fileTypeDescription);

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI30198", _OBJECT_NAME);

                RegistryKey root = Registry.ClassesRoot;
                root.DeleteSubKey(fileExtension, false);
                root.DeleteSubKeyTree(fileTypeDescription);
                root.Close();

                // Notify shell of file association change.
                NativeMethods.NotifyFileAssociationsChanged();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30193", ex);
            }
        }

        /// <summary>
        /// Checks if the specified sub key exists under the specified <see cref="RegistryKey"/>.
        /// </summary>
        /// <param name="key">The <see cref="RegistryKey"/> to check.</param>
        /// <param name="folderName">The sub key to look for.</param>
        /// <returns><see langword="true"/> if the key exists, and <see langword="false"/>
        /// otherwise.</returns>
        public static bool RegistrySubkeyExists(RegistryKey key, string folderName)
        {
            try
            {
                // Attempt to open the sub key
                RegistryKey temp = key.OpenSubKey(folderName);

                // Return whether the sub key was opened or not
                return temp != null;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30194", ex);
            }
        }

        #endregion Methods
    }
}
