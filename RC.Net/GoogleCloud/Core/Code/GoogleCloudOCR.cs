using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Google.Cloud.Vision.V1;
using Grpc.Auth;
using Grpc.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UCLID_COMUTILSLib;
using DataObject = Google.Apis.Storage.v1.Data.Object;

namespace Extract.GoogleCloud
{
    [CLSCompliant(false)]
    public class GoogleCloudOCR
    {
        #region Fields

        private GoogleCredential _cred;
        private StorageClient _storageClient;
        private ImageAnnotatorClient _imageAnnotatorClient;
        private readonly string _imageBucketName;
        private readonly string _outputBucketName;

        #endregion Fields

        #region Constructors

        static GoogleCloudOCR()
        {
            // Add support for secure protocols when this is run from non-CLR apps (else ServicePointManager.SecurityProtocol = Ssl3 | Tls)
            // https://extract.atlassian.net/browse/ISSUE-18425
            ServicePointManager.SecurityProtocol |=
                 SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls12
                | SecurityProtocolType.Tls13;
        }

        public GoogleCloudOCR(string credentials, string imageBucketName, string outputBucketName)
        {
            try
            {
                _imageBucketName = imageBucketName;
                _outputBucketName = outputBucketName;
                _cred = GoogleCredential.FromJson(credentials);
                string projectID = (string)JObject.Parse(credentials)["project_id"];
                _storageClient = StorageClient.Create(_cred);
                _imageAnnotatorClient = new ImageAnnotatorClientBuilder()
                {
                    ChannelCredentials = _cred.ToChannelCredentials()
                }
                .Build();

                var existingBuckets = _storageClient.ListBuckets(projectID);
                foreach (var bucket in new[] { _imageBucketName, _outputBucketName })
                {
                    if (existingBuckets.FirstOrDefault(b => b.Name == bucket) == null)
                    {
                        try
                        {
                            ExtractException uex = new ExtractException("ELI46806", "Specified bucket doesn't exist. Attempting to create...");
                            uex.AddDebugData("Project ID", projectID);
                            uex.AddDebugData("Bucket Name", bucket);
                            uex.Log();
                            _storageClient.CreateBucket(projectID, bucket);
                        }
                        catch (Exception ex)
                        {
                            throw new ExtractException("ELI46807", "Failed to create bucket", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46797");
            }
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Uploads <see paramref="inputPath"/> to google cloud storage and OCRs it. Uploaded and output files are attempted to be removed from GCS after processing completes
        /// </summary>
        /// <param name="inputPath">The TIF or PDF file to be OCRed</param>
        /// <param name="outputPath">The USS file path to write the output to. If this file already exists then it will be deleted</param>
        /// <param name="fileID">Optional ID to append to the generated cloud storage name</param>
        /// <param name="progressStatus">Optional <see cref="ProgressStatus"/> object</param>
        public void ProcessFile(string inputPath, string outputPath, int fileID = -1, ProgressStatus progressStatus = null)
        {
            try
            {
                ProcessFileAsync(inputPath, outputPath, fileID, progressStatus).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46823");
            }
        }

        /// <summary>
        /// Uploads <see paramref="inputPath"/> to google cloud storage and OCRs it. Uploaded and output files are attempted to be removed from GCS after processing completes
        /// </summary>
        /// <param name="inputPath">The TIF or PDF file to be OCRed</param>
        /// <param name="outputPath">The USS file path to write the output to. If this file already exists then it will be deleted</param>
        /// <param name="fileID">Optional ID to append to the generated cloud storage name</param>
        /// <param name="progressStatus">Optional <see cref="ProgressStatus"/> object</param>
        public async Task ProcessFileAsync(string inputPath, string outputPath, int fileID = -1, ProgressStatus progressStatus = null)
        {
            string uploadedImageName = null;
            IEnumerable<(int pageNumber, DataObject blob)> output = null;
            try
            {
                progressStatus?.InitProgressStatus("Initializing cloud OCR", 0, 10, false);

                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }

                string extension = Path.GetExtension(inputPath).ToLowerInvariant();
                string imageType = extension == ".pdf" ? "application/pdf" : "image/tiff";
                uploadedImageName = GetSafeName(fileID);
                using (var fileStream = new FileStream(inputPath, FileMode.Open))
                {
                    progressStatus?.StartNextItemGroup("Uploading image file", 1);
                    await UploadImage(fileStream, imageType, uploadedImageName);
                }

                progressStatus?.StartNextItemGroup("Waiting for recognition operation to complete", 7);
                output = await DetectDocument(uploadedImageName, imageType);

                progressStatus?.StartNextItemGroup("Saving results to file", 1);
                using (var zipArchive = ZipFile.Open(outputPath, ZipArchiveMode.Create))
                {
                    var entry = zipArchive.CreateEntry("0000.DocumentInfo.json");
                    using (var entryStream = entry.Open())
                    using (var sw = new StreamWriter(entryStream))
                    {
                        var json = new JObject { { "SourceDocName", inputPath } };
                        sw.Write(json);
                    }

                    foreach (var (pageNumber, blob) in output)
                    {
                        string page = pageNumber.ToString("D4", CultureInfo.InvariantCulture);
                        entry = zipArchive.CreateEntry(page + ".gcv v1.json");
                        using (var entryStream = entry.Open())
                        {
                            _storageClient.DownloadObject(blob, entryStream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46822");
            }
            finally
            {
                progressStatus?.StartNextItemGroup("Cleaning up temporary objects", 1);

                // Delete output
                if (output != null)
                {
                    foreach (var (_, blob) in output)
                    {
                        try
                        {
                            await DeleteStorageObject(blob.Bucket, blob.Name);
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractLog("ELI46824");
                        }
                    }
                }

                // Delete uploaded image
                if (uploadedImageName != null)
                {
                    try
                    {
                        await DeleteStorageObject(_imageBucketName, uploadedImageName);
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI46825");
                    }
                }

                progressStatus?.CompleteCurrentItemGroup();
            }
        }

        #endregion Public Methods

        #region Private Methods

        private async Task<string> UploadImage(Stream image, string contentType, string name)
        {
            var imageAcl = PredefinedObjectAcl.Private;

            var imageObject = await _storageClient.UploadObjectAsync(
                bucket: _imageBucketName,
                objectName: name,
                contentType: contentType,
                source: image,
                options: new UploadObjectOptions { PredefinedAcl = imageAcl }
            );

            return imageObject.MediaLink;
        }

        private async Task DeleteStorageObject(string bucketName, string name)
        {
            try
            {
                await _storageClient.DeleteObjectAsync(bucketName, name);
            }
            catch (Google.GoogleApiException exception) when (exception.Error.Code == 404)
            { }
        }

        private async Task<List<(int pageNumber, DataObject blob)>> DetectDocument(string imageName, string imageType)
        {
            var sourceUri = "gs://" + _imageBucketName + "/" + imageName;
            var destinationUri = "gs://" + _outputBucketName + "/" + imageName;
            var asyncRequest = new AsyncAnnotateFileRequest
            {
                InputConfig = new InputConfig
                {
                    GcsSource = new GcsSource { Uri = sourceUri },
                    MimeType = imageType
                },
                OutputConfig = new OutputConfig
                {
                    // How many pages should be grouped into each json output file.
                    BatchSize = 1,
                    GcsDestination = new GcsDestination { Uri = destinationUri }
                }
            };

            asyncRequest.Features.Add(new Feature { Type = Feature.Types.Type.DocumentTextDetection });

            // NOTE: Currently this only accepts a single request in the list
            var requests = new List<AsyncAnnotateFileRequest> { asyncRequest };
            var operation = _imageAnnotatorClient.AsyncBatchAnnotateFiles(requests);

            var response = await operation.PollUntilCompletedAsync(new Google.Api.Gax.PollSettings(Google.Api.Gax.Expiration.FromTimeout(TimeSpan.FromMinutes(10)), TimeSpan.FromSeconds(1)));

            if (response.IsFaulted)
            {
                throw new ExtractException("ELI46813", "Operation failed", response.Exception);
            }

            var blobList = _storageClient.ListObjects(_outputBucketName, imageName);

            return (from blob in blobList
                    let match = Regex.Match(blob.Name, @"\1(?=-to-(\d+)\.json\z)", RegexOptions.RightToLeft)
                    where match.Success
                    let pageNum = int.Parse(match.Value)
                    orderby pageNum
                    select (pageNum, blob)).ToList();
        }

        private static string GetSafeName(int fileID)
        {
            return Guid.NewGuid().ToString() + "_FamFileID=" + fileID.ToString(CultureInfo.InvariantCulture);
        }

        #endregion Private Methods
    }
}
