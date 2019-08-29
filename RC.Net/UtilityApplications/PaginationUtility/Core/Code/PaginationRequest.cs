using Extract.AttributeFinder;
using Extract.Imaging;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using UCLID_AFCORELib;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Whether a pagination request was specified automatically (via rules) or specified in verification.     
    /// </summary>
    public enum PaginationRequestType
    {
        Automatic,
        Verified
    }

    /// <summary>
    /// Represents a request to create a new document by paginating existing document(s).
    /// </summary>
    public class PaginationRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationRequest"/> class.
        /// </summary>
        /// <param name="requestType">The <see cref="PaginationRequestType"/> of this request.</param>
        /// <param name="fileTaskSessionId">The file task session ID from the process that generated the request.</param>
        /// <param name="outputFileId">The ID of the output file generated.</param>
        /// <param name="imagePages">The source document <see cref="ImagePage"/>s representing each
        /// successive page in the requested pagination output document.</param>
        public PaginationRequest(PaginationRequestType requestType, int fileTaskSessionId, int outputFileId, IEnumerable<ImagePage> imagePages)
        {
            PaginationRequestType = requestType;
            FileTaskSessionId = fileTaskSessionId;
            OutputFileId = outputFileId;
            ImagePages = (imagePages ?? new ImagePage[0]).ToList().AsReadOnly();
            RequestDateTime = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationRequest"/> class.
        /// </summary>
        /// <param name="requestAttribute">And <see cref="IAttribute"/> representing the pagination request.</param>
        public PaginationRequest(IAttribute requestAttribute)
        {
            PaginationRequestType = (PaginationRequestType)
                TypeDescriptor.GetConverter(typeof(PaginationRequestType))
                    .ConvertFromString(
                        AttributeMethods.GetSingleAttributeByName(requestAttribute.SubAttributes, "Type").Value.String);
            FileTaskSessionId = int.Parse(
                AttributeMethods.GetSingleAttributeByName(requestAttribute.SubAttributes, "SourceTaskSessionID").Value.String,
                CultureInfo.InvariantCulture);
            OutputFileId = int.Parse(
                AttributeMethods.GetSingleAttributeByName(requestAttribute.SubAttributes, "OutputFileId").Value.String,
                CultureInfo.InvariantCulture);

            var requestDateTimeAttribute = AttributeMethods.GetSingleAttributeByName(requestAttribute.SubAttributes, "RequestDateTime");
            RequestDateTime = (requestDateTimeAttribute == null)
                ? DateTime.Now
                : DateTime.Parse(
                    AttributeMethods.GetSingleAttributeByName(requestAttribute.SubAttributes, "RequestDateTime").Value.String,
                    CultureInfo.InvariantCulture);

            var pageAttributes = AttributeMethods.GetAttributesByName(requestAttribute.SubAttributes, "Page");
            ImagePages = pageAttributes.AsEnumerable<IAttribute>()
                .Select(pageAttribute => pageAttribute.GetAsImagePage())
                .ToList()
                .AsReadOnly();
        }

        /// <summary>
        /// Gets or sets the <see cref="PaginationRequestType"/>.
        /// </summary>
        public PaginationRequestType PaginationRequestType { get; set; } = PaginationRequestType.Verified;

        /// <summary>
        /// Gets or sets the file task session ID from the process that generated the request.
        /// </summary>
        public int FileTaskSessionId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the output file generated.
        /// </summary>
        public int OutputFileId { get; set; }

        /// <summary>
        /// Gets or sets the DateTime of the pagination request
        /// </summary>
        public DateTime RequestDateTime { get; set; }

        /// <summary>
        /// Gets or sets the ID of the output file generated.
        /// </summary>
        public ReadOnlyCollection<ImagePage> ImagePages { get; private set; }

        /// <summary>
        /// Returns an <see cref="IAttribute"/> representing this instance.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IAttribute GetAsAttribute()
        {
            try
            {
                var requestAttribute = new AttributeClass() { Name = "PaginationRequest" };

                requestAttribute.AddSubAttribute("Type", PaginationRequestType.ToString());
                requestAttribute.AddSubAttribute("SourceTaskSessionID", FileTaskSessionId.ToString(CultureInfo.InvariantCulture));
                requestAttribute.AddSubAttribute("OutputFileID", OutputFileId.ToString(CultureInfo.InvariantCulture));
                requestAttribute.AddSubAttribute("RequestDateTime", RequestDateTime.ToString("G", CultureInfo.InvariantCulture));

                requestAttribute.SubAttributes.Append(
                    ImagePages.Select((page, pageNum) => page.GetAsAttribute(++pageNum))
                    .ToIUnknownVector<IAttribute>());

                return requestAttribute;
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI46830");
            }
        }
    }
}
