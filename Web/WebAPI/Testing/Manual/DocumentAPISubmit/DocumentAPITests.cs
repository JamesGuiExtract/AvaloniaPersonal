using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Extract.Web.WebAPI.DocumentAPISubmit
{
    class DocumentAPITests : ApiTester
    {
        UsersClient _userClient;
        DocumentClient _docClient;
        WorkflowClient _workflowClient;
        bool _processText;
        int _naiveAttempts = 10;

        public DocumentAPITests(string fileName, User user, string auth,
            DocumentClient docClient, WorkflowClient workflowClient, UsersClient userClient,
            int pollingInterval, int maxRetries, int naiveAttempts, bool processText)
            : base(fileName, user, auth, pollingInterval, maxRetries)
        {
            _userClient = userClient;
            _docClient = docClient;
            _workflowClient = workflowClient;
            _naiveAttempts = naiveAttempts;
            _processText = processText;

            if (string.IsNullOrWhiteSpace(auth) && user != null)
            {
                var tokenResult = userClient.LoginAsync(user).GetAwaiter().GetResult();
                this.auth = "Bearer " + tokenResult.Access_token;
            }
        }

        protected override string testerName => "Doc_Api";

        public async Task PostDocument()
        {
            Func<Task> postDocFunc = async () =>
            {
                DocumentIdResult result;

                if (Path.GetExtension(fileName).Equals(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    var text = File.ReadAllText(fileName);
                    result = await _docClient.TextPostAsync(text, auth);
                }
                else
                {
                    using (var stream = File.OpenRead(fileName))
                    {
                        var file = new FileParameter(stream, fileName);
                        result = await _docClient.DocumentPostAsync(file, auth);
                    }
                }

                this.fileId = result.Id ?? -1;
                Log("Created");
            };

            await KeepTrying(postDocFunc, retryOn404: true, logFailures: true);
        }

        public async Task RunMainSequenceTests(string statusPrefix = "")
        {
            if (!string.IsNullOrWhiteSpace(statusPrefix))
            {
                statusPrefix = statusPrefix.TrimEnd() + " ";
            }
            var tests = new List<TestInfo>()
            {
                new TestInfo(1, GetDataTest, $"{statusPrefix}getting data from", expect404: false, retryOn404: true),
                new TestInfo(2, GetDocumentTest, $"{statusPrefix}getting document from", expect404: false, retryOn404: true),
                new TestInfo(2, GetDocumentTypeTest, $"{statusPrefix}getting document type from", expect404: false, retryOn404: true),
                new TestInfo(2, GetOutputFileTest, $"{statusPrefix}getting output file for", expect404: false, retryOn404: true),
                new TestInfo(2, GetPageTextTest, $"{statusPrefix}getting page text from", expect404: false, retryOn404: true),
                new TestInfo(2, GetPageZonesTest, $"{statusPrefix}getting page zones from", expect404: false, retryOn404: true),
                new TestInfo(2, GetTextTest, $"{statusPrefix}getting text from", expect404: false, retryOn404: true),
                new TestInfo(2, PatchDataUpdateTest, $"{statusPrefix}update-patching data to", expect404: false, retryOn404: true),
                new TestInfo(2, PatchDataDeleteTest, $"{statusPrefix}delete-patching data to", expect404: false, retryOn404: true),
                new TestInfo(2, PatchDataCreateTest, $"{statusPrefix}create-patching data to", expect404: false, retryOn404: true),
                new TestInfo(2, PatchBadDataTest, $"{statusPrefix}bad-patching data to", expect404: false, retryOn404: true),
                new TestInfo(2, RestoreDataTest, $"{statusPrefix}restoring data to", expect404: false, retryOn404: true),
                new TestInfo(2, ClearDataTest, $"{statusPrefix}clearing data from", expect404: false, retryOn404: true),
                new TestInfo(2, GetPageInfoTest, $"{statusPrefix}getting page info for", expect404: fileIsText, retryOn404: !fileIsText),
                new TestInfo(2, GetOutputTextTest, $"{statusPrefix}getting output text from", expect404: !fileIsText, retryOn404: fileIsText),
                new TestInfo(2, GetDocumentStatusesTest, $"{statusPrefix}getting document statuses for", expect404: false, retryOn404: true),
                new TestInfo(2, GetWorkflowStatusTest, $"{statusPrefix}getting workflow status for", expect404: false, retryOn404: true)
            };
            if (user != null)
            {
                tests.Add(
                    new TestInfo(2, LoginTest, $"{statusPrefix}Logging in user", expect404: false, retryOn404: true));
            }
            if (_processText)
            {
                tests.Add(
                    new TestInfo(2, PostTextTest, $"{statusPrefix}posting text from", expect404: false, retryOn404: true));
            }

            await KeepTrying(() => RunTests(tests, statusPrefix), retryOn404: false, logFailures: true);
        }

        public async Task RunDeletionTests()
        {
            var tests = new List<TestInfo>()
            {
                new TestInfo(1, DeleteDocumentTest, "deleting document ", expect404: false, retryOn404: false),
                new TestInfo(2, GetDocumentTest, "getting document from", expect404: true, retryOn404: false)
            };

            await KeepTrying(() => RunTests(tests, ""), retryOn404: false, logFailures: true);
        }

        async Task LoginTest()
        {
            var tokenResult = await _userClient.LoginAsync(user);
            auth = "Bearer " + tokenResult.Access_token;
        }

        async Task GetDataTest()
        {
            DocumentDataResult data = null;

            for (int attempts = 1; attempts <= _naiveAttempts; attempts++)
            {
                Log($"Naive attempt {attempts} for ");
                try
                {
                    data = await _docClient.DataGetAsync(fileId, auth);
                }
                catch (Exception)
                {
                    Log($"Naive attempt {attempts} failed with exception for ");
                    data = null;
                }
            }

            if (data != null)
            {
                Log("Successful naive get-data for ");
            }
            else
            {
                await Log("Waiting for ", async () =>
                    await PollForCompletion());

                data = await Log("Getting data ", async () =>
                    await _docClient.DataGetAsync(fileId, auth));
            }

            origData = data;
            currentData = data;
        }

        async Task PatchDataUpdateTest()
        {
            var patch = new DocumentDataPatch()
            {
                Attributes = new DocumentAttributePatch[0]
            };

            var attr = currentData.Attributes.FirstOrDefault();
            if (attr != null)
            {
                var attrPatch = new DocumentAttributePatch(attr) { Operation = DocumentAttributePatchOperation.Update };
                attrPatch.Name = "Blah";
                patch.Attributes = new[] { attrPatch };
            }

            await Log("Patching ", async () =>
            await _docClient.DataPatchAsync(fileId, patch, auth));

            var patchedData = await Log("Getting patched data for ", async () =>
                await _docClient.DataGetAsync(fileId, auth));

            currentData = patchedData;

            if (!reprocessing &&
                attr != null &&
                !patchedData.Attributes.Any(a =>
                    a.Id == patch.Attributes.Single().Id && a.Name == patch.Attributes.Single().Name))
            {
                throw new Exception("Patch had no effect!");
            }
        }

        async Task PatchDataDeleteTest()
        {
            var patch = new DocumentDataPatch()
            {
                Attributes = new DocumentAttributePatch[0]
            };

            var attr = currentData.Attributes.FirstOrDefault();
            if (attr != null)
            {
                var attrPatch = new DocumentAttributePatch
                {
                    Id = attr.Id,
                    Operation = DocumentAttributePatchOperation.Delete
                };

                patch.Attributes = new[] { attrPatch };
            }

            await Log("Delete-patching ", async () =>
                await _docClient.DataPatchAsync(fileId, patch, auth));

            var patchedData = await Log("Getting patched data for ", async () =>
                await KeepTrying(() => _docClient.DataGetAsync(fileId, auth), retryOn404: false));

            currentData = patchedData;

            if (!reprocessing &&
                attr != null &&
                patchedData.Attributes.Any(a => a.Id == attr.Id))
            {
                throw new Exception("Delete patch had no effect!");
            }
        }

        async Task PatchDataCreateTest()
        {
            var attrPatch = new DocumentAttributePatch
            {
                ConfidenceLevel = "High",
                HasPositionInfo = false,
                Id = Guid.NewGuid().ToString(),
                Name = "MyName",
                Type = "SSN",
                Value = "123-12-1234",
                Operation = DocumentAttributePatchOperation.Create
            };

            var patch = new DocumentDataPatch
            {
                Attributes = new[] { attrPatch }
            };

            await Log("Create-patching ", async () =>
                await _docClient.DataPatchAsync(fileId, patch, auth));

            var patchedData = await Log("Getting patched data for ", async () =>
                await KeepTrying(() => _docClient.DataGetAsync(fileId, auth), retryOn404: false));

            currentData = patchedData;

            if (!reprocessing &&
                !patchedData.Attributes.Any(a => a.Id == attrPatch.Id))
            {
                throw new Exception("Create patch had no effect!");
            }
        }

        async Task PatchBadDataTest()
        {
            try
            {
                var attrPatch = new DocumentAttributePatch
                {
                    ConfidenceLevel = "High",
                    HasPositionInfo = false,
                    Id = Guid.NewGuid().ToString(),
                    Name = "MyName",
                    Type = "SSN",
                    Value = "123-12-1234",
                    Operation = DocumentAttributePatchOperation.Update
                };

                var patch = new DocumentDataPatch
                {
                    Attributes = new[] { attrPatch }
                };

                await Log("Patching Bad Data ", async () =>
                    await _docClient.DataPatchAsync(fileId, patch, auth));

                if (!reprocessing)
                {
                    throw new Exception("Lack of exception patch-updating non-existent attribute");
                }
            }
            catch (SwaggerException<ErrorResult> ex) when (ex.StatusCode == 404 && ex.Result.Error.Message.Contains("Attribute not found"))
            {
                Log(FormattableString.Invariant($"Expected exception caught for file {fileName}"));
            }
        }

        async Task ClearDataTest()
        {
            var input = new DocumentDataInput
            {
                Attributes = new DocumentAttribute[0]
            };

            await Log("Clearing data for ", async () =>
                await _docClient.DataPutAsync(fileId, input, auth));

            var clearedData = await Log("Getting cleared data for ", async () =>
                await KeepTrying(() => _docClient.DataGetAsync(fileId, auth), retryOn404: false));

            currentData = clearedData;
            if (!reprocessing && clearedData.Attributes.Count != 0)
            {
                throw new Exception(FormattableString.Invariant(
                    $"Clear data had no effect! Expecting 0 attributes but got {clearedData.Attributes.Count}"));
            }
        }

        async Task RestoreDataTest()
        {
            var input = new DocumentDataInput
            {
                Attributes = origData.Attributes
            };

            await Log("Restoring data for ", async () =>
                await _docClient.DataPutAsync(fileId, input, auth));

            var restoredData = await Log("Getting restored data for ", async () =>
                await KeepTrying(() => _docClient.DataGetAsync(fileId, auth), retryOn404: false));

            currentData = restoredData;

            if (!reprocessing &&
                !restoredData.Attributes.Select(a => a.Id)
                    .OrderBy(id => id)
                .SequenceEqual(
                    origData.Attributes.Select(a => a.Id)
                        .OrderBy(id => id)))
            {
                throw new Exception(FormattableString.Invariant(
                    $"Restore data failed! Expecting {origData.Attributes.Count} attributes but got {restoredData.Attributes.Count}"));
            }
        }

        async Task DeleteDocumentTest()
        {
            await Log($"Deleting {fileId} {fileName}", async () =>
            {
                deleted = true;
                await _docClient.DocumentDeleteAsync(fileId, auth);
            });
        }

        async Task GetDocumentTest()
        {
            var file = await Log("Getting document from ", async () =>
                await _docClient.DocumentGetAsync(fileId, auth));

            using (var ms = new MemoryStream())
            {
                file.Stream.CopyTo(ms);

                var origInfo = new FileInfo(fileName);
                if (origInfo.Length != ms.Length)
                {
                    // For debugging
                    using (var fs = new FileStream(fileName + ".wrongSize", FileMode.Create))
                    {
                        ms.Position = 0;
                        ms.CopyTo(fs);
                    }

                    throw new Exception(FormattableString.Invariant(
                        $"Retrieved file size differs from original file size! (expected: {origInfo.Length} retrieved: {ms.Length})"));
                }
            }
        }

        async Task GetDocumentTypeTest()
        {
            var docType = await Log("Getting document type from ", async () =>
                await _docClient.DocumentTypeAsync(fileId, auth));
        }

        async Task GetOutputFileTest()
        {
            var file = await Log("Getting output file for ", async () =>
                await _docClient.OutputFileAsync(fileId, auth));
        }

        async Task GetPageInfoTest()
        {
            var info = await Log("Getting page info for ", async () =>
                await _docClient.PageInfoAsync(fileId, auth));

            if (!info.PageCount.HasValue || info.PageCount == 0 || info.PageCount != info.PageInfos.Count)
            {
                throw new Exception("Unexpected discrepency in page info counts");
            }

            pageCount = info.PageCount.Value;
        }

        async Task GetPageZonesTest()
        {
            var pageNum = Math.Max(pageCount, 1);
            var zones = await Log(FormattableString.Invariant($"Getting page text from page {pageCount} of {fileName}"), async () =>
                await _docClient.WordZonesAsync(fileId, pageNum, auth));
        }

        async Task GetOutputTextTest()
        {
            await Log("Getting output text from ", async () =>
                await _docClient.OutputTextAsync(fileId, auth));
        }

        async Task GetPageTextTest()
        {
            // Use either first or last page, depending on whether GetPageInfoTest has been run already
            var pageNum = Math.Max(pageCount, 1);

            await Log(FormattableString.Invariant($"Getting page text from page {pageNum} of {fileName}"), async () =>
                await _docClient.TextGetAsync(fileId, pageNum, auth));
        }

        async Task GetTextTest()
        {
            var textResult = await Log(FormattableString.Invariant($"Getting text from {fileName}"), async () =>
                await _docClient.TextGetAsync(fileId, auth));

            if (!fileIsText && _processText)
            {
                // Use the retrieved document text as the content for a source text file in future cycles.
                // Prepend the source file name in the text so that "rules" running in a text setup can use
                // it to grab a pre-calculated voa (for tests focused on straining the API rather than rules)
                documentText = "<" + fileName + ">\r\n" +
                    string.Join("\r\n\r\n", textResult.Pages.Select(p => p.Text));

                // Write the source text file parallel to the image it came from, but only if it doesn't
                // already exist.
                var txtFileName = fileName + ".txt";
                if (!File.Exists(txtFileName))
                {
                    Log("Writing text from ", () =>
                        File.WriteAllText(txtFileName, documentText));
                }
            }
        }

        async Task PostTextTest()
        {
            // If we're already processing a text file, don't generate a duplicate text file in the database;
            // the current file was already a demonstration of post text.
            if (!fileIsText)
            {
                // Currently posting an empty string is an error so ensure there is at least one character
                if (!string.IsNullOrEmpty(documentText))
                {
                    await Log(FormattableString.Invariant($"Posting text from {fileName}"), async () =>
                        await _docClient.TextPostAsync(documentText, auth));
                }
            }
        }

        async Task GetDocumentStatusesTest()
        {
            var docStatuses = await Log("Getting workflow document statuses", async () => await _workflowClient.DocumentStatusesAsync(auth));
        }

        async Task GetWorkflowStatusTest()
        {
            var status = await Log("Getting workflow status", async () => await _workflowClient.StatusAsync(auth));
        }

        async Task PollForCompletion()
        {
            while (true)
            {
                var result = await _docClient.StatusAsync(fileId, auth);

                if (result.StatusText == "Processing")
                {
                    await Task.Delay(pollingInterval);
                }
                else if (result.StatusText == "Done")
                {
                    return;
                }
                else
                {
                    var ex = new InvalidOperationException("Unexpected status: " + result.StatusText);
                    Log(ex.Message, true);
                    throw ex;
                }
            }
        }
    }

    public partial class DocumentAttributePatch : DocumentAttribute
    {
        public DocumentAttributePatch() { }

        public DocumentAttributePatch(DocumentAttribute attribute)
        {
            ConfidenceLevel = attribute.ConfidenceLevel;
            HasPositionInfo = attribute.HasPositionInfo;
            Id = attribute.Id;
            Name = attribute.Name;
            SpatialPosition = attribute.SpatialPosition;
            Type = attribute.Type;
            Value = attribute.Value;
        }
    }
}
