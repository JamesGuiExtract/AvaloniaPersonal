using Extract.Interop;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

using ComStringPair = UCLID_COMUTILSLib.StringPair;
using StringPair = Extract.Utilities.StringPair;

namespace Extract.Redaction.CustomComponentsHelper
{
    /// <summary>
    /// Interface for the create redacted image advanced label settings
    /// </summary>
    [ComVisible(true)]
    [Guid("E5A24491-E863-4BD2-9EA2-E8429D5BEEC9")]
    [CLSCompliant(false)]
    public interface ICreateRedactedImageAdvancedLabelSettings
    {
        /// <summary>
        /// Gets an IUnknownVector of IStringPairs representing the
        /// text to replace and the replacement text.
        /// </summary>
        [CLSCompliant(false)]
        IUnknownVector Replacements { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the redaction text case should be
        /// auto adjusted based on the words current context.
        /// </summary>
        /// <value>
        ///	<see langword="true"/> if the case should be adjusted; otherwise, <see langword="false"/>.
        /// </value>
        bool AutoAdjustCase { get; set; }

        /// <summary>
        /// Gets or sets the text that the first instance of a type should be prefixed with.
        /// </summary>
        /// <value>
        /// The text that the first instance of a type should be prefixed with.
        /// </value>
        string PrefixFirstInstance { get; set; }

        /// <summary>
        /// Gets or sets the text that the first instance of a type should be suffixed with.
        /// </summary>
        /// <value>
        /// The text that the first instance of a type should be suffixed with.
        /// </value>
        string SuffixFirstInstance { get; set; }
    }

    /// <summary>
    /// Class that defines the advanced label settings
    /// </summary>
    [ComVisible(true)]
    [Guid("2F5F7E44-3FA5-4619-BB50-CE1FD686528F")]
    [ProgId("Extract.Redaction.CreateRedactedImageAdvancedLabelSettings")]
    public class CreateRedactedImageAdvancedLabelSettings : IConfigurableObject,
        ICreateRedactedImageAdvancedLabelSettings, ICopyableObject, IPersistStream
    {
        #region Constants

        /// <summary>
        /// Current version of the settings object.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// Name of the object used in license validation calls.
        /// </summary>
        static readonly string _COMPONENT_NAME =
            typeof(CreateRedactedImageAdvancedLabelSettings).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The collection of replacement strings
        /// </summary>
        List<StringPair> _replacementStrings = new List<StringPair>();

        /// <summary>
        /// Whether or not the casing of the text should be auto-adjusted.
        /// </summary>
        bool _autoAdjustCase;

        /// <summary>
        /// The text to use to prefix the first instance of a type.
        /// </summary>
        string _prefixFirstInstance;

        /// <summary>
        /// The text to use to suffix the first instance of a type.
        /// </summary>
        string _suffixFirstInstance;

        /// <summary>
        /// Indicates whether this object is dirty or not.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRedactedImageAdvancedLabelSettings"/> class.
        /// </summary>
        public CreateRedactedImageAdvancedLabelSettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRedactedImageAdvancedLabelSettings"/> class.
        /// </summary>
        /// <param name="settings">The settings to initialize from.</param>
        public CreateRedactedImageAdvancedLabelSettings(
            CreateRedactedImageAdvancedLabelSettings settings)
        {
            try
            {
                CopyFrom(settings);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31708");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the collection of replacement pairs.
        /// </summary>
        public ReadOnlyCollection<StringPair> ReplacementStrings
        {
            get
            {
                return _replacementStrings.AsReadOnly();
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Validates the license.
        /// </summary>
        /// <param name="eliCode">The eli code.</param>
        static void ValidateLicense(string eliCode)
        {
            LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects,
                eliCode, _COMPONENT_NAME);
        }

        #endregion Methods

        #region IConfigurableObject

        /// <summary>
        /// Runs the configuration.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully
        /// run (dialog result was <see cref="DialogResult.OK"/>),
        /// <see langword="false"/> otherwise.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                ValidateLicense("ELI31785");

                using (var dialog = new CreateRedactedImageAdvanceLabelingDialog(this))
                {
                    var result = dialog.ShowDialog() == DialogResult.OK;
                    if (result)
                    {
                        _replacementStrings = new List<StringPair>(
                            dialog.ReplacementStrings);
                        _autoAdjustCase = dialog.AutoAdjustCase;
                        _prefixFirstInstance = dialog.PrefixText;
                        _suffixFirstInstance = dialog.SuffixText;
                        _dirty = true;
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31709",
                    "Unable to run configuration for advanced redaction text settings.");
            }
        }

        #endregion IConfigurableObject

        #region ICreateRedactedImageAdvancedLabelSettings

        /// <summary>
        /// Gets an IUnknownVector of IStringPairs representing the
        /// text to replace and the replacement text.
        /// </summary>
        [CLSCompliant(false)]
        public IUnknownVector Replacements
        {
            get
            {
                try
                {
                    var vector = new IUnknownVector();
                    foreach (var pair in _replacementStrings)
                    {
                        var stringPair = new ComStringPair();
                        stringPair.SetKeyValuePair(pair.First, pair.Second);
                        vector.PushBack(stringPair);
                    }

                    return vector;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI31710", "Unable to get replacement strings.");
                }
            }
            set
            {
                try
                {
                    ValidateLicense("ELI31788");

                    // Get the size of the vector
                    var size = value.Size();
                    _replacementStrings = new List<StringPair>(size);

                    // Get each pair from the vector and add to the new list
                    for (int i = 0; i < size; i++)
                    {
                        var pair = (IStringPair)value.At(i);
                        string key;
                        string val;
                        pair.GetKeyValuePair(out key, out val);
                        _replacementStrings.Add(new StringPair()
                            {
                                First = key,
                                Second = val
                            });
                    }

                    // Replace the existing list
                    _dirty = true;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI31711", "Unable to set replacement strings.");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the redaction text case should be
        /// auto adjusted based on the words current context.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if the case should be adjusted; otherwise, <see langword="false"/>.
        /// </value>
        public bool AutoAdjustCase
        {
            get
            {
                return _autoAdjustCase;
            }
            set
            {
                try
                {
                    ValidateLicense("ELI31789");

                    _dirty |= _autoAdjustCase != value;
                    _autoAdjustCase = value;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI31790",
                        "Unable to set the auto adjust case property.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the text that the first instance of a type should be prefixed with.
        /// </summary>
        /// <value>
        /// The text that the first instance of a type should be prefixed with.
        /// </value>
        public string PrefixFirstInstance
        {
            get
            {
                return _prefixFirstInstance ?? string.Empty;
            }
            set
            {
                try
                {
                    ValidateLicense("ELI31791");

                    if (!value.Equals(_prefixFirstInstance, StringComparison.Ordinal))
                    {
                        _prefixFirstInstance = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI31792",
                        "Unable to set the prefix first instance property.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the text that the first instance of a type should be suffixed with.
        /// </summary>
        /// <value>
        /// The text that the first instance of a type should be suffixed with.
        /// </value>
        public string SuffixFirstInstance
        {
            get
            {
                return _suffixFirstInstance ?? string.Empty;
            }
            set
            {
                try
                {
                    ValidateLicense("ELI31793");

                    if (!value.Equals(_suffixFirstInstance, StringComparison.Ordinal))
                    {
                        _suffixFirstInstance = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI31794",
                        "Unable to set the suffix first instance property.");
                }
            }
        }

        #endregion ICreateRedactedImageAdvancedLabelSettings

        #region ICopyableObject

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A new instance with the same settings as this instance.</returns>
        public object Clone()
        {
            try
            {
                ValidateLicense("ELI31786");
                return new CreateRedactedImageAdvancedLabelSettings(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31712", "Unable to clone object.");
            }
        }

        /// <summary>
        /// Copies from the specified object.
        /// </summary>
        /// <param name="pObject">The p object.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var settings = (CreateRedactedImageAdvancedLabelSettings)pObject;
                CopyFrom(settings);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31713", "Unable to copy object.");
            }
        }

        /// <summary>
        /// Copies from the specified object.
        /// </summary>
        /// <param name="settings">The settings.</param>
        [ComVisible(false)]
        public void CopyFrom(CreateRedactedImageAdvancedLabelSettings settings)
        {
            try
            {
                ValidateLicense("ELI31787");

                _replacementStrings = new List<StringPair>(
                    settings.ReplacementStrings);
                _autoAdjustCase = settings.AutoAdjustCase;
                _prefixFirstInstance = settings.PrefixFirstInstance;
                _suffixFirstInstance = settings.SuffixFirstInstance;
                _dirty = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31714");
            }
        }

        #endregion ICopyableObject

        #region IPersistStream

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
        /// <returns>
        /// <see cref="HResult.Ok"/> if changes have been made;
        /// <see cref="HResult.False"/> if changes have not been made.
        /// </returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the IStream where it was previously saved.
        /// </summary>
        /// <param name="stream">IStream from which the object should be loaded.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    // Read the count of list items
                    var count = reader.ReadInt32();

                    // Read the list items
                    var list = new List<StringPair>(count);
                    for (int i = 0; i < count; i++)
                    {
                        var key = reader.ReadString();
                        var value = reader.ReadString();
                        list.Add(new StringPair()
                            {
                                First = key,
                                Second = value
                            });
                    }

                    // Read other settings
                    bool autoAdjust = reader.ReadBoolean();
                    string prefixText = reader.ReadString();
                    string suffixText = reader.ReadString();

                    // Update settings
                    _replacementStrings = list;
                    _autoAdjustCase = autoAdjust;
                    _prefixFirstInstance = prefixText;
                    _suffixFirstInstance = suffixText;
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI31715",
                    "Unable to load Redaction task advanced label settings from stream.");
            }
        }

        /// <summary>
        /// Saves an object into the specified IStream and indicates whether the
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream">IStream into which the object should be saved.</param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    // Write the count of items in the list first
                    writer.Write(_replacementStrings.Count);

                    // Write each pair of replacement strings
                    foreach (var pair in _replacementStrings)
                    {
                        writer.Write(pair.First);
                        writer.Write(pair.Second);
                    }
                    writer.Write(_autoAdjustCase);
                    writer.Write(_prefixFirstInstance);
                    writer.Write(_suffixFirstInstance);

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
                throw ex.CreateComVisible("ELI31716",
                    "Unable to save Redaction task advanced label settings to stream.");
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

        #endregion IPersistStream
    }
}
