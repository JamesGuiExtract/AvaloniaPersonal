using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Linq;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// A component that allows a LabDE DEP to update a FAM DB's metadata fields to reflect the
    /// current document's data, whenever the document is saved.
    /// </summary>
    public class FAMMetadataManager : Component
    {
        #region Fields

        /// <summary>
        /// The <see cref="DataEntryControlHost"/> to be used to update the metadata fields.
        /// </summary>
        DataEntryControlHost _dataEntryControlHost;

        /// <summary>
        /// Specifies the FAM DB metadata fields that should be kept in sync data saved in a DEP.
        /// </summary>
        OrderedDictionary _managedFields;

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMData"/> class.  
        /// </summary>
        public FAMMetadataManager()
        {
            try
            {
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                _managedFields = new OrderedDictionary();
                _managedFields.Add("PatientFirstName", "/PatientInfo/Name/First");
                _managedFields.Add("PatientLastName", "/PatientInfo/Name/Last");
                _managedFields.Add("PatientDOB", "/PatientInfo/DOB");
                _managedFields.Add("CollectionDate", "/Test/CollectionDate");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38428");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="DataEntryControlHost"/> to be used to update the metadata
        /// fields.
        /// </summary>
        /// <value>
        /// The <see cref="DataEntryControlHost"/> to be used to update the metadata fields.
        /// </value>
        public DataEntryControlHost DataEntryControlHost
        {
            get
            {
                return _dataEntryControlHost;
            }

            set
            {
                try
                {
                    if (value != _dataEntryControlHost)
                    {
                        if (!_inDesignMode && _dataEntryControlHost != null)
                        {
                            _dataEntryControlHost.DataSaved -= HandleDataEntryControlHostDataSaved;
                        }

                        _dataEntryControlHost = value;

                        if (!_inDesignMode && _dataEntryControlHost != null)
                        {
                            _dataEntryControlHost.DataSaved += HandleDataEntryControlHostDataSaved;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38434");
                }
            }
        }

        /// <summary>
        /// Gets or sets the FAM DB metadata fields that should be kept in sync data saved in a DEP.
        /// Each field is specified via a separate line that starts with the name of the metadata
        /// field in the FAM DB followed by a colon. The remainder of the line should be the AFQuery
        /// path of the attribute(s) that should be used to update the metadata field. If there are
        /// multiple attributes matching the specified AFQuery, all unique values will be
        /// concatenated with a comma as a separator.
        /// </summary>
        /// <value>
        /// The FAM DB metadata fields that should be kept in sync data saved in a DEP.
        /// </value>
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string ManagedFields
        {
            get
            {
                try
                {
                    return FromOrderedDictionary(_managedFields);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38429");
                }
            }

            set
            {
                try
                {
                    _managedFields = ToOrderedDictionary(value);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38430");
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Updates the FAM DB's metadata fields to reflect the current document's data.
        /// </summary>
        public void UpdateMetadata()
        {
            try
            {
                ExtractException.Assert("ELI38431", "Missing DataEntryControlHost reference.",
                    DataEntryControlHost != null);

                var fileProcessingDB = DataEntryControlHost.DataEntryApplication.FileProcessingDB;
                if (fileProcessingDB == null)
                {
                    // If the DEP is running in a context that does not have a FileProcessingDB,
                    // don't throw an exception that will complicate use of the DEP. Just ignore the
                    // metadata update.
                    return;
                }

                // Ensure AttributeStatusInfo is currently initialized against the same document
                // DataEntryControlHost is.
                ExtractException.Assert("ELI38435", "Unexpected source doc name.",
                    AttributeStatusInfo.SourceDocName == DataEntryControlHost.ImageViewer.ImageFile);

                int fileId = fileProcessingDB.GetFileID(AttributeStatusInfo.SourceDocName);

                foreach (var field in _managedFields.OfType<DictionaryEntry>())
                {
                    var attributes =
                        AttributeStatusInfo.ResolveAttributeQuery(null, (string)field.Value);

                    string fieldValue = string.Join(",", attributes
                        .Select(attribute => attribute.Value.String)
                        .Distinct());

                    fileProcessingDB.SetMetadataFieldValue(fileId, (string)field.Key, fieldValue);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38432");
            }
        }

        #endregion Methods

        #region EventHandlers

        /// <summary>
        /// Handles the <see cref="E:DataEntryControlHost.DataSaved"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDataEntryControlHostDataSaved(object sender, EventArgs e)
        {
            try
            {
                UpdateMetadata();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38433");
            }
        } 

        #endregion EventHanders

        #region Private Members

        /// <summary>
        /// Converts the specified <see paramref="dictionary"/> to a string where each line is an
        /// entry. The key is the text up to the first colon on the line and the value is everything
        /// after the colon.
        /// </summary>
        /// <returns>A string representation of <see paramref="dictionary"/></returns>
        static string FromOrderedDictionary(OrderedDictionary dictionary)
        {
            return string.Join("\r\n", dictionary
                .OfType<DictionaryEntry>()
                .Select(entry => entry.Key + ": " + entry.Value));
        }

        /// <summary>
        /// Converts the specified <see paramref="description"/> to an
        /// <see cref="OrderedDictionary"/>. The description is expected to have one line for each
        /// entry. The key should be the text up to the first colon on the line and the value should
        /// be everything after the colon.
        /// </summary>
        /// <returns>The <see cref="OrderedDictionary"/> represented by <see paramref="description"/>.
        /// </returns>
        static OrderedDictionary ToOrderedDictionary(string description)
        {
            string[] rows =
                description.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var newDictionary = new OrderedDictionary();
            foreach (string columnDefintion in rows)
            {
                int colonIndex = columnDefintion.IndexOf(':');
                string key = (colonIndex >= 0)
                    ? columnDefintion.Substring(0, colonIndex).Trim()
                    : columnDefintion.Trim();
                string value = (colonIndex >= 0)
                    ? columnDefintion.Substring(colonIndex + 1).Trim()
                    : "";

                newDictionary.Add(key, value);
            }

            return newDictionary;
        }

        #endregion Private Members
    }
}
