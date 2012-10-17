using Extract;
using Extract.Licensing;
using Extract.Redaction;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UCLID_COMUTILSLib;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.NetDMSUtilities
{
    /// <summary>
    /// Represents and area of a document to redact.
    /// </summary>
    public struct RedactionArea : IEquatable<RedactionArea> 
    {
        /// <summary>
        /// Gets of sets the page on which the redaction should be added.
        /// </summary>
        /// <value>
        /// The page on which the redaction should be added.
        /// </value>
        public int Page
        {
            get;
            set;
        }

        /// <summary>
        /// Gets of sets the image coordinates of the area to add the redaction.
        /// </summary>
        /// <value>
        /// The image coordinates of the area to add the redaction.
        /// </value>
        public Rectangle Bounds
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            try
            {
                return Page ^ Bounds.GetHashCode();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34886");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            try
            {
                if (!(obj is RedactionArea))
                {
                    return false;
                }

                return Equals((RedactionArea)obj);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34887");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="RedactionArea"/> is equal to this
        /// <see cref="RedactionArea"/>.
        /// </summary>
        /// <param name="other">The other <see cref="RedactionArea"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="RedactionArea"/>s are equal;
        /// otherwise, <see langword="false"/></returns>
        public bool Equals(RedactionArea other)
        {
            try
            {
                if (Page != other.Page)
                {
                    return false;
                }

                return Bounds == other.Bounds;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34888");
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="redactionArea1">The first redaction area.</param>
        /// <param name="redactionArea2">The second redaction area.</param>
        /// <returns><see langword="true"/> if the <see cref="RedactionArea"/>s are equal;
        /// otherwise, <see langword="false"/></returns>
        public static bool operator ==(RedactionArea redactionArea1, RedactionArea redactionArea2)
        {
            try
            {
                return redactionArea1.Equals(redactionArea2);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34889");
            }
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="redactionArea1">The first redaction area.</param>
        /// <param name="redactionArea2">The second redaction area.</param>
        /// <returns><see langword="true"/> if the <see cref="RedactionArea"/>s are not equal;
        /// otherwise, <see langword="false"/></returns>
        public static bool operator !=(RedactionArea redactionArea1, RedactionArea redactionArea2)
        {
            try
            {
                return !redactionArea1.Equals(redactionArea2);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34890");
            }
        }    
    }

    /// <summary>
    /// A Helper class for the NetDMSCustomComponents assembly. This class contains code that cannot
    /// be run as part of this NetDMSCustomComponents assembly due to the fact that the NetDMS API
    /// (and subsequently this assembly) are not strong-named. The code that cannot be used is code
    /// which passes or returns parameter or value types that are defined in one of our COM modules.
    /// Since NetDMSCustomComponents is not strong-named it must use weak references to those COM
    /// objects, but the method signature being used will expect a strongly named type and will fail
    /// to compile. RedactionFileLoader.LoadFrom(IUnknownVector attributes, strng sourceDocument) is
    /// one example of a method that cannot be used in NetDMSCustomComponents because the
    /// signature calls for a strongly named IUnknownVector, but NetDMSCustomComponents code is only
    /// capable of providing a weakly named IUnknownVector.
    /// </summary>
    public class NetDMSMethods
    {
        #region Constants

        static readonly string _OBJECT_NAME = typeof(NetDMSMethods).ToString();

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FlexIndexIDShieldCoreObjects;

        #endregion Constants

        /// <summary>
        /// Loads redaction voa files.
        /// </summary>
        RedactionFileLoader _voaLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetDMSMethods"/> class.
        /// </summary>
        public NetDMSMethods()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI34884", _OBJECT_NAME);

                InitializationSettings settings = new InitializationSettings();
                _voaLoader = new RedactionFileLoader(settings.ConfidenceLevels);
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI34885");
            }
        }

        /// <summary>
        /// Gets the <see cref="RedactionArea"/>s to redact in <see paramref="fileName"/> based on
        /// the attributes in <see paramref="dataFileName"/>. In the case that an attribute spans
        /// multiple pages, a separate <see cref="RedactionArea"/> will be returned for each page of
        /// the redaction.
        /// </summary>
        /// <param name="fileName">The filename to be redacted.</param>
        /// <param name="dataFileName">The VOA file that contains the redactions to apply.</param>
        /// <returns>The <see cref="RedactionArea"/>s to redact in <see paramref="fileName"/>.
        /// </returns>
        public IEnumerable<RedactionArea> GetDocumentRedactionAreas(string fileName, string dataFileName)
        {
            _voaLoader.LoadFrom(dataFileName, fileName);

            foreach (SpatialString pageOfRedaction in _voaLoader.Items
                .Select(item => item.Attribute)
                .Where(attribute => attribute.Redacted)
                .SelectMany(attribute =>
                    attribute.SpatialString.GetPages()
                        .ToIEnumerable<SpatialString>()))
            {
                RedactionArea redactionArea = new RedactionArea();
                redactionArea.Page = pageOfRedaction.GetFirstPageNumber();
                ILongRectangle attributeBounds = pageOfRedaction.GetOriginalImageBounds();
                redactionArea.Bounds = Rectangle.FromLTRB(attributeBounds.Left, attributeBounds.Top,
                    attributeBounds.Right, attributeBounds.Bottom);

                yield return redactionArea;
            }
        }
    }
}
