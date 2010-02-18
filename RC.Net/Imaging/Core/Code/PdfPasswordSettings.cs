using Extract.Encryption;
using Extract.Interop;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

namespace Extract.Imaging
{
    /// <summary>
    /// Enum for defining the owner permission set
    /// </summary>
    [ComVisible(true)]
    [Guid("3ED3B445-6F78-4FD2-92E4-ECA33CD6E994")]
    [Flags]
    // The zero value in an enum should be called "None" according to this rule,
    // but in order to make the values more clear in this enum DisallowAll is a better
    // name than "None"
    [SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum PdfOwnerPermissions
    {
        /// <summary>
        /// All options allowed
        /// </summary>
        DisallowAll = 0x0,

        /// <summary>
        /// Allow low quality printing.
        /// </summary>
        AllowLowQualityPrinting = 0x1,

        /// <summary>
        /// Allow high quality printing.
        /// </summary>
        AllowHighQualityPrinting = 0x2,

        /// <summary>
        /// Allow modifications to document.
        /// </summary>
        AllowDocumentModifications = 0x4,

        /// <summary>
        /// Allow copying of document content (copy text and images). Also allows
        /// accessiblity access.
        /// </summary>
        AllowContentCopying = 0x8,

        /// <summary>
        /// Allow copying of content for accessibility access (screen readers, etc).
        /// </summary>
        AllowContentCopyingForAccessibility = 0x10,

        /// <summary>
        /// Allow adding/modifying document annotations.
        /// </summary>
        AllowAddingModifyingAnnotations = 0x20,

        /// <summary>
        /// Alllow form fields to be filled in.
        /// </summary>
        AllowFillingInFields = 0x40,

        /// <summary>
        /// Allow document assembly.
        /// </summary>
        AllowDocumentAssembly = 0x80,
        
        /// <summary>
        /// Disallows all permissions.
        /// </summary>
        AllowAll = 0x80 + 0x40 + 0x20 + 0x10 + 0x8 + 0x4 + 0x2 + 0x1
    }

    /// <summary>
    /// Class for storing PDF password settings to be applied to PDF files when saving.
    /// </summary>
    [ComVisible(true)]
    [Guid("F794C26A-23A6-4653-876E-ABEC72FB8C08")]
    [ProgId("Extract.Imaging.PdfPasswordSettings")]
    public class PdfPasswordSettings : IConfigurableObject, ICopyableObject, IPersistStream
    {
        #region Constants

        /// <summary>
        /// The current version of this object.
        /// Versions:
        /// 1. Initial version
        /// </summary>
        const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The user password to add to the PDF file.
        /// </summary>
        string _userPassword;

        /// <summary>
        /// The owner password to add to the PDF file.
        /// </summary>
        string _ownerPassword;

        /// <summary>
        /// The permissions to set when setting the owner password.
        /// </summary>
        PdfOwnerPermissions _permissions;

        /// <summary>
        /// Whether this object is dirty or not.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="PdfPasswordSettings"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPasswordSettings"/> class.
        /// </summary>
        public PdfPasswordSettings() : this(null, null, PdfOwnerPermissions.AllowAll)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPasswordSettings"/> class.
        /// </summary>
        /// <param name="userPassword">The user password to apply to the document.</param>
        /// <param name="ownerPassword">The owner password to apply to the document.</param>
        /// <param name="permissions">The owner permissions to apply to the document.</param>
        public PdfPasswordSettings(string userPassword, string ownerPassword,
            PdfOwnerPermissions permissions)
        {
            try
            {
                _userPassword = userPassword ?? "";
                _ownerPassword = ownerPassword ?? "";
                _permissions = permissions;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29722",
                    "Failed to initialize 'Pdf password settings' object.", ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfPasswordSettings"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="PdfPasswordSettings"/> object to use
        /// to initialize this object.</param>
        // FxCop is complaining about _dirty being set to false, but it is being set to
        // true in the CopyFrom method.  Since this is a constructor the object should
        // not be considered dirty when it is constructed so we need to set _dirty
        // back to false after the call to CopyFrom
        [SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public PdfPasswordSettings(PdfPasswordSettings settings)
        {
            try
            {
                if (settings != null)
                {
                    CopyFrom(settings);
                    _dirty = false;
                }
                else
                {
                    _permissions = PdfOwnerPermissions.AllowAll;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29723",
                    "Failed to initialize 'Pdf password settings' object.", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets/sets the user password to be applied to the PDF. If <see langword="null"/>
        /// or empty then no user password will be applied.
        /// </summary>
        /// <value>The user password to be applied to the PDF.</value>
        public string UserPassword
        {
            get
            {
                return _userPassword;
            }
            set
            {
                _userPassword = value ?? "";
                _dirty = true;
            }
        }

        /// <summary>
        /// Gets/sets the owner password to be applied to the PDF. If <see langword="null"/>
        /// or empty then no owner password will be applied.
        /// </summary>
        /// <value>The owner password to be applied to the PDF.</value>
        public string OwnerPassword
        {
            get
            {
                return _ownerPassword;
            }
            set
            {
                _ownerPassword = value ?? "";
                _dirty = true;
            }
        }

        /// <summary>
        /// Gets/sets the owner permissions to be applied to the PDF. If <see cref="OwnerPassword"/>
        /// is <see langword="null"/> or empty then no permissions will be applied.
        /// </summary>
        public PdfOwnerPermissions OwnerPermissions
        {
            get
            {
                return _permissions;
            }
            set
            {
                _permissions = value;
                _dirty = true;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Copies the specified <see cref="PdfPasswordSettings"/> instance into this one.
        /// </summary>
        /// <param name="settings">The <see cref="PdfPasswordSettings"/> from which to copy.</param>
        public void CopyFrom(PdfPasswordSettings settings)
        {
            try
            {
                _userPassword = settings.UserPassword;
                _ownerPassword = settings.OwnerPassword;
                _permissions = settings.OwnerPermissions;
                _dirty = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29724", ex);
            }
        }

        #endregion Methods

        #region IConfigurableObject Members

        /// <summary>
        /// Displays a <see cref="PdfPasswordSettingsDialog"/> that is used to configure
        /// this object.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                using (PdfPasswordSettingsDialog dialog = new PdfPasswordSettingsDialog(this))
                {
                    // Show the dialog and save settings if appropriate
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dialog.Settings);

                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29725",
                    "Unable to configure 'Pdf password settings' object.", ex);
            }
        }

        #endregion

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="PdfPasswordSettings"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="PdfPasswordSettings"/> instance.</returns>
        public object Clone()
        {
            try
            {
                return new PdfPasswordSettings(this);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29735",
                    "Failed to clone 'Pdf password settings' object.", ex);
            }
        }

        /// <summary>
        /// Copies the specified <see cref="PdfPasswordSettings"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                CopyFrom((PdfPasswordSettings)pObject);

            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29726",
                    "Failed to copy 'Pdf password settings' object.", ex);
            }
        }

        #endregion

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = GetType().GUID;
        }

        /// <summary>
        /// Checks the object for changes since it was last saved.
        /// </summary>
        /// <returns><see cref="HResult.Ok"/> if changes have been made; 
        /// <see cref="HResult.False"/> if changes have not been made.</returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the <see cref="IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    // Read the settings
                    MapLabel mapLabel = new MapLabel();
                    _userPassword = ExtractEncryption.DecryptString(reader.ReadString(), mapLabel);
                    _ownerPassword = ExtractEncryption.DecryptString(reader.ReadString(), mapLabel);
                    _permissions = (PdfOwnerPermissions)reader.ReadInt32();
                }


                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29727",
                    "Unable to load 'Pdf password settings' object.", ex);
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="IStream"/> and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> into which the object should be saved.
        /// </param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If 
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(IStream stream, bool clearDirty)
        {
            try 
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    // Serialize the settings
                    MapLabel mapLabel = new MapLabel();
                    writer.Write(ExtractEncryption.EncryptString(_userPassword, mapLabel));
                    writer.Write(ExtractEncryption.EncryptString(_ownerPassword, mapLabel));
                    writer.Write((int)_permissions);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI29728",
                    "Unable to save 'Pdf password settings' object.", ex);
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in 
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            size = HResult.NotImplemented;
        }

        #endregion
    }
}
