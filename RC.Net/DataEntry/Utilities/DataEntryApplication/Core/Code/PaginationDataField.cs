using Extract.Utilities;
using System.Collections.Generic;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// Represents a data field in an <see cref="PaginationDocumentData"/> instance.
    /// </summary>
    public class PaginationDataField
    {
        /// <summary>
        /// An <see cref="AFUtility"/> instance used to locate the <see cref="IAttribute"/>
        /// associated with this instance in the attribute hierarchy.
        /// </summary>
        AFUtility _afUtility = new AFUtility();

        /// <summary>
        /// A path to the associated <see cref="IAttribute"/> defined in terms of the attribute name
        /// for each level in the hierarchy starting at the root and ending with the attribute name
        /// itself.
        /// </summary>
        List<string> _attributePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationDataField"/> class.
        /// </summary>
        /// <param name="attributePath">A path to the associated <see cref="IAttribute"/> defined
        /// in terms of the attribute name for each level in the hierarchy starting at the root and
        /// ending with the attribute name itself.</param>
        public PaginationDataField(params string[] attributePath)
        {
            _attributePath = new List<string>(attributePath);
        }

        /// <summary>
        /// Gets the name of the <see cref="IAttribute"/> associated with this field.
        /// </summary>
        /// <value>
        /// The name of the attribute.
        /// </value>
        public string AttributeName
        {
            get
            {
                return _attributePath.Last();
            }
        }

        /// <summary>
        /// Gets the AFQuery to select the associated <see cref="IAttribute"/> from the attribute
        /// hierarchy.
        /// </summary>
        public string Query
        {
            get
            {
                return string.Join("/", _attributePath);
            }
        }

        /// <summary>
        /// Gets or sets the original value of this field.
        /// </summary>
        /// <value>The original value of this field.</value>
        public string OriginalValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the last known value of this field.
        /// </summary>
        /// <value>The last known value of this field.</value>
        public string PreviousValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this field should be treated as unmodified even
        /// if the currently value differs from <see cref="OriginalValue"/>.
        /// </summary>
        /// <value><see langword="true"/> to treat as un-modified regardless of whether its value
        /// has changed; otherwise, <see langword="false"/>.
        /// </value>
        public bool TreatAsUnmodified
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="IAttribute"/> associated with this instance.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/> associated with this instance.
        /// </param>
        /// <returns>The <see cref="IAttribute"/> associated with this instance.</returns>
        public IAttribute GetAttribute(IUnknownVector attributes)
        {
            try
            {
                return _afUtility
                    .QueryAttributes(attributes, Query, bRemoveMatches: false)
                    .ToIEnumerable<IAttribute>()
                    .FirstOrDefault();
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI39778");
            }
        }

        /// <summary>
        /// Creates the <see cref="IAttribute"/> associated with this field if it doesn't already
        /// exist.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/> hierarchy in which the attribute
        /// should be created.</param>
        /// <returns>The <see cref="IAttribute"/> (whether it needed to be created or already
        /// existed).</returns>
        public IAttribute CreateAttribute(IUnknownVector attributes)
        {
            try
            {
                // Used track of full path to the attribute, one name per level, that currently
                // exists.
                var workingStack = new Stack<string>(_attributePath);
                // Used track all attributes that must be created, one name per level, including the
                // final target attribute.
                var toCreate = new Stack<string>();
                IUnknownVector creationVector = attributes;
                IAttribute targetAttribute = null;

                // Start at the target hierarchy, the work upward until a particular root of
                // destination path exists.
                while (workingStack.Count > 0)
                {
                    string query = string.Join("/", workingStack.Reverse());

                    targetAttribute = _afUtility
                        .QueryAttributes(attributes, query, bRemoveMatches: false)
                        .ToIEnumerable<IAttribute>()
                        .FirstOrDefault();

                    if (targetAttribute == null)
                    {
                        toCreate.Push(workingStack.Pop());
                    }
                    else
                    {
                        creationVector = targetAttribute.SubAttributes;
                        break;
                    }
                }

                // Create each attribute that needs to be created.
                while (toCreate.Count > 0)
                {
                    targetAttribute = new AttributeClass();
                    targetAttribute.Name = toCreate.Pop();
                    creationVector.PushBack(targetAttribute);
                    creationVector = targetAttribute.SubAttributes;
                }

                return targetAttribute;
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI39779");
            }
        }
    }
}
