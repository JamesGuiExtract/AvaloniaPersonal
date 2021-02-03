using Extract.Interop;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules.Json.Test
{
    [TestFixture]
    [Category("JsonRuleObjects")]
    public class BinaryToBinary
    {
        #region Fields

        static TestFileManager<BinaryToBinary> _testFiles;

        #endregion Fields

        #region Setup and Teardown

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<BinaryToBinary>();
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
        /// Load example binary RSD files, save as JSON and reload. Compare the binary streams.
        /// </summary>
        [Test, Category("RulesJsonSerialization")]
        [TestCase("Resources.RuleSets.Indexing-AddressFinders-AddressFinder.rsd", TestName = "RSD_File_Indexing-AddressFinders-AddressFinder")]
        [TestCase("Resources.RuleSets.Indexing-DocumentDate-commonOH.rsd", TestName = "RSD_File_Indexing-DocumentDate-commonOH")]
        [TestCase("Resources.RuleSets.LabDE-PatientInfo-Name-LabSpecificRules.rsd", TestName = "RSD_File_LabDE-PatientInfo-Name-LabSpecificRules")]
        [TestCase("Resources.RuleSets.LabDE-PatientInfo-Name-reOCR.rsd", TestName = "RSD_File_LabDE-PatientInfo-Name-reOCR")]
        [TestCase("Resources.RuleSets.LabDE-SwipingRules-Date.rsd", TestName = "RSD_File_LabDE-SwipingRules-Date")]
        [TestCase("Resources.RuleSets.LabDE-TestResults-processMultipleDates.rsd", TestName = "RSD_File_LabDE-TestResults-processMultipleDates")]
        [TestCase("Resources.RuleSets.ReusableComponents-getDocAndPagesCC.rsd", TestName = "RSD_File_ReusableComponents-getDocAndPagesCC")]
        [TestCase("Resources.RuleSets.ReusableComponents-getDocAndPagesCChelper.rsd", TestName = "RSD_File_ReusableComponents-getDocAndPagesCChelper")]
        [TestCase("Resources.RuleSets.ReusableComponents-MLFeatureGen-LocationFinder-GetPageBoundaries.rsd", TestName = "RSD_File_ReusableComponents-MLFeatureGen-LocationFinder-GetPageBoundaries")]
        [TestCase("Resources.RuleSets.ReusableComponents-MLFeatureGen-LocationFinder-LocationFinder.rsd", TestName = "RSD_File_ReusableComponents-MLFeatureGen-LocationFinder-LocationFinder")]
        [TestCase("Resources.RuleSets.ReusableComponents-PS-MasterConfidence.rsd", TestName = "RSD_File_ReusableComponents-PS-MasterConfidence")]
        [TestCase("Resources.RuleSets.ReusableComponents-RemoveSubattributeDuplicates.rsd", TestName = "RSD_File_ReusableComponents-RemoveSubattributeDuplicates")]
        [TestCase("Resources.RuleSets.ReusableComponents-spansPages.rsd", TestName = "RSD_File_ReusableComponents-spansPages")]
        [TestCase("Resources.RuleSets.RunTest.rsd", TestName = "RSD_File_RunTest")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-CompressAutoPaginationVOA.rsd", TestName = "RSD_File_Essentia-Solution-Rules-CompressAutoPaginationVOA")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-CreateDocDescVOA.rsd", TestName = "RSD_File_Essentia-Solution-Rules-CreateDocDescVOA")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-DocumentDate-HCdates.rsd", TestName = "RSD_File_Essentia-Solution-Rules-DocumentDate-HCdates")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-FilenameFix.rsd", TestName = "RSD_File_Essentia-Solution-Rules-FilenameFix")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-findDocumentData.rsd", TestName = "RSD_File_Essentia-Solution-Rules-findDocumentData")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-Master.rsd", TestName = "RSD_File_Essentia-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-Master_NoVOA.rsd", TestName = "RSD_File_Essentia-Solution-Rules-Master_NoVOA")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ML-Deleter-deleter.rsd", TestName = "RSD_File_Essentia-Solution-Rules-ML-Deleter-deleter")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ML-Pagination-createProtofeatures.rsd", TestName = "RSD_File_Essentia-Solution-Rules-ML-Pagination-createProtofeatures")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ML-Pagination-getImagePageNumber.rsd", TestName = "RSD_File_Essentia-Solution-Rules-ML-Pagination-getImagePageNumber")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-PaginationMaster.rsd", TestName = "RSD_File_Essentia-Solution-Rules-PaginationMaster")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-PatientInfo-findDOB.rsd", TestName = "RSD_File_Essentia-Solution-Rules-PatientInfo-findDOB")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ShadowFax.rsd", TestName = "RSD_File_Essentia-Solution-Rules-ShadowFax")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-Singles-PaginateSingles.rsd", TestName = "RSD_File_Essentia-Solution-Rules-Singles-PaginateSingles")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-UnsolicitedOrders-PopulateOrderAndCSN.rsd", TestName = "RSD_File_Essentia-Solution-Rules-UnsolicitedOrders-PopulateOrderAndCSN")]
        [TestCase("Resources.RuleSets._Demo-Demo_HIM-Solution-Rules-findDocumentData.rsd", TestName = "RSD_File__Demo-Demo_HIM-Solution-Rules-findDocumentData")]
        [TestCase("Resources.RuleSets._Demo-Demo_HIM-Solution-Rules-PaginationMaster.rsd", TestName = "RSD_File__Demo-Demo_HIM-Solution-Rules-PaginationMaster")]
        [TestCase("Resources.RuleSets._Demo-Demo_HIM-Solution-Rules-PaginationRules-main.rsd", TestName = "RSD_File__Demo-Demo_HIM-Solution-Rules-PaginationRules-main")]
        [TestCase("Resources.RuleSets.American Family-Rules-master.rsd", TestName = "RSD_File_American Family-Rules-master")]
        [TestCase("Resources.RuleSets.American Family-TestingFiles-testAllSingleValues.rsd", TestName = "RSD_File_American Family-TestingFiles-testAllSingleValues")]
        [TestCase("Resources.RuleSets.CA - Sacramento Demo-Rules-SSNRedaction-DOB.rsd", TestName = "RSD_File_CA - Sacramento Demo-Rules-SSNRedaction-DOB")]
        [TestCase("Resources.RuleSets.CA - Santa Clara-Solution-Rules-docTypeOH.rsd", TestName = "RSD_File_CA - Santa Clara-Solution-Rules-docTypeOH")]
        [TestCase("Resources.RuleSets.MN - District - Tyler-Rules-rules.rsd", TestName = "RSD_File_MN - District - Tyler-Rules-rules")]
        [TestCase("Resources.RuleSets.Surefire - Judgments-Solution-Rules-Master.rsd", TestName = "RSD_File_Surefire - Judgments-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.Surefire - SNL-Solution-Rules-Master.rsd", TestName = "RSD_File_Surefire - SNL-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.Surefire-Solution-Rules-Common-selectMajoritySubs.rsd", TestName = "RSD_File_Surefire-Solution-Rules-Common-selectMajoritySubs")]
        [TestCase("Resources.RuleSets.Surefire-Solution-Rules-Master.rsd", TestName = "RSD_File_Surefire-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.TX - Tarrant-DataEntryConfig-SaveVolumeAndPage.rsd", TestName = "RSD_File_TX - Tarrant-DataEntryConfig-SaveVolumeAndPage")]
        [TestCase("Resources.RuleSets.ZZ-Situs-Solution-Rules-Master.rsd", TestName = "RSD_File_ZZ-Situs-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.ZZ-Situs-Solution-Rules-Rent-data.rsd", TestName = "RSD_File_ZZ-Situs-Solution-Rules-Rent-data")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-CreateTrainingDataAndTryToLearn.rsd", TestName = "RSD_File_Arcondis-Solution-Rules-CreateTrainingDataAndTryToLearn")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-Ensemble-main.rsd", TestName = "RSD_File_Arcondis-Solution-Rules-Ensemble-main")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-Ensemble-SensitiveRows.rsd", TestName = "RSD_File_Arcondis-Solution-Rules-Ensemble-SensitiveRows")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-Master.rsd", TestName = "RSD_File_Arcondis-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.CA - Amador - BMI-Rules-Master.rsd", TestName = "RSD_File_CA - Amador - BMI-Rules-Master")]
        [TestCase("Resources.RuleSets.CA - Humboldt - BMI Imaging-Rules-Utils-CChelper.rsd", TestName = "RSD_File_CA - Humboldt - BMI Imaging-Rules-Utils-CChelper")]
        [TestCase("Resources.RuleSets.CA - Marin Vital Records - DFM-Rules-Master.rsd", TestName = "RSD_File_CA - Marin Vital Records - DFM-Rules-Master")]
        [TestCase("Resources.RuleSets.CA - Orange - SouthTech-Rules-Master.rsd", TestName = "RSD_File_CA - Orange - SouthTech-Rules-Master")]
        [TestCase("Resources.RuleSets.Delta Dental-Rules-Master.rsd", TestName = "RSD_File_Delta Dental-Rules-Master")]
        [TestCase("Resources.RuleSets.Demo_IDShield_EDI_sanitize-Rules-People-People.rsd", TestName = "RSD_File_Demo_IDShield_EDI_sanitize-Rules-People-People")]
        [TestCase("Resources.RuleSets.Demo_MGIC-Rules-Master.rsd", TestName = "RSD_File_Demo_MGIC-Rules-Master")]
        [TestCase("Resources.RuleSets.Demo_PHI-Rules-MRN-MRNMaster.rsd", TestName = "RSD_File_Demo_PHI-Rules-MRN-MRNMaster")]
        [TestCase("Resources.RuleSets.IL - Ogle-Rules-Master.rsd", TestName = "RSD_File_IL - Ogle-Rules-Master")]
        [TestCase("Resources.RuleSets.IN - District Rules - ACS-Rules-Master.rsd", TestName = "RSD_File_IN - District Rules - ACS-Rules-Master")]
        [TestCase("Resources.RuleSets.IN - SOS - GCR-Solution-Rules-Master.rsd", TestName = "RSD_File_IN - SOS - GCR-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.IN - SOS-Rules-perPage.rsd", TestName = "RSD_File_IN - SOS-Rules-perPage")]
        [TestCase("Resources.RuleSets.MGIC-Rules-SensitivePageClassifier-makePageClue.rsd", TestName = "RSD_File_MGIC-Rules-SensitivePageClassifier-makePageClue")]
        [TestCase("Resources.RuleSets.MGIC-Utils-OnlyPageClues.rsd", TestName = "RSD_File_MGIC-Utils-OnlyPageClues")]
        [TestCase("Resources.RuleSets.OH - BWC-Rules-Master.rsd", TestName = "RSD_File_OH - BWC-Rules-Master")]
        [TestCase("Resources.RuleSets.OH - Cuyahoga Probate Court - Proware-Rules-decrement.rsd", TestName = "RSD_File_OH - Cuyahoga Probate Court - Proware-Rules-decrement")]
        [TestCase("Resources.RuleSets.Pfizer-Rules-Master.rsd", TestName = "RSD_File_Pfizer-Rules-Master")]
        [TestCase("Resources.RuleSets.Pfizer-Rules-ML-currency-cellClassifier-getCandidates.rsd", TestName = "RSD_File_Pfizer-Rules-ML-currency-cellClassifier-getCandidates")]
        [TestCase("Resources.RuleSets.Pfizer-Rules-ML-main.rsd", TestName = "RSD_File_Pfizer-Rules-ML-main")]
        [TestCase("Resources.RuleSets.TX - DallasCountyElections-Rules-Master.rsd", TestName = "RSD_File_TX - DallasCountyElections-Rules-Master")]
        [TestCase("Resources.RuleSets.WI - Dodge - TriMin-Rules-Master.rsd", TestName = "RSD_File_WI - Dodge - TriMin-Rules-Master")]
        [TestCase("Resources.RuleSets.WI - Dodge - TriMin-Rules-Utils-PageCC.rsd", TestName = "RSD_File_WI - Dodge - TriMin-Rules-Utils-PageCC")]
        [TestCase("Resources.RuleSets.WV - SOS UCC-Rules-Master.rsd", TestName = "RSD_File_WV - SOS UCC-Rules-Master")]
        [TestCase("Resources.RuleSets.CA - Cedar Sinai-Solution-Rules-Master.rsd", TestName = "RSD_File_CA - Cedar Sinai-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.CA - Cedar Sinai-Solution-Rules-TestResults-getTests.rsd", TestName = "RSD_File_CA - Cedar Sinai-Solution-Rules-TestResults-getTests")]
        [TestCase("Resources.RuleSets.Demo_LabDE-Solution-Rules-DocumentSorter-Master.rsd", TestName = "RSD_File_Demo_LabDE-Solution-Rules-DocumentSorter-Master")]
        [TestCase("Resources.RuleSets.Hurley-Solution-Rules-SplitDeletedPages.rsd", TestName = "RSD_File_Hurley-Solution-Rules-SplitDeletedPages")]
        [TestCase("Resources.RuleSets.Northwestern Memorial Hospital-Solution-Rules-ShadowFax.rsd", TestName = "RSD_File_Northwestern Memorial Hospital-Solution-Rules-ShadowFax")]
        [TestCase("Resources.RuleSets.UW Transplant-Solution-Rules-DocumentSorter-Master.rsd", TestName = "RSD_File_UW Transplant-Solution-Rules-DocumentSorter-Master")]
        [TestCase("Resources.RuleSets.UW Transplant-Solution-Rules-PaginationMaster.rsd", TestName = "RSD_File_UW Transplant-Solution-Rules-PaginationMaster")]
        [TestCase("Resources.RuleSets.UW Transplant-Solution-Rules-Swiping-CollectionDate.rsd", TestName = "RSD_File_UW Transplant-Solution-Rules-Swiping-CollectionDate")]
        [TestCase("Resources.RuleSets.WI - Exact Sciences-Solution-Rules-Master.rsd", TestName = "RSD_File_WI - Exact Sciences-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.WI - Exact Sciences-Solution-Rules-Master_NoUSS.rsd", TestName = "RSD_File_WI - Exact Sciences-Solution-Rules-Master_NoUSS")]
        [TestCase("Resources.RuleSets.empty.rsd", TestName = "RSD_File_empty")]
        [TestCase("Resources.RuleSets.EntityFinder-ScoreEntity.rsd", TestName = "EntityFinder-ScoreEntity")]
        public static void ExampleRuleSets(string resourceName)
        {
            var rsdPath = _testFiles.GetFile(resourceName);
            var rulesetFromFile = new RuleSetClass();
            rulesetFromFile.LoadFrom(rsdPath, false);
            rulesetFromFile.FileName = "";

            var sourceBytes = GetRuleObjectBytes(rulesetFromFile);

            var json = RuleObjectJsonSerializer.Serialize(rulesetFromFile);
            var rulesetFromJson = RuleObjectJsonSerializer.Deserialize<RuleSetClass>(json);

            var roundTripBytes = GetRuleObjectBytes(rulesetFromJson);

            RemoveGuids(ref sourceBytes, ref roundTripBytes);

            CollectionAssert.AreEqual(sourceBytes, roundTripBytes);
        }


        /// <summary>
        /// Up to 100 distinct examples of each rule object taken from the customer rules git repo.
        /// Sequence objects (select multiple preprocessor/output handler) were limited to a single example.
        /// Some known objects were not encountered. These are either truly not present in customer rules or were in old
        /// rulesets that couldn't be used because of deprecated objects or invalid configurations:
        ///   - CheckFinder
        ///   - DoubleInputValidator
        ///   - FloatInputValidator
        ///   - StringTokenizerModifier
        /// TODO: Add missing tests as json-to-json tests
        /// </summary>
        [Test, Category("RulesJsonSerialization")]
        [TestCase("Resources.RuleObjects.AdvancedReplaceString.hex.txt", TestName = "AdvancedReplaceString")]
        [TestCase("Resources.RuleObjects.AutoShrinkRedactionZones.hex.txt", TestName = "AutoShrinkRedactionZones")]
        [TestCase("Resources.RuleObjects.BarcodeFinder.hex.txt", TestName = "BarcodeFinder")]
        [TestCase("Resources.RuleObjects.BlockFinder.hex.txt", TestName = "BlockFinder")]
        [TestCase("Resources.RuleObjects.BoxFinder.hex.txt", TestName = "BoxFinder")]
        [TestCase("Resources.RuleObjects.ChangeCase.hex.txt", TestName = "ChangeCase")]
        [TestCase("Resources.RuleObjects.CharacterConfidenceCondition.hex.txt", TestName = "CharacterConfidenceCondition")]
        [TestCase("Resources.RuleObjects.CharacterConfidenceDS.hex.txt", TestName = "CharacterConfidenceDS")]
        [TestCase("Resources.RuleObjects.ConditionalAttributeModifier.hex.txt", TestName = "ConditionalAttributeModifier")]
        [TestCase("Resources.RuleObjects.ConditionalOutputHandler.hex.txt", TestName = "ConditionalOutputHandler")]
        [TestCase("Resources.RuleObjects.ConditionalPreprocessor.hex.txt", TestName = "ConditionalPreprocessor")]
        [TestCase("Resources.RuleObjects.ConditionalValueFinder.hex.txt", TestName = "ConditionalValueFinder")]
        [TestCase("Resources.RuleObjects.CreateAttribute.hex.txt", TestName = "CreateAttribute")]
        [TestCase("Resources.RuleObjects.CreateValue.hex.txt", TestName = "CreateValue")]
        [TestCase("Resources.RuleObjects.DataEntryPreloader.hex.txt", TestName = "DataEntryPreloader")]
        [TestCase("Resources.RuleObjects.DataQueryRuleObject.hex.txt", TestName = "DataQueryRuleObject")]
        [TestCase("Resources.RuleObjects.DataScorerBasedAS.hex.txt", TestName = "DataScorerBasedAS")]
        [TestCase("Resources.RuleObjects.DateInputValidator.hex.txt", TestName = "DateInputValidator")]
        [TestCase("Resources.RuleObjects.DateTimeSplitter.hex.txt", TestName = "DateTimeSplitter")]
        [TestCase("Resources.RuleObjects.DocPreprocessorSequence.hex.txt", TestName = "DocPreprocessorSequence")]
        [TestCase("Resources.RuleObjects.DocTypeCondition.hex.txt", TestName = "DocTypeCondition")]
        [TestCase("Resources.RuleObjects.DocumentClassifier.hex.txt", TestName = "DocumentClassifier")]
        [TestCase("Resources.RuleObjects.DuplicateAndSeparateTrees.hex.txt", TestName = "DuplicateAndSeparateTrees")]
        [TestCase("Resources.RuleObjects.EliminateDuplicates.hex.txt", TestName = "EliminateDuplicates")]
        [TestCase("Resources.RuleObjects.EnhanceOCR.hex.txt", TestName = "EnhanceOCR")]
        [TestCase("Resources.RuleObjects.EntityFinder.hex.txt", TestName = "EntityFinder")]
        [TestCase("Resources.RuleObjects.EntityNameDataScorer.hex.txt", TestName = "EntityNameDataScorer")]
        [TestCase("Resources.RuleObjects.EntityNameSplitter.hex.txt", TestName = "EntityNameSplitter")]
        [TestCase("Resources.RuleObjects.ExtractLine.hex.txt", TestName = "ExtractLine")]
        [TestCase("Resources.RuleObjects.ExtractOcrTextInImageArea.hex.txt", TestName = "ExtractOcrTextInImageArea")]
        [TestCase("Resources.RuleObjects.FindFromRSD.hex.txt", TestName = "FindFromRSD")]
        [TestCase("Resources.RuleObjects.FindingRuleCondition.hex.txt", TestName = "FindingRuleCondition")]
        [TestCase("Resources.RuleObjects.ImageRegionWithLines.hex.txt", TestName = "ImageRegionWithLines")]
        [TestCase("Resources.RuleObjects.InputFinder.hex.txt", TestName = "InputFinder")]
        [TestCase("Resources.RuleObjects.InsertCharacters.hex.txt", TestName = "InsertCharacters")]
        [TestCase("Resources.RuleObjects.IntegerInputValidator.hex.txt", TestName = "IntegerInputValidator")]
        [TestCase("Resources.RuleObjects.LabDEOrderMapper.hex.txt", TestName = "LabDEOrderMapper")]
        [TestCase("Resources.RuleObjects.LearningMachineOutputHandler.hex.txt", TestName = "LearningMachineOutputHandler")]
        [TestCase("Resources.RuleObjects.LimitAsLeftPart.hex.txt", TestName = "LimitAsLeftPart")]
        [TestCase("Resources.RuleObjects.LimitAsMidPart.hex.txt", TestName = "LimitAsMidPart")]
        [TestCase("Resources.RuleObjects.LimitAsRightPart.hex.txt", TestName = "LimitAsRightPart")]
        [TestCase("Resources.RuleObjects.LocateImageRegion.hex.txt", TestName = "LocateImageRegion")]
        [TestCase("Resources.RuleObjects.LoopFinder.hex.txt", TestName = "LoopFinder")]
        [TestCase("Resources.RuleObjects.LoopPreprocessor.hex.txt", TestName = "LoopPreprocessor")]
        [TestCase("Resources.RuleObjects.MergeAttributes.hex.txt", TestName = "MergeAttributes")]
        [TestCase("Resources.RuleObjects.MergeAttributeTrees.hex.txt", TestName = "MergeAttributeTrees")]
        [TestCase("Resources.RuleObjects.MERSHandler.hex.txt", TestName = "MERSHandler")]
        [TestCase("Resources.RuleObjects.MicrFinderV1.hex.txt", TestName = "MicrFinderV1")]
        [TestCase("Resources.RuleObjects.ModifyAttributeValueOH.hex.txt", TestName = "ModifyAttributeValueOH")]
        [TestCase("Resources.RuleObjects.ModifySpatialMode.hex.txt", TestName = "ModifySpatialMode")]
        [TestCase("Resources.RuleObjects.MoveAndModifyAttributes.hex.txt", TestName = "MoveAndModifyAttributes")]
        [TestCase("Resources.RuleObjects.MoveOrCopyAttributes.hex.txt", TestName = "MoveOrCopyAttributes")]
        [TestCase("Resources.RuleObjects.MultipleCriteriaSelector.hex.txt", TestName = "MultipleCriteriaSelector")]
        [TestCase("Resources.RuleObjects.NERFinder.hex.txt", TestName = "NERFinder")]
        [TestCase("Resources.RuleObjects.NumericSequencer.hex.txt", TestName = "NumericSequencer")]
        [TestCase("Resources.RuleObjects.OCRArea.hex.txt", TestName = "OCRArea")]
        [TestCase("Resources.RuleObjects.OutputHandlerSequence.hex.txt", TestName = "OutputHandlerSequence")]
        [TestCase("Resources.RuleObjects.OutputToVOA.hex.txt", TestName = "OutputToVOA")]
        [TestCase("Resources.RuleObjects.OutputToXML.hex.txt", TestName = "OutputToXML")]
        [TestCase("Resources.RuleObjects.PadValue.hex.txt", TestName = "PadValue")]
        [TestCase("Resources.RuleObjects.PersonNameSplitter.hex.txt", TestName = "PersonNameSplitter")]
        [TestCase("Resources.RuleObjects.QueryBasedAS.hex.txt", TestName = "QueryBasedAS")]
        [TestCase("Resources.RuleObjects.ReformatPersonNames.hex.txt", TestName = "ReformatPersonNames")]
        [TestCase("Resources.RuleObjects.RegExprInputValidator.hex.txt", TestName = "RegExprInputValidator")]
        [TestCase("Resources.RuleObjects.RegExprRule.hex.txt", TestName = "RegExprRule")]
        [TestCase("Resources.RuleObjects.RemoveCharacters.hex.txt", TestName = "RemoveCharacters")]
        [TestCase("Resources.RuleObjects.RemoveEntriesFromList.hex.txt", TestName = "RemoveEntriesFromList")]
        [TestCase("Resources.RuleObjects.RemoveInvalidEntries.hex.txt", TestName = "RemoveInvalidEntries")]
        [TestCase("Resources.RuleObjects.RemoveSpatialInfo.hex.txt", TestName = "RemoveSpatialInfo")]
        [TestCase("Resources.RuleObjects.RemoveSubAttributes.hex.txt", TestName = "RemoveSubAttributes")]
        [TestCase("Resources.RuleObjects.ReplaceStrings.hex.txt", TestName = "ReplaceStrings")]
        [TestCase("Resources.RuleObjects.REPMFinder.hex.txt", TestName = "REPMFinder")]
        [TestCase("Resources.RuleObjects.RSDDataScorer.hex.txt", TestName = "RSDDataScorer")]
        [TestCase("Resources.RuleObjects.RSDFileCondition.hex.txt", TestName = "RSDFileCondition")]
        [TestCase("Resources.RuleObjects.RSDSplitter.hex.txt", TestName = "RSDSplitter")]
        [TestCase("Resources.RuleObjects.RunObjectOnAttributes.hex.txt", TestName = "RunObjectOnAttributes")]
        [TestCase("Resources.RuleObjects.SelectOnlyUniqueValues.hex.txt", TestName = "SelectOnlyUniqueValues")]
        [TestCase("Resources.RuleObjects.SelectPageRegion.hex.txt", TestName = "SelectPageRegion")]
        [TestCase("Resources.RuleObjects.SelectUsingMajority.hex.txt", TestName = "SelectUsingMajority")]
        [TestCase("Resources.RuleObjects.SetDocumentTags.hex.txt", TestName = "SetDocumentTags")]
        [TestCase("Resources.RuleObjects.ShortInputValidator.hex.txt", TestName = "ShortInputValidator")]
        [TestCase("Resources.RuleObjects.SpatialContentBasedAS.hex.txt", TestName = "SpatialContentBasedAS")]
        [TestCase("Resources.RuleObjects.SpatiallySortAttributes.hex.txt", TestName = "SpatiallySortAttributes")]
        [TestCase("Resources.RuleObjects.SpatialProximityAS.hex.txt", TestName = "SpatialProximityAS")]
        [TestCase("Resources.RuleObjects.SplitRegionIntoContentAreas.hex.txt", TestName = "SplitRegionIntoContentAreas")]
        [TestCase("Resources.RuleObjects.SSNFinder.hex.txt", TestName = "SSNFinder")]
        [TestCase("Resources.RuleObjects.StringTokenizerSplitter.hex.txt", TestName = "StringTokenizerSplitter")]
        [TestCase("Resources.RuleObjects.TemplateFinder.hex.txt", TestName = "TemplateFinder")]
        [TestCase("Resources.RuleObjects.TranslateToClosestValueInList.hex.txt", TestName = "TranslateToClosestValueInList")]
        [TestCase("Resources.RuleObjects.TranslateValue.hex.txt", TestName = "TranslateValue")]
        [TestCase("Resources.RuleObjects.TranslateValueToBestMatch.hex.txt", TestName = "TranslateValueToBestMatch")]
        [TestCase("Resources.RuleObjects.ValueAfterClue.hex.txt", TestName = "ValueAfterClue")]
        [TestCase("Resources.RuleObjects.ValueBeforeClue.hex.txt", TestName = "ValueBeforeClue")]
        [TestCase("Resources.RuleObjects.ValueConditionSelector.hex.txt", TestName = "ValueConditionSelector")]
        [TestCase("Resources.RuleObjects.ValueFromList.hex.txt", TestName = "ValueFromList")]
        public static void ExampleRuleObjects(string resourceName)
        {
            var fname = _testFiles.GetFile(resourceName);
            var lines = File.ReadLines(fname);
            var lineNum = 0;

            // Each line in file is a hex-string-encoded ObjectWithDescription where the Object is the rule object being tested
            foreach (var line in lines)
            {
                lineNum++;

                var sourceBytes = line.ToByteArray();
                var stream = new MemoryStream(sourceBytes);
                var istream = new IStreamWrapper(stream);
                var ipersistStream = (IPersistStream)new ObjectWithDescriptionClass().Clone();
                ipersistStream.Load(istream);
                var owd = (IObjectWithDescription)ipersistStream;

                string json = RuleObjectJsonSerializer.Serialize(owd);

                var owdFromJson = RuleObjectJsonSerializer.Deserialize<ObjectWithDescriptionClass>(json);

                var roundTripBytes = GetRuleObjectBytes(owdFromJson);

                RemoveGuids(ref sourceBytes, ref roundTripBytes);

                CollectionAssert.AreEqual(sourceBytes, roundTripBytes, $"Failure on line #{lineNum}");
            }
        }

        #endregion Public Test Functions

        #region Helper Methods

        public static byte[] GetRuleObjectBytes(object ruleObject)
        {
            var stream = new MemoryStream();
            var istream = new IStreamWrapper(stream);
            var ipersistStream = (IPersistStream)((ICopyableObject)ruleObject).Clone();
            ipersistStream.Save(istream, false);
            return stream.ToArray();
        }


        static void RemoveGuids(ref byte[] bytes1, ref byte[] bytes2)
        {
            var bytes1WithoutGuidsList = new List<byte>();
            var bytes2WithoutGuidsList = new List<byte>();

            var minLength = Math.Min(bytes1.Length, bytes2.Length);

            for (int byteIndex = 0; byteIndex < minLength; byteIndex++)
            {
                var isBinaryGuidStart = false;
                var isStringGuidStart = false;

                if (byteIndex + 36 <= bytes1.Length)
                {
                    var candidate = new byte[36];
                    Array.Copy(bytes1, byteIndex, candidate, 0, 36);
                    isStringGuidStart = candidate.Count(b => b == '-') == 4;
                    if (isStringGuidStart)
                    {
                        Array.Copy(bytes2, byteIndex, candidate, 0, 36);
                        isStringGuidStart = candidate.Count(b => b == '-') == 4;
                    }
                    if (isStringGuidStart)
                    {
                        isStringGuidStart = bytes1[byteIndex + 8] == '-' && bytes2[byteIndex + 8] == '-';
                        isStringGuidStart = isStringGuidStart && bytes1[byteIndex + 13] == '-' && bytes2[byteIndex + 13] == '-';
                        isStringGuidStart = isStringGuidStart && bytes1[byteIndex + 18] == '-' && bytes2[byteIndex + 18] == '-';
                        isStringGuidStart = isStringGuidStart && bytes1[byteIndex + 23] == '-' && bytes2[byteIndex + 23] == '-';
                    }
                }

                if (!isStringGuidStart && byteIndex + 24 <= bytes1.Length)
                {
                    var len1 = BitConverter.ToInt32(bytes1, byteIndex);
                    var len2 = BitConverter.ToInt32(bytes2, byteIndex);
                    var ver1 = BitConverter.ToInt32(bytes1, byteIndex + 4);
                    var ver2 = BitConverter.ToInt32(bytes2, byteIndex + 4);
                    var isValidGuidStart = len1 == 20;
                    isValidGuidStart = isValidGuidStart && len2 == 20;
                    isValidGuidStart = isValidGuidStart && ver1 == 1;
                    isValidGuidStart = isValidGuidStart && ver2 == 1;

                    // Eliminate false positive GUIDs (which can prevent the next real GUID from being recognized)
                    // by requiring at least one of the next 4 bytes to differ
                    if (isValidGuidStart)
                    {
                        isBinaryGuidStart =
                            bytes1[byteIndex + 8] != bytes2[byteIndex + 8]
                            || bytes1[byteIndex + 9] != bytes2[byteIndex + 9]
                            || bytes1[byteIndex + 10] != bytes2[byteIndex + 10]
                            || bytes1[byteIndex + 11] != bytes2[byteIndex + 11];
                    }
                }

                if (isBinaryGuidStart)
                {
                    byteIndex += 24 - 1;
                    continue;
                }
                else if (isStringGuidStart)
                {
                    byteIndex += 36 - 1;
                    continue;
                }
                else
                {
                    bytes1WithoutGuidsList.Add(bytes1[byteIndex]);
                    bytes2WithoutGuidsList.Add(bytes2[byteIndex]);
                }
            }
            bytes1WithoutGuidsList.AddRange(bytes1.Skip(minLength));
            bytes2WithoutGuidsList.AddRange(bytes2.Skip(minLength));
            bytes1 = bytes1WithoutGuidsList.ToArray();
            bytes2 = bytes2WithoutGuidsList.ToArray();
        }

        #endregion Helper Methods
    }
}
