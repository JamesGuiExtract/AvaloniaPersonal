using EnterpriseDT.Net.Ftp;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Extract.Utilities.Ftp
{
    /// <summary>
    /// Class that handles saving and loading the downloaded info file
    /// </summary>
    public class FtpDownloadedFileInfo
    {
        #region Constants

        int _VERSION = 1;

        readonly string _REMOTE_SOURCE_DOC_NAME = "RemoteSourceDocName";

        readonly string _LAST_MODIFIED = "LastModifiedTime";

        readonly string _SOURCE_DOC_SIZE = "SourceDocSize";

        readonly string _ROOT_NODE_NAME = "RemoteFileInfo";

        #endregion

        #region Properties

        /// <summary>
        /// The path that will be saved in the .info file.  This should be the entire remote path
        /// to the files on the remote server
        /// </summary>
        public string RemoteSourceDocName { get; set; }

        /// <summary>
        /// The last time the file was modified at the time it was downloaded
        /// </summary>
        public DateTime RemoteLastModifiedTime { get; set; }

        /// <summary>
        /// The size of the file at the time it was downloaded
        /// </summary>
        public long RemoteFileSize { get; set; }

        /// <summary>
        /// The path of the .info file that will be saved.
        /// </summary>
        public string InfoFileName { get; set; }

        #endregion

        #region Constructors


        /// <summary>
        /// Initializes <see cref="FtpDownloadedFileInfo"/> class for save.
        /// </summary>
        /// <param name="infoFileName">Name of the download info file</param>
        /// <param name="fileDownloaded">FTPFile record that contains the ftp info for the file</param>
        [CLSCompliant(false)]
        public FtpDownloadedFileInfo(string infoFileName, FTPFile fileDownloaded)
        {
            InfoFileName = infoFileName;

            RemoteSourceDocName = fileDownloaded.Path;
            RemoteLastModifiedTime = fileDownloaded.LastModified;
            RemoteFileSize = fileDownloaded.Size;
        }

        /// <summary>
        /// Initializes <see cref="FtpDownloadedFileInfo"/> class for load.
        /// </summary>
        /// <param name="infoFileName"></param>
        public FtpDownloadedFileInfo(string infoFileName)
        {
            InfoFileName = infoFileName;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Saves the data in a file named InfoFileName
        /// </summary>
        public void Save()
        {
            XmlTextWriter xmlWriter = null;
            try
            {
                ExtractException.Assert("ELI32655", ".info file name cannot be empty", !string.IsNullOrEmpty(InfoFileName));

                xmlWriter = new XmlTextWriter(InfoFileName, Encoding.ASCII);
                xmlWriter.Formatting = Formatting.Indented;
                xmlWriter.Indentation = 4;

                // Write the root
                xmlWriter.WriteStartElement(_ROOT_NODE_NAME);
                xmlWriter.WriteAttributeString("Version", _VERSION.ToString(CultureInfo.InvariantCulture));

                // Write the remote source document element
                xmlWriter.WriteElementString(_REMOTE_SOURCE_DOC_NAME, RemoteSourceDocName);

                xmlWriter.WriteElementString(_SOURCE_DOC_SIZE,
                    RemoteFileSize.ToString(CultureInfo.InvariantCulture));

                xmlWriter.WriteElementString(_LAST_MODIFIED,
                    RemoteLastModifiedTime.Ticks.ToString(CultureInfo.InvariantCulture));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32654");
            }
            finally
            {
                if (xmlWriter != null)
                {
                    xmlWriter.Close();
                    xmlWriter = null;
                }
            }
        }

        /// <summary>
        /// Loads the info file.
        /// </summary>
        public void Load()
        {
            try
            {
                // Reset the RemoteSourceDocName before loading
                RemoteSourceDocName = "";

                // File must exist
                if (!File.Exists(InfoFileName))
                {
                    ExtractException ee = new ExtractException("ELI32658", "Download info file does not exist.");
                    ee.AddDebugData("DownloadInfoFileName", InfoFileName, false);
                    throw ee;
                }

                // Load the XML file into a string
                string xml = File.ReadAllText(InfoFileName, Encoding.ASCII);

                // Parse the XML file
                using (StringReader reader = new StringReader(xml))
                using (XmlTextReader xmlReader = new XmlTextReader(reader))
                {
                    xmlReader.WhitespaceHandling = WhitespaceHandling.Significant;
                    xmlReader.Normalization = true;
                    xmlReader.Read();

                    if (xmlReader.Name != _ROOT_NODE_NAME)
                    {
                        ExtractException ee = new ExtractException("ELI32656", "Invalid download info file.");
                        ee.AddDebugData("Info File Name", InfoFileName, false);
                        throw ee;
                    }

                    int version = Convert.ToInt32(xmlReader.GetAttribute("Version"), CultureInfo.InvariantCulture);

                    if (version > _VERSION)
                    {
                        ExtractException ee = new ExtractException("ELI32657", "Invalid download info file version.");
                        ee.AddDebugData("Maximum version", _VERSION, false);
                        ee.AddDebugData("Version in file", version, false);
                        ee.AddDebugData("Info File Name", InfoFileName, false);
                        throw ee;
                    }

                    xmlReader.ReadStartElement();
                    RemoteSourceDocName = xmlReader.ReadElementString(_REMOTE_SOURCE_DOC_NAME);
                    RemoteFileSize = Convert.ToInt64(
                        xmlReader.ReadElementString(_SOURCE_DOC_SIZE), CultureInfo.InvariantCulture);
                    RemoteLastModifiedTime =new DateTime(Convert.ToInt64(
                        xmlReader.ReadElementString(_LAST_MODIFIED), CultureInfo.InvariantCulture));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32662");
            }
        }
        
        #endregion
    }
}
