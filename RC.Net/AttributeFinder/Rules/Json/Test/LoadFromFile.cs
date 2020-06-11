using Extract.Interop;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System.IO;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules.Json.Test
{
    [TestFixture]
    [Category("JsonRuleObjects")]
    public class LoadFromFile
    {
        #region Fields

        static TestFileManager<LoadFromFile> _testFiles;

        #endregion Fields

        #region Setup and Teardown

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<LoadFromFile>();
        }

        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            if (_testFiles != null)
            {
                _testFiles.Dispose();
            }
        }

        #endregion Setup and Teardown

        #region Public Test Functions

        /// <summary>
        /// Load example binary RSD files, save to file as JSON. Reload from file. Confirm that the object is not marked Dirty
        /// </summary>
        [Test, Category("RulesJsonSerialization")]
        [TestCase("Resources.RuleSets.Indexing-AddressFinders-AddressFinder.rsd", TestName = "Indexing-AddressFinders-AddressFinder.rsd")]
        [TestCase("Resources.RuleSets.Indexing-DocumentDate-commonOH.rsd", TestName = "Indexing-DocumentDate-commonOH.rsd")]
        [TestCase("Resources.RuleSets.LabDE-PatientInfo-Name-LabSpecificRules.rsd", TestName = "LabDE-PatientInfo-Name-LabSpecificRules.rsd")]
        [TestCase("Resources.RuleSets.LabDE-PatientInfo-Name-reOCR.rsd", TestName = "LabDE-PatientInfo-Name-reOCR.rsd")]
        [TestCase("Resources.RuleSets.LabDE-SwipingRules-Date.rsd", TestName = "LabDE-SwipingRules-Date.rsd")]
        [TestCase("Resources.RuleSets.LabDE-TestResults-processMultipleDates.rsd", TestName = "LabDE-TestResults-processMultipleDates.rsd")]
        [TestCase("Resources.RuleSets.ReusableComponents-getDocAndPagesCC.rsd", TestName = "ReusableComponents-getDocAndPagesCC.rsd")]
        [TestCase("Resources.RuleSets.ReusableComponents-getDocAndPagesCChelper.rsd", TestName = "ReusableComponents-getDocAndPagesCChelper.rsd")]
        [TestCase("Resources.RuleSets.ReusableComponents-MLFeatureGen-LocationFinder-GetPageBoundaries.rsd", TestName = "ReusableComponents-MLFeatureGen-LocationFinder-GetPageBoundaries.rsd")]
        [TestCase("Resources.RuleSets.ReusableComponents-MLFeatureGen-LocationFinder-LocationFinder.rsd", TestName = "ReusableComponents-MLFeatureGen-LocationFinder-LocationFinder.rsd")]
        [TestCase("Resources.RuleSets.ReusableComponents-PS-MasterConfidence.rsd", TestName = "ReusableComponents-PS-MasterConfidence.rsd")]
        [TestCase("Resources.RuleSets.ReusableComponents-RemoveSubattributeDuplicates.rsd", TestName = "ReusableComponents-RemoveSubattributeDuplicates.rsd")]
        [TestCase("Resources.RuleSets.ReusableComponents-spansPages.rsd", TestName = "ReusableComponents-spansPages.rsd")]
        [TestCase("Resources.RuleSets.RunTest.rsd", TestName = "RunTest.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-CompressAutoPaginationVOA.rsd", TestName = "Essentia-Solution-Rules-CompressAutoPaginationVOA.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-CreateDocDescVOA.rsd", TestName = "Essentia-Solution-Rules-CreateDocDescVOA.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-DocumentDate-HCdates.rsd", TestName = "Essentia-Solution-Rules-DocumentDate-HCdates.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-FilenameFix.rsd", TestName = "Essentia-Solution-Rules-FilenameFix.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-findDocumentData.rsd", TestName = "Essentia-Solution-Rules-findDocumentData.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-Master.rsd", TestName = "Essentia-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-Master_NoVOA.rsd", TestName = "Essentia-Solution-Rules-Master_NoVOA.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ML-Deleter-deleter.rsd", TestName = "Essentia-Solution-Rules-ML-Deleter-deleter.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ML-Pagination-createProtofeatures.rsd", TestName = "Essentia-Solution-Rules-ML-Pagination-createProtofeatures.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ML-Pagination-getImagePageNumber.rsd", TestName = "Essentia-Solution-Rules-ML-Pagination-getImagePageNumber.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-PaginationMaster.rsd", TestName = "Essentia-Solution-Rules-PaginationMaster.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-PatientInfo-findDOB.rsd", TestName = "Essentia-Solution-Rules-PatientInfo-findDOB.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ShadowFax.rsd", TestName = "Essentia-Solution-Rules-ShadowFax.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-Singles-PaginateSingles.rsd", TestName = "Essentia-Solution-Rules-Singles-PaginateSingles.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-UnsolicitedOrders-PopulateOrderAndCSN.rsd", TestName = "Essentia-Solution-Rules-UnsolicitedOrders-PopulateOrderAndCSN.rsd")]
        [TestCase("Resources.RuleSets._Demo-Demo_HIM-Solution-Rules-findDocumentData.rsd", TestName = "_Demo-Demo_HIM-Solution-Rules-findDocumentData.rsd")]
        [TestCase("Resources.RuleSets._Demo-Demo_HIM-Solution-Rules-PaginationMaster.rsd", TestName = "_Demo-Demo_HIM-Solution-Rules-PaginationMaster.rsd")]
        [TestCase("Resources.RuleSets._Demo-Demo_HIM-Solution-Rules-PaginationRules-main.rsd", TestName = "_Demo-Demo_HIM-Solution-Rules-PaginationRules-main.rsd")]
        [TestCase("Resources.RuleSets.American Family-Rules-master.rsd", TestName = "American Family-Rules-master.rsd")]
        [TestCase("Resources.RuleSets.American Family-TestingFiles-testAllSingleValues.rsd", TestName = "American Family-TestingFiles-testAllSingleValues.rsd")]
        [TestCase("Resources.RuleSets.CA - Sacramento Demo-Rules-SSNRedaction-DOB.rsd", TestName = "CA - Sacramento Demo-Rules-SSNRedaction-DOB.rsd")]
        [TestCase("Resources.RuleSets.CA - Santa Clara-Solution-Rules-docTypeOH.rsd", TestName = "CA - Santa Clara-Solution-Rules-docTypeOH.rsd")]
        [TestCase("Resources.RuleSets.MN - District - Tyler-Rules-rules.rsd", TestName = "MN - District - Tyler-Rules-rules.rsd")]
        [TestCase("Resources.RuleSets.Surefire - Judgments-Solution-Rules-Master.rsd", TestName = "Surefire - Judgments-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.Surefire - SNL-Solution-Rules-Master.rsd", TestName = "Surefire - SNL-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.Surefire-Solution-Rules-Common-selectMajoritySubs.rsd", TestName = "Surefire-Solution-Rules-Common-selectMajoritySubs.rsd")]
        [TestCase("Resources.RuleSets.Surefire-Solution-Rules-Master.rsd", TestName = "Surefire-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.TX - Tarrant-DataEntryConfig-SaveVolumeAndPage.rsd", TestName = "TX - Tarrant-DataEntryConfig-SaveVolumeAndPage.rsd")]
        [TestCase("Resources.RuleSets.ZZ-Situs-Solution-Rules-Master.rsd", TestName = "ZZ-Situs-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.ZZ-Situs-Solution-Rules-Rent-data.rsd", TestName = "ZZ-Situs-Solution-Rules-Rent-data.rsd")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-CreateTrainingDataAndTryToLearn.rsd", TestName = "Arcondis-Solution-Rules-CreateTrainingDataAndTryToLearn.rsd")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-Ensemble-main.rsd", TestName = "Arcondis-Solution-Rules-Ensemble-main.rsd")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-Ensemble-SensitiveRows.rsd", TestName = "Arcondis-Solution-Rules-Ensemble-SensitiveRows.rsd")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-Master.rsd", TestName = "Arcondis-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.CA - Amador - BMI-Rules-Master.rsd", TestName = "CA - Amador - BMI-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.CA - Humboldt - BMI Imaging-Rules-Utils-CChelper.rsd", TestName = "CA - Humboldt - BMI Imaging-Rules-Utils-CChelper.rsd")]
        [TestCase("Resources.RuleSets.CA - Marin Vital Records - DFM-Rules-Master.rsd", TestName = "CA - Marin Vital Records - DFM-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.CA - Orange - SouthTech-Rules-Master.rsd", TestName = "CA - Orange - SouthTech-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.Delta Dental-Rules-Master.rsd", TestName = "Delta Dental-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.Demo_IDShield_EDI_sanitize-Rules-People-People.rsd", TestName = "Demo_IDShield_EDI_sanitize-Rules-People-People.rsd")]
        [TestCase("Resources.RuleSets.Demo_MGIC-Rules-Master.rsd", TestName = "Demo_MGIC-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.Demo_PHI-Rules-MRN-MRNMaster.rsd", TestName = "Demo_PHI-Rules-MRN-MRNMaster.rsd")]
        [TestCase("Resources.RuleSets.IL - Ogle-Rules-Master.rsd", TestName = "IL - Ogle-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.IN - District Rules - ACS-Rules-Master.rsd", TestName = "IN - District Rules - ACS-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.IN - SOS - GCR-Solution-Rules-Master.rsd", TestName = "IN - SOS - GCR-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.IN - SOS-Rules-perPage.rsd", TestName = "IN - SOS-Rules-perPage.rsd")]
        [TestCase("Resources.RuleSets.MGIC-Rules-SensitivePageClassifier-makePageClue.rsd", TestName = "MGIC-Rules-SensitivePageClassifier-makePageClue.rsd")]
        [TestCase("Resources.RuleSets.MGIC-Utils-OnlyPageClues.rsd", TestName = "MGIC-Utils-OnlyPageClues.rsd")]
        [TestCase("Resources.RuleSets.OH - BWC-Rules-Master.rsd", TestName = "OH - BWC-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.OH - Cuyahoga Probate Court - Proware-Rules-decrement.rsd", TestName = "OH - Cuyahoga Probate Court - Proware-Rules-decrement.rsd")]
        [TestCase("Resources.RuleSets.Pfizer-Rules-Master.rsd", TestName = "Pfizer-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.Pfizer-Rules-ML-currency-cellClassifier-getCandidates.rsd", TestName = "Pfizer-Rules-ML-currency-cellClassifier-getCandidates.rsd")]
        [TestCase("Resources.RuleSets.Pfizer-Rules-ML-main.rsd", TestName = "Pfizer-Rules-ML-main.rsd")]
        [TestCase("Resources.RuleSets.TX - DallasCountyElections-Rules-Master.rsd", TestName = "TX - DallasCountyElections-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.WI - Dodge - TriMin-Rules-Master.rsd", TestName = "WI - Dodge - TriMin-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.WI - Dodge - TriMin-Rules-Utils-PageCC.rsd", TestName = "WI - Dodge - TriMin-Rules-Utils-PageCC.rsd")]
        [TestCase("Resources.RuleSets.WV - SOS UCC-Rules-Master.rsd", TestName = "WV - SOS UCC-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.CA - Cedar Sinai-Solution-Rules-Master.rsd", TestName = "CA - Cedar Sinai-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.CA - Cedar Sinai-Solution-Rules-TestResults-getTests.rsd", TestName = "CA - Cedar Sinai-Solution-Rules-TestResults-getTests.rsd")]
        [TestCase("Resources.RuleSets.Demo_LabDE-Solution-Rules-DocumentSorter-Master.rsd", TestName = "Demo_LabDE-Solution-Rules-DocumentSorter-Master.rsd")]
        [TestCase("Resources.RuleSets.Hurley-Solution-Rules-SplitDeletedPages.rsd", TestName = "Hurley-Solution-Rules-SplitDeletedPages.rsd")]
        [TestCase("Resources.RuleSets.Northwestern Memorial Hospital-Solution-Rules-ShadowFax.rsd", TestName = "Northwestern Memorial Hospital-Solution-Rules-ShadowFax.rsd")]
        [TestCase("Resources.RuleSets.UW Transplant-Solution-Rules-DocumentSorter-Master.rsd", TestName = "UW Transplant-Solution-Rules-DocumentSorter-Master.rsd")]
        [TestCase("Resources.RuleSets.UW Transplant-Solution-Rules-PaginationMaster.rsd", TestName = "UW Transplant-Solution-Rules-PaginationMaster.rsd")]
        [TestCase("Resources.RuleSets.UW Transplant-Solution-Rules-Swiping-CollectionDate.rsd", TestName = "UW Transplant-Solution-Rules-Swiping-CollectionDate.rsd")]
        [TestCase("Resources.RuleSets.WI - Exact Sciences-Solution-Rules-Master.rsd", TestName = "WI - Exact Sciences-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.WI - Exact Sciences-Solution-Rules-Master_NoUSS.rsd", TestName = "WI - Exact Sciences-Solution-Rules-Master_NoUSS.rsd")]
        public static void ExampleRuleSetsDirtyFlag(string resourceName)
        {
            var rsdPath = _testFiles.GetFile(resourceName);
            var ruleset = new RuleSetClass();

            // Load binary
            ruleset.LoadFrom(rsdPath, false);
            Assert.IsFalse(ruleset.IsDirty); // Should be not dirty
            ruleset.IsSwipingRule = !ruleset.IsSwipingRule; // Change something to make dirty
            Assert.IsTrue(ruleset.IsDirty); // Is dirty!

            // Save json
            ruleset.SaveTo(rsdPath, false);
            Assert.IsTrue(ruleset.IsDirty); // Still dirty since bClearDirty = false
            ruleset.SaveTo(rsdPath, true);
            Assert.IsFalse(ruleset.IsDirty);
            ruleset.FKBVersion += ".1";
            Assert.IsTrue(ruleset.IsDirty);

            // Load JSON
            ruleset.LoadFrom(rsdPath, false);
            Assert.IsFalse(ruleset.IsDirty);
            ruleset.ForInternalUseOnly = !ruleset.ForInternalUseOnly;
            Assert.IsTrue(ruleset.IsDirty);
        }

        /// <summary>
        /// Load example binary RSD files, save to file as JSON. Reload from file. Confirm that the DTO from binary = the DTO from JSON
        /// </summary>
        [Test, Category("RulesJsonSerialization")]
        [TestCase("Resources.RuleSets.Indexing-AddressFinders-AddressFinder.rsd", TestName = "Indexing-AddressFinders-AddressFinder.rsd")]
        [TestCase("Resources.RuleSets.Indexing-DocumentDate-commonOH.rsd", TestName = "Indexing-DocumentDate-commonOH.rsd")]
        [TestCase("Resources.RuleSets.LabDE-PatientInfo-Name-LabSpecificRules.rsd", TestName = "LabDE-PatientInfo-Name-LabSpecificRules.rsd")]
        [TestCase("Resources.RuleSets.LabDE-PatientInfo-Name-reOCR.rsd", TestName = "LabDE-PatientInfo-Name-reOCR.rsd")]
        [TestCase("Resources.RuleSets.LabDE-SwipingRules-Date.rsd", TestName = "LabDE-SwipingRules-Date.rsd")]
        [TestCase("Resources.RuleSets.LabDE-TestResults-processMultipleDates.rsd", TestName = "LabDE-TestResults-processMultipleDates.rsd")]
        [TestCase("Resources.RuleSets.ReusableComponents-getDocAndPagesCC.rsd", TestName = "ReusableComponents-getDocAndPagesCC.rsd")]
        [TestCase("Resources.RuleSets.ReusableComponents-getDocAndPagesCChelper.rsd", TestName = "ReusableComponents-getDocAndPagesCChelper.rsd")]
        [TestCase("Resources.RuleSets.ReusableComponents-MLFeatureGen-LocationFinder-GetPageBoundaries.rsd", TestName = "ReusableComponents-MLFeatureGen-LocationFinder-GetPageBoundaries.rsd")]
        [TestCase("Resources.RuleSets.ReusableComponents-MLFeatureGen-LocationFinder-LocationFinder.rsd", TestName = "ReusableComponents-MLFeatureGen-LocationFinder-LocationFinder.rsd")]
        [TestCase("Resources.RuleSets.ReusableComponents-PS-MasterConfidence.rsd", TestName = "ReusableComponents-PS-MasterConfidence.rsd")]
        [TestCase("Resources.RuleSets.ReusableComponents-RemoveSubattributeDuplicates.rsd", TestName = "ReusableComponents-RemoveSubattributeDuplicates.rsd")]
        [TestCase("Resources.RuleSets.ReusableComponents-spansPages.rsd", TestName = "ReusableComponents-spansPages.rsd")]
        [TestCase("Resources.RuleSets.RunTest.rsd", TestName = "RunTest.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-CompressAutoPaginationVOA.rsd", TestName = "Essentia-Solution-Rules-CompressAutoPaginationVOA.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-CreateDocDescVOA.rsd", TestName = "Essentia-Solution-Rules-CreateDocDescVOA.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-DocumentDate-HCdates.rsd", TestName = "Essentia-Solution-Rules-DocumentDate-HCdates.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-FilenameFix.rsd", TestName = "Essentia-Solution-Rules-FilenameFix.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-findDocumentData.rsd", TestName = "Essentia-Solution-Rules-findDocumentData.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-Master.rsd", TestName = "Essentia-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-Master_NoVOA.rsd", TestName = "Essentia-Solution-Rules-Master_NoVOA.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ML-Deleter-deleter.rsd", TestName = "Essentia-Solution-Rules-ML-Deleter-deleter.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ML-Pagination-createProtofeatures.rsd", TestName = "Essentia-Solution-Rules-ML-Pagination-createProtofeatures.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ML-Pagination-getImagePageNumber.rsd", TestName = "Essentia-Solution-Rules-ML-Pagination-getImagePageNumber.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-PaginationMaster.rsd", TestName = "Essentia-Solution-Rules-PaginationMaster.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-PatientInfo-findDOB.rsd", TestName = "Essentia-Solution-Rules-PatientInfo-findDOB.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ShadowFax.rsd", TestName = "Essentia-Solution-Rules-ShadowFax.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-Singles-PaginateSingles.rsd", TestName = "Essentia-Solution-Rules-Singles-PaginateSingles.rsd")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-UnsolicitedOrders-PopulateOrderAndCSN.rsd", TestName = "Essentia-Solution-Rules-UnsolicitedOrders-PopulateOrderAndCSN.rsd")]
        [TestCase("Resources.RuleSets._Demo-Demo_HIM-Solution-Rules-findDocumentData.rsd", TestName = "_Demo-Demo_HIM-Solution-Rules-findDocumentData.rsd")]
        [TestCase("Resources.RuleSets._Demo-Demo_HIM-Solution-Rules-PaginationMaster.rsd", TestName = "_Demo-Demo_HIM-Solution-Rules-PaginationMaster.rsd")]
        [TestCase("Resources.RuleSets._Demo-Demo_HIM-Solution-Rules-PaginationRules-main.rsd", TestName = "_Demo-Demo_HIM-Solution-Rules-PaginationRules-main.rsd")]
        [TestCase("Resources.RuleSets.American Family-Rules-master.rsd", TestName = "American Family-Rules-master.rsd")]
        [TestCase("Resources.RuleSets.American Family-TestingFiles-testAllSingleValues.rsd", TestName = "American Family-TestingFiles-testAllSingleValues.rsd")]
        [TestCase("Resources.RuleSets.CA - Sacramento Demo-Rules-SSNRedaction-DOB.rsd", TestName = "CA - Sacramento Demo-Rules-SSNRedaction-DOB.rsd")]
        [TestCase("Resources.RuleSets.CA - Santa Clara-Solution-Rules-docTypeOH.rsd", TestName = "CA - Santa Clara-Solution-Rules-docTypeOH.rsd")]
        [TestCase("Resources.RuleSets.MN - District - Tyler-Rules-rules.rsd", TestName = "MN - District - Tyler-Rules-rules.rsd")]
        [TestCase("Resources.RuleSets.Surefire - Judgments-Solution-Rules-Master.rsd", TestName = "Surefire - Judgments-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.Surefire - SNL-Solution-Rules-Master.rsd", TestName = "Surefire - SNL-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.Surefire-Solution-Rules-Common-selectMajoritySubs.rsd", TestName = "Surefire-Solution-Rules-Common-selectMajoritySubs.rsd")]
        [TestCase("Resources.RuleSets.Surefire-Solution-Rules-Master.rsd", TestName = "Surefire-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.TX - Tarrant-DataEntryConfig-SaveVolumeAndPage.rsd", TestName = "TX - Tarrant-DataEntryConfig-SaveVolumeAndPage.rsd")]
        [TestCase("Resources.RuleSets.ZZ-Situs-Solution-Rules-Master.rsd", TestName = "ZZ-Situs-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.ZZ-Situs-Solution-Rules-Rent-data.rsd", TestName = "ZZ-Situs-Solution-Rules-Rent-data.rsd")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-CreateTrainingDataAndTryToLearn.rsd", TestName = "Arcondis-Solution-Rules-CreateTrainingDataAndTryToLearn.rsd")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-Ensemble-main.rsd", TestName = "Arcondis-Solution-Rules-Ensemble-main.rsd")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-Ensemble-SensitiveRows.rsd", TestName = "Arcondis-Solution-Rules-Ensemble-SensitiveRows.rsd")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-Master.rsd", TestName = "Arcondis-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.CA - Amador - BMI-Rules-Master.rsd", TestName = "CA - Amador - BMI-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.CA - Humboldt - BMI Imaging-Rules-Utils-CChelper.rsd", TestName = "CA - Humboldt - BMI Imaging-Rules-Utils-CChelper.rsd")]
        [TestCase("Resources.RuleSets.CA - Marin Vital Records - DFM-Rules-Master.rsd", TestName = "CA - Marin Vital Records - DFM-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.CA - Orange - SouthTech-Rules-Master.rsd", TestName = "CA - Orange - SouthTech-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.Delta Dental-Rules-Master.rsd", TestName = "Delta Dental-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.Demo_IDShield_EDI_sanitize-Rules-People-People.rsd", TestName = "Demo_IDShield_EDI_sanitize-Rules-People-People.rsd")]
        [TestCase("Resources.RuleSets.Demo_MGIC-Rules-Master.rsd", TestName = "Demo_MGIC-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.Demo_PHI-Rules-MRN-MRNMaster.rsd", TestName = "Demo_PHI-Rules-MRN-MRNMaster.rsd")]
        [TestCase("Resources.RuleSets.IL - Ogle-Rules-Master.rsd", TestName = "IL - Ogle-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.IN - District Rules - ACS-Rules-Master.rsd", TestName = "IN - District Rules - ACS-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.IN - SOS - GCR-Solution-Rules-Master.rsd", TestName = "IN - SOS - GCR-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.IN - SOS-Rules-perPage.rsd", TestName = "IN - SOS-Rules-perPage.rsd")]
        [TestCase("Resources.RuleSets.MGIC-Rules-SensitivePageClassifier-makePageClue.rsd", TestName = "MGIC-Rules-SensitivePageClassifier-makePageClue.rsd")]
        [TestCase("Resources.RuleSets.MGIC-Utils-OnlyPageClues.rsd", TestName = "MGIC-Utils-OnlyPageClues.rsd")]
        [TestCase("Resources.RuleSets.OH - BWC-Rules-Master.rsd", TestName = "OH - BWC-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.OH - Cuyahoga Probate Court - Proware-Rules-decrement.rsd", TestName = "OH - Cuyahoga Probate Court - Proware-Rules-decrement.rsd")]
        [TestCase("Resources.RuleSets.Pfizer-Rules-Master.rsd", TestName = "Pfizer-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.Pfizer-Rules-ML-currency-cellClassifier-getCandidates.rsd", TestName = "Pfizer-Rules-ML-currency-cellClassifier-getCandidates.rsd")]
        [TestCase("Resources.RuleSets.Pfizer-Rules-ML-main.rsd", TestName = "Pfizer-Rules-ML-main.rsd")]
        [TestCase("Resources.RuleSets.TX - DallasCountyElections-Rules-Master.rsd", TestName = "TX - DallasCountyElections-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.WI - Dodge - TriMin-Rules-Master.rsd", TestName = "WI - Dodge - TriMin-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.WI - Dodge - TriMin-Rules-Utils-PageCC.rsd", TestName = "WI - Dodge - TriMin-Rules-Utils-PageCC.rsd")]
        [TestCase("Resources.RuleSets.WV - SOS UCC-Rules-Master.rsd", TestName = "WV - SOS UCC-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.CA - Cedar Sinai-Solution-Rules-Master.rsd", TestName = "CA - Cedar Sinai-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.CA - Cedar Sinai-Solution-Rules-TestResults-getTests.rsd", TestName = "CA - Cedar Sinai-Solution-Rules-TestResults-getTests.rsd")]
        [TestCase("Resources.RuleSets.Demo_LabDE-Solution-Rules-DocumentSorter-Master.rsd", TestName = "Demo_LabDE-Solution-Rules-DocumentSorter-Master.rsd")]
        [TestCase("Resources.RuleSets.Hurley-Solution-Rules-SplitDeletedPages.rsd", TestName = "Hurley-Solution-Rules-SplitDeletedPages.rsd")]
        [TestCase("Resources.RuleSets.Northwestern Memorial Hospital-Solution-Rules-ShadowFax.rsd", TestName = "Northwestern Memorial Hospital-Solution-Rules-ShadowFax.rsd")]
        [TestCase("Resources.RuleSets.UW Transplant-Solution-Rules-DocumentSorter-Master.rsd", TestName = "UW Transplant-Solution-Rules-DocumentSorter-Master.rsd")]
        [TestCase("Resources.RuleSets.UW Transplant-Solution-Rules-PaginationMaster.rsd", TestName = "UW Transplant-Solution-Rules-PaginationMaster.rsd")]
        [TestCase("Resources.RuleSets.UW Transplant-Solution-Rules-Swiping-CollectionDate.rsd", TestName = "UW Transplant-Solution-Rules-Swiping-CollectionDate.rsd")]
        [TestCase("Resources.RuleSets.WI - Exact Sciences-Solution-Rules-Master.rsd", TestName = "WI - Exact Sciences-Solution-Rules-Master.rsd")]
        [TestCase("Resources.RuleSets.WI - Exact Sciences-Solution-Rules-Master_NoUSS.rsd", TestName = "WI - Exact Sciences-Solution-Rules-Master_NoUSS.rsd")]
        public static void ExampleRuleSetsRoundTrip(string resourceName)
        {
            var rsdPath = _testFiles.GetFile(resourceName);
            var ruleset = new RuleSetClass();

            // Load binary
            ruleset.LoadFrom(rsdPath, false);
            var dtoFromBinary = (Dto.RuleSet)Domain.RuleObjectConverter.ConvertToDto(ruleset);

            // Save/load json
            ruleset.SaveTo(rsdPath, false);
            ruleset.LoadFrom(rsdPath, false);

            var dtoFromJson = (Dto.RuleSet)Domain.RuleObjectConverter.ConvertToDto(ruleset);
            Assert.AreEqual(dtoFromBinary, dtoFromJson);
        }

        #endregion
    }
}
