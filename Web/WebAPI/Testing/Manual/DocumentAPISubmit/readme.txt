This utility is used to test the DocumentAPI, AppBackedAPI or both: 
- When docurl is specified, this test utility will post all documents in the specified source directory (recursive) via DocumentAPI and execute DocumentAPI calls.
- When backendurl is specified, this test utility will grab files from the very queue and test AppBackendAPI calls

DocumentAPI:
- The test utility will test many API calls on each document, processing multiple documents at a time (defined by batchSize; default 50). As each document completes the series of tests, the next source file will be grabbed until all documents in the provided source directory have been processed.
- On the initial loop thru the source files, a txt document will be produced alongside the images in the source directory where the contents is the OCR data for the document pre-pended by the name of the source image. This is the only change the utility will make to files in the source directory. On subsequent loops through the images, the text from these files will be submitted as source documents that are execised by tests similar to the tests for source image files.
- After posting a file, the utility will make 10 immediate "naive" attempts to get the output data for the file in a tight loop. It is not necessary for the output data to be available in that time; once the naive attempts complete, the utility will begin polling document status at the defined polling frequency (default = 10 seconds) until the document completes procesing.
- After the API reports the document to have completed processing and makes the data available, the remaining API calls will be exercised against the document.
- After processing all documents in the source directory, the utility will begin submitting them again unless minTimeToRun has been exceeded (default 15 minutes)

The utility assumes the following of an active FAM workflow:
- Posted files shoule be run thru rules (or a rule subsitute) will be running that:
	- Saves redaction rule output to the ruleset defined as the workflow's output attribute set
	- Sets to complete the actions required for the file to be considered complete in the workflow
	- Work on image files as well as text files.
	- Text files will be generated for each source document pre-pended with the source filename from which they came. This may be useful in to "fake" rule results for a test that focuses on the API calls and not rules execution).
	- Generate and output file (both for posted images and posted text) based on the data found by the rules

AppBackendAPI:
- To test the AppBackendAPI, the utility will be looking for queued files (either via the DocumentAPI tests or another process that is setting files to pending in verify)
- For each document the test will:
	- Open a document session
	- Exercise API calls that get document related info
	- For the first page, then up to 10 subsequent pages that are chosen in sequence most of the time, but at random others:
		- Exercise API calls that get document page related info and the update data for the page
	- Document data will be commited, cached data managed, then then document will be closed

NOTE: When running both DocumentAPI tests and AppBackendAPI tests at the same time:
- Set reprocessrandom to false; Randomly pulling files back into processing will generate document lock contention issues with the AppBackend that will be checking out those files
- The DocumentAPI put/patch tests will report lots errors periodically due to unexpected voa data as a result of the AppBackendAPI tests. Commenting out tests that throw "had no effect!" messages will result in better test reliability. In the future, this utility should automatically make this adjustment.

NOTE: If rules are being run (vs simulating rule execution) and the documents submitted include large documents, ensure pollint x maxretries adds up to enough time to account for these files to have gone thru rules.

NOTE: It can be helpful to pipe main console output to a file via the ">" operator (e.g.: > Logs\TestUtil\Output.txt), or the error output to a different file via "2>" (e.g. 2> 2> Logs\TestUtil\Errors.txt). However, output to files can cause contention amongst different utility threads that bogs down the speed of execution, especially when the primary output is being output to a file.

Parameters:
-docurl: The url for the DocumentAPI (if the DocumentAPI is to be tested)
-backendurl The url for the AppBackendAPI (if the AppBackendAPI is to be tested)
-user: The username to use to login
-pwd: The passworkd to use to login
-workflow: The workflow the utility should run against.
-batchsize: (default 50) The number of documents that should be in processs at the same time. For example, if set to 50 and the utility is drawing on a pool of 100 documents, 50 will be submitted immediately. Once the first document from these files finishes processing, another file will be grabbed in its place. This continues until all of the 100 source documents has been submitted. At that point the remaining documents will work through the tests until all documents are complete. At that point, the utility will start all over again by submitting 50 of the 100 files (presuming mintime hasn't elapsed)
-pollint: (default 3000) The number of milliseconds to wait before retrying an API call the failed unexpectedly.
-maxretries: (default 10) The number of times to return an API call that unexpectedly failed.
-mintime: (default 15 min) The minimum span of time the test should run: See for param formatting: https://docs.microsoft.com/en-us/dotnet/api/system.timespan.parse?view=netframework-4.8
-reprocessrandom: For the Document API, true if running tests on a document documents should randomly be run thru the tests again (this will result in the API sometimes trying to test against the same document on multiple threads at once)
-naiveAttempts: For the Document API, the number of times the API will immediately try to get result data without first checking the file status. After this number of attempts, the API will start using the pollint/maxretries to periodically check for file completion before getting the result data.
-processText: For Document API, whether to test submitting text as well. If true, as the utility works through the provided batch of input files it will save a txt file alongside each where the txt file contains the OCR output for each file. These text files will then serve as the text to submit for text file testing.