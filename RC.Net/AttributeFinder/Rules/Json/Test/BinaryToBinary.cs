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

        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<BinaryToBinary>();
        }

        [TestFixtureTearDown]
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
        [TestCase("Resources.RuleObjects.FSharpPreprocessor.hex.txt", TestName = "FSharpPreprocessor")]
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
        [TestCase("Resources.RuleObjects.MicrFinderV2.hex.txt", TestName = "MicrFinderV2")]
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
