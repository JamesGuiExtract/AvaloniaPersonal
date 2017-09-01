﻿using Extract.UtilityApplications.PaginationUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Represents data associated with an <see cref="OutputDocument"/>.
    /// </summary>
    public class PaginationDocumentData
    {
        #region Fields

        /// <summary>
        /// The attribute hierarchy (voa data) on which this instance is based.
        /// </summary>
        IUnknownVector _attributes;

        /// <summary>
        /// Indicates whether this instance was modified as of the last time the
        /// <see cref="Modified"/> accessor was checked.
        /// </summary>
        bool _modified;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationDocumentData"/> class.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/> hierarchy (voa data) on which this
        /// instance is based.</param>
        /// <param name="sourceDocName"><param name="sourceDocName">The name of the source document
        /// for which data is being loaded.</param></param>
        public PaginationDocumentData(IUnknownVector attributes, string sourceDocName)
        {
            _attributes = attributes;
            SourceDocName = sourceDocName;
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when the value of an attribute (field) is changed.
        /// </summary>
        public event EventHandler<AttributeValueChangedEventArgs> AttributeValueChanged;

        /// <summary>
        /// Raised when the value of <see cref="Modified"/> or <see cref="DataError"/> has changed.
        /// </summary>
        public event EventHandler<EventArgs> DocumentDataStateChanged;

        #endregion Events

        #region Public Members

        /// <summary>
        /// Gets a value indicating whether the data has been initialized for display.
        /// </summary>
        public virtual bool Initialized
        {
            get
            {
                // Unless overridden, data is assumed to be initialized right away.
                return true;
            }
        }

        /// <summary>
        /// The source document related to this instance if there is a singular source document;
        /// otherwise, <see langword="null"/>.
        /// </summary>
        public string SourceDocName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="IAttribute"/> hierarchy (voa data) on which this
        /// instance is based.
        /// </summary>
        public IUnknownVector Attributes
        {
            get
            {
                return _attributes;
            }

            set
            {
                try
                {
                    if (_attributes != value)
                    {
                        _attributes = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41342");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the data represented by this instance is shared in an active
        /// verification control.
        /// </summary>
        public bool DataSharedInVerification
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether this instances data has been modified.
        /// </summary>
        public virtual bool Modified
        {
            get
            {
                try
                {
                    _modified = Fields.Values.Any(
                        field => !field.TreatAsUnmodified &&
                            field.OriginalValue != GetAttributeValue(field));

                    return _modified;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39777");
                }
            }
        }

        /// <summary>
        /// Gets whether this instances data has been modified.
        /// </summary>
        public virtual bool DataError
        {
            get
            {
                // Unless overridden, the data is never considered to have an error.
                return false;
            }
        }

        /// <summary>
        /// A description of the document.
        /// </summary>
        public virtual string Summary
        {
            get
            {
                return "";
            }
        }

        /// <summary>
        /// Gets a value indicating whether editing of this data is allowed.
        /// </summary>
        /// <value><see langword="true"/> if data editing is allowed; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public virtual bool AllowDataEdit
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance wants to override whether the document
        /// is returned to the server for reprocessing.
        /// </summary>
        /// <value><see langword="null"/> to if the decision should not be overridden, otherwise
        /// a boolean value indicating what the override should be.</value>
        public virtual bool? SendForReprocessing
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Reverts the data back to its original values.
        /// </summary>
        public virtual void Revert()
        {
            try
            {
                // If Modified is false, don't attempt to revert; Modified may be intentionally
                // suppressed for a document that is open in verification.
                if (Modified)
                {
                    foreach (var field in Fields.Values)
                    {
                        SetAttributeValue(field, field.OriginalValue);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39776");
            }
        }

        /// <summary>
        /// Defines the current state of the <see cref="Attributes"/> hierarchy as the original
        /// state (used to determine the value of <see cref="Modified"/>).
        /// </summary>
        public void SetOriginalForm()
        {
            try
            {
                bool dataStateChanged = false;

                foreach (var field in Fields.Values)
                {
                    string currentValue = GetAttributeValue(field);

                    if (currentValue != field.OriginalValue)
                    {
                        field.OriginalValue = currentValue;
                        dataStateChanged = true;
                    }
                }

                if (dataStateChanged)
                {
                    OnDocumentDataStateChanged();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39772");
            }
        }

        #endregion Public Members

        #region Protected Members

        /// <summary>
        /// Maps all field names for the extending class with the <see cref="PaginationDataField"/>
        /// that defines the field.
        /// </summary>
        protected virtual Dictionary<string, PaginationDataField> Fields
        {
            get
            {
                return new Dictionary<string, PaginationDataField>();
            }
        }

        /// <summary>
        /// Gets the value of the specified <see paramref="field"/> from the current
        /// <see cref="Attributes"/> hierarchy.
        /// </summary>
        /// <param name="field">The <see cref="PaginationDataField"/> for which the value is needed.
        /// </param>
        /// <returns>The <see paramref="field"/>'s value.</returns>
        protected string GetAttributeValue(PaginationDataField field)
        {
            try
            {
                var attribute = GetCurrentAttribute(field, createIfMissing: false);

                string value = (attribute != null)
                    ? attribute.Value.String
                    : "";

                if (value != field.PreviousValue)
                {
                    field.PreviousValue = value;
                    var args = new AttributeValueChangedEventArgs(attribute);
                    OnAttributeValueChanged(args);
                    // If a handler doesn't want this change to count as a modified, flag the field
                    // to always be treated as un-modified.
                    if (field.TreatAsUnmodified == args.MarkAsModified)
                    {
                        field.TreatAsUnmodified = !args.MarkAsModified;
                        OnDocumentDataStateChanged();
                    }
                }

                return value;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39773");
            }
        }

        /// <summary>
        /// Sets the value of the specified <see paramref="field"/> from the current
        /// <see cref="Attributes"/> hierarchy to <see paramref="value"/>.
        /// </summary>
        /// <param name="field">The <see cref="PaginationDataField"/> for which the value is to be
        /// set.
        /// </param>
        /// <param name="value">The value to apply to the <see paramref="field"/>.</param>
        protected void SetAttributeValue(PaginationDataField field, string value)
        {
            try
            {
                var attribute = GetCurrentAttribute(field, createIfMissing: false);

                if (attribute == null)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        // If setting an empty value, no need to create the attribute.
                        return;
                    }
                    else
                    {
                        attribute = GetCurrentAttribute(field, createIfMissing: true);
                    }
                }

                if (attribute.Value.String != value)
                {
                    bool modified = _modified;
                    attribute.Value.ReplaceAndDowngradeToNonSpatial(value);
                    var args = new AttributeValueChangedEventArgs(attribute);
                    OnAttributeValueChanged(args);
                    // If a handler doesn't want this change to count as a modified, flag the field
                    // to always be treated as un-modified.
                    field.TreatAsUnmodified = !args.MarkAsModified;                    
                    field.PreviousValue = value;
                    if (Modified != modified)
                    {
                        OnDocumentDataStateChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39774");
            }
        }

        #endregion Protected Members

        #region Private Members

        /// <summary>
        /// Gets the current <see cref="IAttribute"/> from <see cref="Attributes"/> that is
        /// associated with the specified <see paramref="field"/>.
        /// </summary>
        /// <param name="field">The <see cref="PaginationDataField"/> for which the associated
        /// <see cref="IAttribute"/> is needed.</param>
        /// <param name="createIfMissing"><see langword="true"/> to create the attribute if is
        /// doesn't already exist; <see langword="false"/> to return <see langword="null"/> in that
        /// case.</param>
        /// <returns>The <see cref="IAttribute"/> associated with the specified
        /// <see paramref="field"/>.</returns>
        IAttribute GetCurrentAttribute(PaginationDataField field, bool createIfMissing)
        {
            try
            {
                IAttribute docTypeAttribute = field.GetAttribute(_attributes);

                if (docTypeAttribute == null && createIfMissing)
                {
                    docTypeAttribute = field.CreateAttribute(_attributes);
                }

                return docTypeAttribute;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39775");
            }
        }

        /// <summary>
        /// Raises the <see cref="AttributeValueChanged"/> event
        /// </summary>
        /// <param name="args">The <see cref="AttributeValueChangedEventArgs"/> instance containing
        /// the event data.</param>
        void OnAttributeValueChanged(AttributeValueChangedEventArgs args)
        {
            AttributeValueChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="DocumentDataChanged"/> event.
        /// </summary>
        protected void OnDocumentDataStateChanged()
        {
            DocumentDataStateChanged?.Invoke(this, new EventArgs());
        }

        #endregion Private Members
    }

    /// <summary>
    /// The type of <see cref="EventArgs"/> used for the
    /// <see cref="PaginationDocumentData.AttributeValueChanged"/> event.
    /// </summary>
    public class AttributeValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueChangedEventArgs"/> class.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose value has changed.</param>
        public AttributeValueChangedEventArgs(IAttribute attribute)
        {
            ModifiedAttribute = attribute;
            MarkAsModified = true;
        }

        /// <summary>
        /// The <see cref="IAttribute"/> whose value has changed.
        /// </summary>
        public IAttribute ModifiedAttribute
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the change will cause the
        /// <see cref="PaginationDocumentData"/> with which it is associated to be indicated as
        /// modified.
        /// </summary>
        /// <value><see langword="true"/> if the change should mark the
        /// <see cref="PaginationDocumentData"/> instance as modified; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool MarkAsModified
        {
            get;
            set;
        }
    }
}
