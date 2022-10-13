namespace CalculatePostVerificationStats.Test

open NUnit.Framework
open System
open System.IO
open System.IO.Compression

open Extract.Utilities
open Extract.Testing.Utilities
open UCLID_AFUTILSLib

open CalculatePostVerificationStats
open FileSetComparer

// Automated Tests for the CompareRedactions module
[<Category("CalculatePostVerificationStats"); Category("Automated")>]
type ``Test FileSetComparer``() =

    static let DATA_QUERY = "/*/HCData|/*/MCData|/*/LCData|/*/Manual"
    static let FLAGGED_QUERY = "/*/HCData|/*/MCData|/*/LCData|/*/Clues"

    // This data is from running rules against files from the Redaction NRS test
    // All attribute values have been replaced with random characters, with line-breaks preserved
    static let EXPECTED_DATA_ZIP = "Resources.ExpectedData.zip"
    static let FOUND_DATA_ZIP = "Resources.FoundHCDataClues.zip"

    static let testFiles = new TestFileManager<``Test FileSetComparer``>()
    static let tempImageDir = FileSystemMethods.GetTemporaryFolder()

    do
        ZipFile.ExtractToDirectory(testFiles.GetFile(EXPECTED_DATA_ZIP), tempImageDir.FullName)
        ZipFile.ExtractToDirectory(testFiles.GetFile(FOUND_DATA_ZIP), tempImageDir.FullName)

    let afutil = AFUtilityClass()

    let imageFiles =
        Directory.GetFiles(tempImageDir.FullName, "*.evoa", SearchOption.AllDirectories)
        |> Array.map (fun fileName -> Path.ChangeExtension(fileName, null))
        |> ResizeArray<_>

    let dbMock =
        { new Database.IAttributeSource with
            member _.getExpAttributes(imageFile) =
                Some(afutil.GetAttributesFromFile(imageFile + ".evoa"), DateTime.Now)

            member _.getFndAttributes(imageFile) =
                Some(afutil.GetAttributesFromFile(imageFile + ".voa"), DateTime.Now) }

    let expectedHeader =
        [ "Expecteds"
          "Correct"
          "Miss"
          "False Positive"
          "Flagged" ]

    [<OneTimeSetUp>]
    static member Setup() = GeneralMethods.TestSetup()

    [<OneTimeTearDown>]
    static member Teardown() =
        testFiles.Dispose()
        tempImageDir.Delete(true)

    // These results were confirmed to match the results of running the legacy RedactionTester via the TestHarness
    [<Test>]
    member _.``Compare set of files from mocked DB``() =
        // Arrange
        let fileComparer =
            FileSetComparer(dbMock, DATA_QUERY, FLAGGED_QUERY, Environment.ProcessorCount) :> IFileSetComparer

        // Act
        let result = fileComparer.CompareRedactions(imageFiles)
        let resultString = result.ToString()

        // Assert
        Assert.Multiple (fun () ->
            Assert.AreEqual(
                """Total Files: 499
Flagged for review: 444 (89.0%)
Expected: 1918
Missed: 597
Recall: 0.689
Precision: 0.841
Post-Verify Precision: 1.000
Post-Verify Recall: 0.978""",
                resultString
            )

            Assert.AreEqual(expectedHeader, result.rulesTable.Head)
            Assert.AreEqual([ "5"; "5"; "0"; "10"; "1" ], result.rulesTable.Tail.Head)
            Assert.AreEqual(expectedHeader, result.postVerifyTable.Head)
            Assert.AreEqual([ "5"; "5"; "0"; "0"; "1" ], result.postVerifyTable.Tail.Head))

    // These results were confirmed to match the results of running the legacy RedactionTester via the TestHarness
    [<Test>]
    member _.``Compare set of files from mocked DB in 'hybrid' mode``() =
        // Arrange
        let flaggedQuery = "/*/MCData|/*/LCData|/*/Clues"

        let fileComparer =
            FileSetComparer(dbMock, DATA_QUERY, flaggedQuery, Environment.ProcessorCount) :> IFileSetComparer

        // Act
        let result = fileComparer.CompareRedactions(imageFiles)
        let resultString = result.ToString()

        // Assert
        Assert.Multiple (fun () ->
            Assert.AreEqual(
                """Total Files: 499
Flagged for review: 414 (83.0%)
Expected: 1918
Missed: 597
Recall: 0.689
Precision: 0.841
Post-Verify Precision: 0.997
Post-Verify Recall: 0.974""",
                resultString
            )

            Assert.AreEqual(expectedHeader, result.rulesTable.Head)
            Assert.AreEqual([ "5"; "5"; "0"; "10"; "1" ], result.rulesTable.Tail.Head)
            Assert.AreEqual(expectedHeader, result.postVerifyTable.Head)
            Assert.AreEqual([ "5"; "5"; "0"; "0"; "1" ], result.postVerifyTable.Tail.Head))
