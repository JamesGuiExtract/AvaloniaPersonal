namespace CalculatePostVerificationStats.Test

open NUnit.Framework
open Swensen.Unquote
open System

open Extract.FileActionManager.Database.Test
open Extract.Testing.Utilities
open UCLID_COMUTILSLib

open CalculatePostVerificationStats
open Database

// Automated Tests for the CompareRedactions module
[<Category("CalculatePostVerificationStats"); Category("Automated")>]
type ``Test Database``() =

    static let DATABASE_WITH_SANITIZED_DATA = "Resources.RedactionNRS.bak"

    static let testFiles = new TestFileManager<``Test Database``>()
    static let testDatabases = new FAMTestDBManager<``Test Database``>()
    static let redactionDB = ref null
    static let databaseName = testDatabases.GenerateDatabaseName()

    static let databaseInfo =
        { Server = "(local)"
          Name = databaseName
          VerifyActions = [ "Verify" ]
          ExpAttributeSet = "exp"
          FndAttributeSet = "fnd" }

    [<OneTimeSetUp>]
    static member Setup() =
        GeneralMethods.TestSetup()

        redactionDB.Value <- testDatabases.GetDatabase(DATABASE_WITH_SANITIZED_DATA, databaseName)

    [<OneTimeTearDown>]
    static member Teardown() =
        testFiles.Dispose()
        testDatabases.Dispose()

    [<Test>]
    member _.``Get files complete for the verify action``() =
        // Arrange
        let documentSource = Database(databaseInfo) :> IDocumentSource

        // Act
        let verifiedFiles = documentSource.getFilesCompleteForActions ()

        // Assert
        test <@ verifiedFiles.Count = 250 @>
        let first4Files = verifiedFiles |> Seq.truncate 4 |> Seq.toList

        test
            <@ first4Files = [ """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\Clues\Images\RER_Clues_001.tif"""
                               """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\Clues\Images\RER_Clues_002.tif"""
                               """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\Clues\Images\RER_Clues_004.tif"""
                               """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\Clues\Images\RER_Clues_006.tif""" ] @>

    [<Test>]
    member _.``Get first 100 files complete for the verify action``() =
        // Arrange
        let limit = 100
        let documentSource = Database(databaseInfo) :> IDocumentSource

        // Act
        let verifiedFiles = documentSource.getFirstFilesCompleteForActions limit

        // Assert
        test <@ verifiedFiles.Count = limit @>
        let first4Files = verifiedFiles |> Seq.truncate 4 |> Seq.toList

        test
            <@ first4Files = [ """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\HCData\Images\RER_HCData_001.tif"""
                               """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\Clues\Images\RER_Clues_004.tif"""
                               """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\HCData\Images\RER_HCData_004.tif"""
                               """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\HCData\Images\RER_HCData_003.tif""" ] @>

    [<Test>]
    member _.``Get files complete for multiple actions``() =
        // Arrange
        let documentSource =
            Database({ databaseInfo with VerifyActions = [ "Verify"; "b" ] }) :> IDocumentSource

        // Act
        let verifiedFiles = documentSource.getFilesCompleteForActions ()

        // Assert
        test <@ verifiedFiles.Count = 499 @>
        let first4Files = verifiedFiles |> Seq.truncate 4 |> Seq.toList

        test
            <@ first4Files = [ """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\Clues\Images\RER_Clues_001.tif"""
                               """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\Clues\Images\RER_Clues_002.tif"""
                               """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\Clues\Images\RER_Clues_003.tif"""
                               """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\Clues\Images\RER_Clues_004.tif""" ] @>

    [<Test>]
    member _.``Get first 100 files complete for multiple actions``() =
        // Arrange
        let limit = 100

        let documentSource =
            Database({ databaseInfo with VerifyActions = [ "Verify"; "b" ] }) :> IDocumentSource

        // Act
        let verifiedFiles = documentSource.getFirstFilesCompleteForActions limit

        // Assert
        test <@ verifiedFiles.Count = limit @>
        let first4Files = verifiedFiles |> Seq.truncate 4 |> Seq.toList

        test
            <@ first4Files = [ """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\Clues\Images\RER_Clues_007.tif"""
                               """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\Clues\Images\RER_Clues_005.tif"""
                               """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\Clues\Images\RER_Clues_008.tif"""
                               """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\Clues\Images\RER_Clues_004.tif""" ] @>

    [<Test>]
    member _.``Confirm that the latest version of a VOA is retrieved``() =
        // Arrange
        // The found VOA was empty in the original set of data but I stored the expected MCData items afterwords
        let updatedFile =
            """C:\Rules\NRSTest\Redaction\AutomatedTest\BankAccount\MCData\Images\RER_MCData_006.tif"""

        let attributeSource = Database(databaseInfo) :> IAttributeSource

        // Act
        let voa, _ =
            attributeSource.getFndAttributes (updatedFile)
            |> Option.defaultValue (IUnknownVectorClass(), DateTime.Now)

        // Assert
        test <@ voa.Size() = 2 @>
