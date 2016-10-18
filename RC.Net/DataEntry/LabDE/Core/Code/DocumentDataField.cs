using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UCLID_AFCORELib;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// Represents a field in the voa data found by rules and/or displayed for verification.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class DocumentDataField : IDisposable
    {
        #region Fields

        /// <summary>
        /// The parent of the attribute representing this field in the voa data file.
        /// </summary>
        IAttribute _recordAttribute;

        /// <summary>
        /// The AF query used to select the attribute (either via absolute path or path relative to ParentAttribute.
        /// </summary>
        string _attributePath;

        /// <summary>
        /// The attribute representing this field's value.
        /// </summary>
        IAttribute _attribute;

        /// <summary>
        /// <c>True</c> if the <see cref="AttributeUpdated"/> even should be raised in response to
        /// updates to the attribute; otherwise, <c>False</c>.
        /// </summary>
        bool _trackChanges;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDataField"/> class.
        /// </summary>
        /// <param name="parentAttribute">The parent of the <see cref="IAttribute"/> representing
        /// this field in the voa data file.</param>
        /// <param name="attributePath">The AF query used to select the attribute (either via
        /// absolute path or path relative to <see paramref="ParentAttribute"/>.</param>
        /// <param name="trackChanges"><c>True</c> if the <see cref="AttributeUpdated"/> even should
        /// be raised in response to updates to the attribute; otherwise, <c>False</c>.</param>
        public DocumentDataField(IAttribute parentAttribute, string attributePath, bool trackChanges)
        {
            try
            {
                _attributePath = attributePath;
                _recordAttribute = parentAttribute;
                _trackChanges = trackChanges;
                Attribute = GetAttribute();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41509");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised to notify listeners that an Attribute's value was modified.
        /// </summary>
        internal event EventHandler<EventArgs> AttributeUpdated;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="IAttribute"/> that represents the
        /// <see cref="DocumentDataRecord"/> to which this field belongs.
        /// </summary>
        /// <value>
        /// The <see cref="IAttribute"/> that represents the <see cref="DocumentDataRecord"/>
        /// to which this field belongs.
        /// </value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public IAttribute RecordAttribute
        {
            get
            {
                return _recordAttribute;
            }

            set
            {
                try
                {
                    if (value != _recordAttribute)
                    {
                        Attribute = GetAttribute();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41512");
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IAttribute"/> representing this field's value.
        /// </summary>
        /// <value>
        /// The <see cref="IAttribute"/> representing this field's value.
        /// </value>
        public IAttribute Attribute
        {
            get
            {
                try
                {
                    if (_attribute == null)
                    {
                        Attribute = GetAttribute();
                    }

                    return _attribute;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41510");
                }
            }

            set
            {
                try
                {
                    if (value != _attribute)
                    {
                        if (_trackChanges && _attribute != null)
                        {
                            var statusInfo = AttributeStatusInfo.GetStatusInfo(_attribute);
                            statusInfo.AttributeValueModified -= Handle_AttributeValueModified;
                            statusInfo.AttributeDeleted -= Handle_AttributeDeleted;
                        }

                        _attribute = value;

                        if (_trackChanges && _attribute != null)
                        {
                            var statusInfo = AttributeStatusInfo.GetStatusInfo(_attribute);
                            statusInfo.AttributeValueModified += Handle_AttributeValueModified;
                            statusInfo.AttributeDeleted += Handle_AttributeDeleted;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41511");
                }
            }
        }

        /// <summary>
        /// Gets the current string value of <see cref="Attribute"/> or <see cref="string.Empty"/> if
        /// <see cref="Attribute"/> does not currently exist.
        /// </summary>
        /// <value>
        /// he current string value of <see cref="Attribute"/> or <see cref="string.Empty"/> if
        /// <see cref="Attribute"/> does not currently exist.
        /// </value>
        public string Value
        {
            get
            {
                return (Attribute != null)
                    ? Attribute.Value.String
                    : "";
            }
        }

        #endregion Properties

        #region IDisposable Members

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (_trackChanges && _attribute != null)
                    {
                        _trackChanges = false;
                        var statusInfo = AttributeStatusInfo.GetStatusInfo(_attribute);
                        statusInfo.AttributeValueModified -= Handle_AttributeValueModified;
                        statusInfo.AttributeDeleted -= Handle_AttributeDeleted;
                    }
                }
                catch { }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="AttributeStatusInfo.AttributeValueModified"/> event for
        /// <see cref="Attribute"/> if changes are being tracked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AttributeValueModifiedEventArgs"/> instance containing the event data.</param>
        void Handle_AttributeValueModified(object sender, AttributeValueModifiedEventArgs e)
        {
            if (!e.IncrementalUpdate)
            {
                OnAttributeUpdated();
            }
        }

        /// <summary>
        /// Handles the <see cref="AttributeStatusInfo.AttributeDeleted"/> event for
        /// <see cref="Attribute"/> if changes are being tracked.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AttributeDeletedEventArgs"/> instance containing the event data.</param>
        void Handle_AttributeDeleted(object sender, AttributeDeletedEventArgs e)
        {
            OnAttributeUpdated();
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets the <see cref="IAttribute"/> that should represent this field's value.
        /// </summary>
        /// <returns>The <see cref="IAttribute"/> that should represent this field's value.</returns>
        IAttribute GetAttribute()
        {
            // If attributeQuery is root-relative, set orderAttribute so that the query is not
            // evaluated relative to it.
            var queryRootAttribute = _attributePath.StartsWith("/", StringComparison.Ordinal)
                ? null
                : _recordAttribute;

            IAttribute attribute = AttributeStatusInfo
                .ResolveAttributeQuery(queryRootAttribute, _attributePath)
                .SingleOrDefault();

            return attribute;
        }

        /// <summary>
        /// Raises the <see cref="AttributeUpdated"/> event.
        /// </summary>
        void OnAttributeUpdated()
        {
            AttributeUpdated?.Invoke(this, new EventArgs());
        }

        #endregion Private Members
    }
}
