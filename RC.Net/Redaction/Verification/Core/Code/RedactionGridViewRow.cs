using Extract.Imaging.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents the row of a <see cref="RedactionGridView"/>.
    /// </summary>
    public class RedactionGridViewRow
    {
        #region RedactionGridViewRow Fields

        /// <summary>
        /// The layer object to which the row corresponds.
        /// </summary>
        readonly LayerObject _layerObject;

        /// <summary>
        /// The text of the redaction.
        /// </summary>
        readonly string _text;

        /// <summary>
        /// The category of the redaction (e.g. Man, Clue, etc.)
        /// </summary>
        readonly string _category;

        /// <summary>
        /// The type of the redaction (e.g. SSN, Driver's license number, etc.)
        /// </summary>
        string _type;

        /// <summary>
        /// Exemptions codes associated with the redaction.
        /// </summary>
        ExemptionCodeList _exemptions;

        #endregion RedactionGridViewRow Fields

        #region RedactionGridViewRow Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactionGridViewRow"/> class.
        /// </summary>
        public RedactionGridViewRow(LayerObject layerObject, string text, string category, string type)
        {
            _layerObject = layerObject;
            _text = text;
            _category = category;
            _type = type;
            _exemptions = new ExemptionCodeList();
        }

        #endregion RedactionGridViewRow Constructors

        #region RedactionGridViewRow Properties

        /// <summary>
        /// Gets the layer object associated with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <returns>The layer object associated with the <see cref="RedactionGridViewRow"/>.
        /// </returns>
        public LayerObject LayerObject
        {
            get
            {
                return _layerObject;
            }
        }

        /// <summary>
        /// Gets the text assocaited with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <returns>The text assocaited with the <see cref="RedactionGridViewRow"/>.</returns>
        public string Text
        {
            get
            {
                return _text;
            }
        }

        /// <summary>
        /// Gets the category associated with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <returns>The category associated with the <see cref="RedactionGridViewRow"/>.</returns>
        public string Category
        {
            get
            {
                return _category;
            }
        }

        /// <summary>
        /// Gets or sets the type associated with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <value>The type associated with the <see cref="RedactionGridViewRow"/>.</value>
        /// <returns>The type associated with the <see cref="RedactionGridViewRow"/>.</returns>
        public string RedactionType
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        /// <summary>
        /// Gets the page number associated with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <returns>The page number associated with the <see cref="RedactionGridViewRow"/>.
        /// </returns>
        public string PageNumber
        {
            get
            {
                return _layerObject.PageNumber.ToString(CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets or sets the exemption codes associated with the <see cref="RedactionGridViewRow"/>.
        /// </summary>
        /// <value>The exemption codes associated with the <see cref="RedactionGridViewRow"/>.
        /// </value>
        /// <returns>The exemption codes associated with the <see cref="RedactionGridViewRow"/>.
        /// </returns>
        public ExemptionCodeList Exemptions
        {
            get
            {
                return _exemptions;
            }
            set
            {
                _exemptions = value;
            }
        }

        #endregion RedactionGridViewRow Properties
    }
}
