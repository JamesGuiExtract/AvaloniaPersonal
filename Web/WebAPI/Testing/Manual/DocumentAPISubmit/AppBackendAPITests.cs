using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Extract.Web.WebAPI.DocumentAPISubmit
{
    class AppBackendAPITests : ApiTester
    {
        AppBackendClient _backendClient;
        string _sessionToken = "[Invalid]";
        int _currentPage = 1;
        List<DocumentAttribute> _currentPageData = new List<DocumentAttribute>();
        int _totalPages = -1;
        int _maxDelayBeforeNextPage = 5000;

        public AppBackendAPITests(User user, string auth, AppBackendClient backendClient, int pollingInterval, int maxRetries)
            : base(null, user, auth, pollingInterval, maxRetries)
        {
            _backendClient = backendClient;

            if (string.IsNullOrWhiteSpace(auth) && user != null)
            {
                var tokenResult = backendClient.LoginAsync(user).GetAwaiter().GetResult();
                this.auth = "Bearer " + tokenResult.Access_token;
            }
        }

        protected override string testerName => "Backend";

        public async Task GetSessionAsync()
        {    
            Func<Task> getSessionFunc = async () =>
            {
                WebAppSettingsResult settingsResult = null;

                try
                {
                    settingsResult = await Log("Getting settings", async () =>
                        await _backendClient.SettingsAsync(_sessionToken));
                }
                catch (SwaggerException ex) when (ex.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    var loginResult = await Log("Logging in", async () =>
                        await _backendClient.SessionLoginAsync(auth));
                    _sessionToken = "Bearer " + loginResult.Access_token;

                    settingsResult = await Log("Getting settings", async () =>
                        await _backendClient.SettingsAsync(_sessionToken));
                }
            };

            await KeepTrying(getSessionFunc, retryOn404: true, logFailures: true);
        }

        public async Task RunDocumentTests()
        {
            await OpenDocumentAsync();
            await RunDocumentInitializationTestsAsync();
            // Test accessing pages both sequentially and randomly for up to 10 pages per document
            for (int i = 0; i < 10 && i < _totalPages; i++)
            {
                await RunDocumentPageTestsAsync();
            }
            await RunDocumentCommitTestsAsync();
        }

        public async Task OpenDocumentAsync()
        {
            for (int i = 0; i < maxRetries; i++)
            {
                Func<Task> openDocFunc = async () =>
                {
                    DocumentIdResult documentIdResult;
                    try
                    {
                        documentIdResult = await Log("Opening document", async () =>
                            await _backendClient.OpenDocumentAsync(fileId, false, _sessionToken));
                    }
                    catch (SwaggerException ex) when (ex.StatusCode == (int)HttpStatusCode.Unauthorized)
                    {
                        var loginResult = await Log("Logging in", async () =>
                            await _backendClient.SessionLoginAsync(auth));
                        _sessionToken = "Bearer " + loginResult.Access_token;

                        documentIdResult = await Log("Opening document", async () =>
                            await _backendClient.OpenDocumentAsync(fileId, false, _sessionToken));
                    }

                    fileId = documentIdResult.Id.Value;
                };

                await KeepTrying(openDocFunc, retryOn404: true, logFailures: true);

                if (fileId > 0)
                {
                    _currentPage = 1;
                    _totalPages = -1;
                    break;
                }
                else
                {
                    Log($"No document available; retrying ({i + 1}) in {pollingInterval}ms... ");
                    await Task.Delay(pollingInterval);
                }
            }

            if (fileId == -1)
            {
                var sessionToken = _sessionToken;
                _sessionToken = "[Invalid]";
                fileName = null;

                Log("No file is available", error: true);

                await Log("Logging out", async () =>
                    await _backendClient.LogoutAsync(sessionToken));

                throw new Exception("No file is available");
            }
        }

        public async Task CloseDocumentAsync()
        {
            await Log("Closing ", async () =>
            {
                await _backendClient.CloseDocumentAsync(fileId, true, null, _sessionToken);
                fileId = -1;
                fileName = null;
                _currentPage = -1;
                _totalPages = -1;
            });
        }

        public async Task RunDocumentInitializationTestsAsync()
        {
            var tests = new List<TestInfo>()
            {
                new TestInfo(1, GetMetadataTest, $"getting metadata", expect404: false, retryOn404: false),
                new TestInfo(1, GetDocumentDataTest, $"getting document data", expect404: false, retryOn404: true),
                new TestInfo(1, GetUncommittedDataTest, $"getting uncommitted data", expect404: false, retryOn404: true),
                new TestInfo(1, GetPageInfoTest, $"getting page info", expect404: false, retryOn404: true),
                new TestInfo(2, GetDocumentPageTest, $"getting document page {_currentPage} ", expect404: false, retryOn404: false),
                new TestInfo(2, GetDocumentPageWordZonesTest, $"getting page {_currentPage} word zones", expect404: false, retryOn404: false),
                new TestInfo(3, GetCommentTest, $"getting comment", expect404: false, retryOn404: false),
            };

            await KeepTrying(() => RunTests(tests, ""), retryOn404: false, logFailures: true);
        }

        public async Task RunDocumentCommitTestsAsync()
        {
            var tests = new List<TestInfo>()
            {
                new TestInfo(1, PostDocumentDataTest, $"posting document data", expect404: false, retryOn404: false),
                new TestInfo(1, PutMetadataFieldTest, $"putting metadata", expect404: false, retryOn404: true),
                new TestInfo(2, DeleteUncommittedDataTest, $"deleting uncommitted data", expect404: false, retryOn404: true),
                new TestInfo(3, CloseDocumentAsync, $"closing document", expect404: false, retryOn404: true)
            };

            await KeepTrying(() => RunTests(tests, ""), retryOn404: false, logFailures: true);
        }

        public async Task RunDocumentPageTestsAsync()
        {
            var tests = new List<TestInfo>()
            {
                new TestInfo(1, GetDocumentPageTest, $"getting document page {_currentPage} ", expect404: false, retryOn404: false),
                new TestInfo(1, GetDocumentPageWordZonesTest, $"getting page {_currentPage} word zones", expect404: false, retryOn404: false),
                new TestInfo(1, GetPutPageDataTest, $"putting page {_currentPage} data", expect404: false, retryOn404: false),
                new TestInfo(1, ProcessAnnotationTest, $"processing {_currentPage} annotation", expect404: false, retryOn404: false)
            };

            await KeepTrying(() => RunTests(tests, ""), retryOn404: false, logFailures: true);

            bool accessSequentialPage =
                (_currentPage < _totalPages - 1)
                && rng.Next(1, 3) == 1;  // 2/3 probability to access next page if not on last page
            _currentPage = accessSequentialPage
                ? _currentPage + 1
                : rng.Next(1, _totalPages);

            await Task.Delay(rng.Next(0, _maxDelayBeforeNextPage));
        }

        async Task GetMetadataTest()
        {
            try
            {
                var metadataFieldResult = await Log("Getting filename metadata", async () =>
                    await _backendClient.MetadataFieldGetAsync(fileId, "OriginalFileName", _sessionToken));
                fileName = string.IsNullOrWhiteSpace(metadataFieldResult.Value)
                    ? "[No OriginalFileName]"
                    : metadataFieldResult.Value;
            }
            catch (SwaggerException ex) when (ex.StatusCode == (int)HttpStatusCode.NotFound)
            {
                fileName = "[No OriginalFileName]";
            }
        }

        async Task GetCommentTest()
        {
            await Log($"Getting comment for ", async () =>
                await _backendClient.CommentGetAsync(fileId, _sessionToken));
        }

        async Task GetDocumentDataTest()
        {
            await Log("Getting data for ", async () =>
                await _backendClient.DocumentDataGetAsync(fileId, _sessionToken));
        }

        async Task GetUncommittedDataTest()
        {
            await Log("Getting uncommited data for ", async () =>
                await _backendClient.UncommittedDocumentDataGetAsync(fileId, _sessionToken));
        }

        async Task DeleteUncommittedDataTest()
        {
            await Log("Delete uncommited data for ", async () =>
                await _backendClient.UncommittedDocumentDataDeleteAsync(fileId, _sessionToken));
        }

        async Task GetPageInfoTest()
        {
            var pageInfoResult = await Log("Getting page info for ", async () =>
                await _backendClient.PageInfoAsync(fileId, _sessionToken));

            if (!pageInfoResult.PageCount.HasValue || pageInfoResult.PageCount.Value < 1)
            {
                throw new Exception($"Unexpected page count: {pageInfoResult.PageCount}");
            }
            else
            {
                _totalPages = pageInfoResult.PageCount.Value;
            }
        }

        async Task GetDocumentPageTest()
        {
            await Log($"Getting document page {_currentPage} for ", async () =>
                await _backendClient.DocumentPageAsync(fileId, _currentPage, _sessionToken));
        }

        async Task GetDocumentPageWordZonesTest()
        {
            await Log($"Getting page {_currentPage} word zones for ", async () =>
                await _backendClient.PageWordZonesAsync(fileId, _currentPage, _sessionToken));
        }

        async Task GetPutPageDataTest()
        {
            var newPageData = new DocumentAttribute[]
            {
                new DocumentAttribute()
                {
                    ConfidenceLevel = "High",
                    HasPositionInfo = false,
                    Id = Guid.NewGuid().ToString(),
                    Name = "MyName",
                    Type = "SSN",
                    Value = "123-12-1234"
                }
            };

            await Log($"Putting page {_currentPage} data ", async () =>
                await _backendClient.DocumentDataPutAsync(fileId, _currentPage, newPageData, _sessionToken));
        }

        async Task ProcessAnnotationTest()
        {
            var annotationToProcess = new ProcessAnnotationParameters()
            {
                OperationType = "modify",
                Definition = "{ AutoShrinkRedactionZones: { } }",
                Annotation = new DocumentAttribute()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "ProcessAnnotationTest",
                    HasPositionInfo = true,
                    SpatialPosition = new Position()
                    {
                        Pages = new[] { _currentPage },
                        LineInfo = new[]
                        {
                            new SpatialLine()
                            {
                                SpatialLineZone = new SpatialLineZone()
                                { 
                                    PageNumber = _currentPage,
                                    StartX = 100,
                                    StartY = 100,
                                    EndX = 300,
                                    EndY = 100,
                                    Height = 25
                                }
                            }
                        }
                    }
                }

            };

            await Log($"Putting page {_currentPage} data ", async () =>
                await _backendClient.ProcessAnnotationAsync(fileId, _currentPage, annotationToProcess, _sessionToken));
        }

        async Task PostDocumentDataTest()
        {
            await Log($"Posting document data for ", async () =>
                await _backendClient.DocumentDataPostAsync(fileId, _sessionToken));
        }

        async Task PutMetadataFieldTest()
        {
            await Log($"Putting metadata for ", async () =>
                await _backendClient.MetadataFieldPutAsync(fileId, "DocumentType", "Tested", _sessionToken));
        }
    }
}
