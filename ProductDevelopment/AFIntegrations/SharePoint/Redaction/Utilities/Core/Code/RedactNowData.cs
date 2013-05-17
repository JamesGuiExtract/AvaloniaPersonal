using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Extract.SharePoint.Redaction.Utilities
{
    /// <summary>
    /// Helper class to pass data from SharePoint to the redact now helper application.
    /// </summary>
    [DataContract(Name="RedactNowData", Namespace="http://www.extractsystems.com")]
    public sealed class RedactNowData
    {
        #region Constants

        /// <summary>
        /// Constant string containing the end point name for the
        /// ID Shield for SP client application channel.
        /// </summary>
        public static readonly string IdShieldForSPClientEndpoint = "029C2C6C-C9C9-4761-839B-0FBF97725661";

        /// <summary>
        /// Constant for the port to open when creating the connection.
        /// </summary>
        public static readonly string IdShieldClientPort = "9077";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Gets or sets the site URL.
        /// </summary>
        /// <value>The site URL.</value>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        [DataMember(IsRequired=true)]
        public string SiteUrl { get; set; }

        /// <summary>
        /// Gets or sets the list id.
        /// </summary>
        /// <value>The list id.</value>
        [DataMember(IsRequired=true)]
        public Guid ListId { get; set; }

        /// <summary>
        /// Gets or sets the file id.
        /// </summary>
        /// <value>The file id.</value>
        [DataMember(IsRequired=true)]
        public Guid FileId { get; set; }

        /// <summary>
        /// Gets or sets the FPS file location.
        /// </summary>
        /// <value>The FPS file location.</value>
        [DataMember(IsRequired=true)]
        public string FpsFileLocation { get; set; }
        
        /// <summary>
        /// Gets or sets the working folder location
        /// </summary>
        [DataMember(IsRequired=true)]
        public string WorkingFolder { get; set; }

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactNowData"/> class.
        /// </summary>
        public RedactNowData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactNowData"/> class.
        /// </summary>
        /// <param name="data">The data to copy from.</param>
        public RedactNowData(RedactNowData data)
        {
            SiteUrl = data.SiteUrl;
            ListId = data.ListId;
            FileId = data.FileId;
            FpsFileLocation = data.FpsFileLocation;
            WorkingFolder = data.WorkingFolder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactNowData"/> class.
        /// </summary>
        /// <param name="siteUrl">The site URL.</param>
        /// <param name="listId">The list id.</param>
        /// <param name="fileId">The file id.</param>
        /// <param name="fpsFileLocation">The FPS file location.</param>
        /// <param name="workingFolder">The working folder that contains the file to be verifyed for the verify process</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId="0#")]
        public RedactNowData(string siteUrl, string listId, string fileId, string fpsFileLocation, string workingFolder = "")
            : this(siteUrl, new Guid(listId), new Guid(fileId), fpsFileLocation, workingFolder)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactNowData"/> class.
        /// </summary>
        /// <param name="siteUrl">The site URL.</param>
        /// <param name="listId">The list id.</param>
        /// <param name="fileId">The file id.</param>
        /// <param name="fpsFileLocation">The FPS file location.</param>
        /// <param name="workingFolder">The working folder that contains the file to be verifyed for the verify process</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId="0#")]
        public RedactNowData(string siteUrl, Guid listId, Guid fileId, string fpsFileLocation, string workingFolder = "")
        {
            SiteUrl = siteUrl;
            ListId = listId;
            FileId = fileId;
            FpsFileLocation = fpsFileLocation;
            WorkingFolder = workingFolder;
        }

        #endregion Constructors
    }
}
