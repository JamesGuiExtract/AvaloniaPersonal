using Extract.Testing.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.IO;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules.Json.Test
{
    [TestFixture]
    [Category("JsonRuleObjects")]
    public class JsonToDtoViaDomain
    {
        #region Fields

        static TestFileManager<JsonToDtoViaDomain> _testFiles;

        #endregion Fields

        #region Setup and Teardown

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<JsonToDtoViaDomain>();
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
        /// Load example JSON RSD files, convert to DTO, save as JSON, and reload to DTO. Compare the DTO object from the original JSON with the DTO object from the saved JSON.
        /// </summary>
        [Category("RulesJsonSerialization")]
        [TestCase("Resources.RuleSets.Indexing-AddressFinders-AddressFinder.json.rsd", TestName = "Indexing-AddressFinders-AddressFinder")]
        [TestCase("Resources.RuleSets.Indexing-DocumentDate-commonOH.json.rsd", TestName = "Indexing-DocumentDate-commonOH")]
        [TestCase("Resources.RuleSets.LabDE-PatientInfo-Name-LabSpecificRules.json.rsd", TestName = "LabDE-PatientInfo-Name-LabSpecificRules")]
        [TestCase("Resources.RuleSets.LabDE-PatientInfo-Name-reOCR.json.rsd", TestName = "LabDE-PatientInfo-Name-reOCR")]
        [TestCase("Resources.RuleSets.LabDE-SwipingRules-Date.json.rsd", TestName = "LabDE-SwipingRules-Date")]
        [TestCase("Resources.RuleSets.LabDE-TestResults-processMultipleDates.json.rsd", TestName = "LabDE-TestResults-processMultipleDates")]
        [TestCase("Resources.RuleSets.ReusableComponents-getDocAndPagesCC.json.rsd", TestName = "ReusableComponents-getDocAndPagesCC")]
        [TestCase("Resources.RuleSets.ReusableComponents-getDocAndPagesCChelper.json.rsd", TestName = "ReusableComponents-getDocAndPagesCChelper")]
        [TestCase("Resources.RuleSets.ReusableComponents-MLFeatureGen-LocationFinder-GetPageBoundaries.json.rsd", TestName = "ReusableComponents-MLFeatureGen-LocationFinder-GetPageBoundaries")]
        [TestCase("Resources.RuleSets.ReusableComponents-MLFeatureGen-LocationFinder-LocationFinder.json.rsd", TestName = "ReusableComponents-MLFeatureGen-LocationFinder-LocationFinder")]
        [TestCase("Resources.RuleSets.ReusableComponents-PS-MasterConfidence.json.rsd", TestName = "ReusableComponents-PS-MasterConfidence")]
        [TestCase("Resources.RuleSets.ReusableComponents-RemoveSubattributeDuplicates.json.rsd", TestName = "ReusableComponents-RemoveSubattributeDuplicates")]
        [TestCase("Resources.RuleSets.ReusableComponents-spansPages.json.rsd", TestName = "ReusableComponents-spansPages")]
        [TestCase("Resources.RuleSets.RunTest.json.rsd", TestName = "RunTest")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-CompressAutoPaginationVOA.json.rsd", TestName = "Essentia-Solution-Rules-CompressAutoPaginationVOA")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-CreateDocDescVOA.json.rsd", TestName = "Essentia-Solution-Rules-CreateDocDescVOA")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-DocumentDate-HCdates.json.rsd", TestName = "Essentia-Solution-Rules-DocumentDate-HCdates")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-FilenameFix.json.rsd", TestName = "Essentia-Solution-Rules-FilenameFix")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-findDocumentData.json.rsd", TestName = "Essentia-Solution-Rules-findDocumentData")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-Master.json.rsd", TestName = "Essentia-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-Master_NoVOA.json.rsd", TestName = "Essentia-Solution-Rules-Master_NoVOA")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ML-Deleter-deleter.json.rsd", TestName = "Essentia-Solution-Rules-ML-Deleter-deleter")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ML-Pagination-createProtofeatures.json.rsd", TestName = "Essentia-Solution-Rules-ML-Pagination-createProtofeatures")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ML-Pagination-getImagePageNumber.json.rsd", TestName = "Essentia-Solution-Rules-ML-Pagination-getImagePageNumber")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-PaginationMaster.json.rsd", TestName = "Essentia-Solution-Rules-PaginationMaster")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-PatientInfo-findDOB.json.rsd", TestName = "Essentia-Solution-Rules-PatientInfo-findDOB")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-ShadowFax.json.rsd", TestName = "Essentia-Solution-Rules-ShadowFax")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-Singles-PaginateSingles.json.rsd", TestName = "Essentia-Solution-Rules-Singles-PaginateSingles")]
        [TestCase("Resources.RuleSets.Essentia-Solution-Rules-UnsolicitedOrders-PopulateOrderAndCSN.json.rsd", TestName = "Essentia-Solution-Rules-UnsolicitedOrders-PopulateOrderAndCSN")]
        [TestCase("Resources.RuleSets._Demo-Demo_HIM-Solution-Rules-findDocumentData.json.rsd", TestName = "_Demo-Demo_HIM-Solution-Rules-findDocumentData")]
        [TestCase("Resources.RuleSets._Demo-Demo_HIM-Solution-Rules-PaginationMaster.json.rsd", TestName = "_Demo-Demo_HIM-Solution-Rules-PaginationMaster")]
        [TestCase("Resources.RuleSets._Demo-Demo_HIM-Solution-Rules-PaginationRules-main.json.rsd", TestName = "_Demo-Demo_HIM-Solution-Rules-PaginationRules-main")]
        [TestCase("Resources.RuleSets.American Family-Rules-master.json.rsd", TestName = "American Family-Rules-master")]
        [TestCase("Resources.RuleSets.American Family-TestingFiles-testAllSingleValues.json.rsd", TestName = "American Family-TestingFiles-testAllSingleValues")]
        [TestCase("Resources.RuleSets.CA - Sacramento Demo-Rules-SSNRedaction-DOB.json.rsd", TestName = "CA - Sacramento Demo-Rules-SSNRedaction-DOB")]
        [TestCase("Resources.RuleSets.CA - Santa Clara-Solution-Rules-docTypeOH.json.rsd", TestName = "CA - Santa Clara-Solution-Rules-docTypeOH")]
        [TestCase("Resources.RuleSets.MN - District - Tyler-Rules-rules.json.rsd", TestName = "MN - District - Tyler-Rules-rules")]
        [TestCase("Resources.RuleSets.Surefire - Judgments-Solution-Rules-Master.json.rsd", TestName = "Surefire - Judgments-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.Surefire - SNL-Solution-Rules-Master.json.rsd", TestName = "Surefire - SNL-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.Surefire-Solution-Rules-Common-selectMajoritySubs.json.rsd", TestName = "Surefire-Solution-Rules-Common-selectMajoritySubs")]
        [TestCase("Resources.RuleSets.Surefire-Solution-Rules-Master.json.rsd", TestName = "Surefire-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.TX - Tarrant-DataEntryConfig-SaveVolumeAndPage.json.rsd", TestName = "TX - Tarrant-DataEntryConfig-SaveVolumeAndPage")]
        [TestCase("Resources.RuleSets.ZZ-Situs-Solution-Rules-Master.json.rsd", TestName = "ZZ-Situs-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.ZZ-Situs-Solution-Rules-Rent-data.json.rsd", TestName = "ZZ-Situs-Solution-Rules-Rent-data")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-CreateTrainingDataAndTryToLearn.json.rsd", TestName = "Arcondis-Solution-Rules-CreateTrainingDataAndTryToLearn")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-Ensemble-main.json.rsd", TestName = "Arcondis-Solution-Rules-Ensemble-main")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-Ensemble-SensitiveRows.json.rsd", TestName = "Arcondis-Solution-Rules-Ensemble-SensitiveRows")]
        [TestCase("Resources.RuleSets.Arcondis-Solution-Rules-Master.json.rsd", TestName = "Arcondis-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.CA - Amador - BMI-Rules-Master.json.rsd", TestName = "CA - Amador - BMI-Rules-Master")]
        [TestCase("Resources.RuleSets.CA - Humboldt - BMI Imaging-Rules-Utils-CChelper.json.rsd", TestName = "CA - Humboldt - BMI Imaging-Rules-Utils-CChelper")]
        [TestCase("Resources.RuleSets.CA - Marin Vital Records - DFM-Rules-Master.json.rsd", TestName = "CA - Marin Vital Records - DFM-Rules-Master")]
        [TestCase("Resources.RuleSets.CA - Orange - SouthTech-Rules-Master.json.rsd", TestName = "CA - Orange - SouthTech-Rules-Master")]
        [TestCase("Resources.RuleSets.Delta Dental-Rules-Master.json.rsd", TestName = "Delta Dental-Rules-Master")]
        [TestCase("Resources.RuleSets.Demo_IDShield_EDI_sanitize-Rules-People-People.json.rsd", TestName = "Demo_IDShield_EDI_sanitize-Rules-People-People")]
        [TestCase("Resources.RuleSets.Demo_MGIC-Rules-Master.json.rsd", TestName = "Demo_MGIC-Rules-Master")]
        [TestCase("Resources.RuleSets.Demo_PHI-Rules-MRN-MRNMaster.json.rsd", TestName = "Demo_PHI-Rules-MRN-MRNMaster")]
        [TestCase("Resources.RuleSets.IL - Ogle-Rules-Master.json.rsd", TestName = "IL - Ogle-Rules-Master")]
        [TestCase("Resources.RuleSets.IN - District Rules - ACS-Rules-Master.json.rsd", TestName = "IN - District Rules - ACS-Rules-Master")]
        [TestCase("Resources.RuleSets.IN - SOS - GCR-Solution-Rules-Master.json.rsd", TestName = "IN - SOS - GCR-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.IN - SOS-Rules-perPage.json.rsd", TestName = "IN - SOS-Rules-perPage")]
        [TestCase("Resources.RuleSets.MGIC-Rules-SensitivePageClassifier-makePageClue.json.rsd", TestName = "MGIC-Rules-SensitivePageClassifier-makePageClue")]
        [TestCase("Resources.RuleSets.MGIC-Utils-OnlyPageClues.json.rsd", TestName = "MGIC-Utils-OnlyPageClues")]
        [TestCase("Resources.RuleSets.OH - BWC-Rules-Master.json.rsd", TestName = "OH - BWC-Rules-Master")]
        [TestCase("Resources.RuleSets.OH - Cuyahoga Probate Court - Proware-Rules-decrement.json.rsd", TestName = "OH - Cuyahoga Probate Court - Proware-Rules-decrement")]
        [TestCase("Resources.RuleSets.Pfizer-Rules-Master.json.rsd", TestName = "Pfizer-Rules-Master")]
        [TestCase("Resources.RuleSets.Pfizer-Rules-ML-currency-cellClassifier-getCandidates.json.rsd", TestName = "Pfizer-Rules-ML-currency-cellClassifier-getCandidates")]
        [TestCase("Resources.RuleSets.Pfizer-Rules-ML-main.json.rsd", TestName = "Pfizer-Rules-ML-main")]
        [TestCase("Resources.RuleSets.TX - DallasCountyElections-Rules-Master.json.rsd", TestName = "TX - DallasCountyElections-Rules-Master")]
        [TestCase("Resources.RuleSets.WI - Dodge - TriMin-Rules-Master.json.rsd", TestName = "WI - Dodge - TriMin-Rules-Master")]
        [TestCase("Resources.RuleSets.WI - Dodge - TriMin-Rules-Utils-PageCC.json.rsd", TestName = "WI - Dodge - TriMin-Rules-Utils-PageCC")]
        [TestCase("Resources.RuleSets.WV - SOS UCC-Rules-Master.json.rsd", TestName = "WV - SOS UCC-Rules-Master")]
        [TestCase("Resources.RuleSets.CA - Cedar Sinai-Solution-Rules-Master.json.rsd", TestName = "CA - Cedar Sinai-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.CA - Cedar Sinai-Solution-Rules-TestResults-getTests.json.rsd", TestName = "CA - Cedar Sinai-Solution-Rules-TestResults-getTests")]
        [TestCase("Resources.RuleSets.Demo_LabDE-Solution-Rules-DocumentSorter-Master.json.rsd", TestName = "Demo_LabDE-Solution-Rules-DocumentSorter-Master")]
        [TestCase("Resources.RuleSets.Hurley-Solution-Rules-SplitDeletedPages.json.rsd", TestName = "Hurley-Solution-Rules-SplitDeletedPages")]
        [TestCase("Resources.RuleSets.Northwestern Memorial Hospital-Solution-Rules-ShadowFax.json.rsd", TestName = "Northwestern Memorial Hospital-Solution-Rules-ShadowFax")]
        [TestCase("Resources.RuleSets.UW Transplant-Solution-Rules-DocumentSorter-Master.json.rsd", TestName = "UW Transplant-Solution-Rules-DocumentSorter-Master")]
        [TestCase("Resources.RuleSets.UW Transplant-Solution-Rules-PaginationMaster.json.rsd", TestName = "UW Transplant-Solution-Rules-PaginationMaster")]
        [TestCase("Resources.RuleSets.UW Transplant-Solution-Rules-Swiping-CollectionDate.json.rsd", TestName = "UW Transplant-Solution-Rules-Swiping-CollectionDate")]
        [TestCase("Resources.RuleSets.WI - Exact Sciences-Solution-Rules-Master.json.rsd", TestName = "WI - Exact Sciences-Solution-Rules-Master")]
        [TestCase("Resources.RuleSets.WI - Exact Sciences-Solution-Rules-Master_NoUSS.json.rsd", TestName = "WI - Exact Sciences-Solution-Rules-Master_NoUSS")]
        [TestCase("Resources.RuleSets.empty.json.rsd", TestName = "empty")]
        public static void ExampleRuleSets(string resourceName)
        {
            var rsdPath = _testFiles.GetFile(resourceName);
            var rulesetFromFile = new RuleSetClass();
            rulesetFromFile.LoadFrom(rsdPath, false);

            var (json, dtoFromDomain) = RuleObjectJsonSerializer.Serialize<RuleSetClass, Dto.RuleSet>(rulesetFromFile);
            var (_, dtoFromJson) = RuleObjectJsonSerializer.DeserializeIncludeIntermediateObject<RuleSetClass>(json);

            Assert.AreEqual(dtoFromDomain, dtoFromJson);
        }

        /// <summary>
        /// Up to 100 distinct examples of each rule object taken from the customer rules git repo.
        /// Sequence objects (select multiple preprocessor/output handler) were limited to a single example.
        /// Compare JSON from a file to JSON after doing a round-trip through the domain object and back to JSON text.
        /// Rule objects with multiple DTO versions need to be tested separately to verify that legacy versions can be loaded
        /// (see LegacyVersionRuleObjects test below)
        /// </summary>
        [Test, Category("RulesJsonSerialization")]
        [TestCase("Resources.RuleObjects.AdvancedReplaceString.json", TestName = "AdvancedReplaceString")]
        [TestCase("Resources.RuleObjects.AutoShrinkRedactionZones.json", TestName = "AutoShrinkRedactionZones")]
        [TestCase("Resources.RuleObjects.BarcodeFinder.json", TestName = "BarcodeFinder")]
        [TestCase("Resources.RuleObjects.BlockFinder.json", TestName = "BlockFinder")]
        [TestCase("Resources.RuleObjects.BoxFinder.json", TestName = "BoxFinder")]
        [TestCase("Resources.RuleObjects.ChangeCase.json", TestName = "ChangeCase")]
        [TestCase("Resources.RuleObjects.CharacterConfidenceCondition.json", TestName = "CharacterConfidenceCondition")]
        [TestCase("Resources.RuleObjects.CharacterConfidenceDS.json", TestName = "CharacterConfidenceDS")]
        [TestCase("Resources.RuleObjects.CheckFinder.json", TestName = "CheckFinder")]
        [TestCase("Resources.RuleObjects.ConditionalAttributeModifier.json", TestName = "ConditionalAttributeModifier")]
        [TestCase("Resources.RuleObjects.ConditionalOutputHandler.json", TestName = "ConditionalOutputHandler")]
        [TestCase("Resources.RuleObjects.ConditionalPreprocessor.json", TestName = "ConditionalPreprocessor")]
        [TestCase("Resources.RuleObjects.ConditionalValueFinder.json", TestName = "ConditionalValueFinder")]
        [TestCase("Resources.RuleObjects.CreateAttribute.json", TestName = "CreateAttribute")]
        [TestCase("Resources.RuleObjects.CreateValue.json", TestName = "CreateValue")]
        [TestCase("Resources.RuleObjects.DataEntryPreloader.json", TestName = "DataEntryPreloader")]
        [TestCase("Resources.RuleObjects.DataQueryRuleObjectV1.1.json", TestName = "DataQueryRuleObjectV1_1")]
        [TestCase("Resources.RuleObjects.DataScorerBasedAS.json", TestName = "DataScorerBasedAS")]
        [TestCase("Resources.RuleObjects.DateInputValidator.json", TestName = "DateInputValidator")]
        [TestCase("Resources.RuleObjects.DateTimeSplitter.json", TestName = "DateTimeSplitter")]
        [TestCase("Resources.RuleObjects.DocPreprocessorSequence.json", TestName = "DocPreprocessorSequence")]
        [TestCase("Resources.RuleObjects.DocTypeCondition.json", TestName = "DocTypeCondition")]
        [TestCase("Resources.RuleObjects.DocumentClassifier.json", TestName = "DocumentClassifier")]
        [TestCase("Resources.RuleObjects.DoubleInputValidator.json", TestName = "DoubleInputValidator")]
        [TestCase("Resources.RuleObjects.DuplicateAndSeparateTrees.json", TestName = "DuplicateAndSeparateTrees")]
        [TestCase("Resources.RuleObjects.EliminateDuplicates.json", TestName = "EliminateDuplicates")]
        [TestCase("Resources.RuleObjects.EnhanceOCR.json", TestName = "EnhanceOCR")]
        [TestCase("Resources.RuleObjects.EntityFinder.json", TestName = "EntityFinder")]
        [TestCase("Resources.RuleObjects.EntityNameDataScorer.json", TestName = "EntityNameDataScorer")]
        [TestCase("Resources.RuleObjects.EntityNameSplitter.json", TestName = "EntityNameSplitter")]
        [TestCase("Resources.RuleObjects.ExtractLine.json", TestName = "ExtractLine")]
        [TestCase("Resources.RuleObjects.ExtractOcrTextInImageArea.json", TestName = "ExtractOcrTextInImageArea")]
        [TestCase("Resources.RuleObjects.FindFromRSD.json", TestName = "FindFromRSD")]
        [TestCase("Resources.RuleObjects.FindingRuleCondition.json", TestName = "FindingRuleCondition")]
        [TestCase("Resources.RuleObjects.FloatInputValidator.json", TestName = "FloatInputValidator")]
        [TestCase("Resources.RuleObjects.FSharpPreprocessorV2.json", TestName = "FSharpPreprocessorV2")]
        [TestCase("Resources.RuleObjects.ImageRegionWithLines.json", TestName = "ImageRegionWithLines")]
        [TestCase("Resources.RuleObjects.InputFinder.json", TestName = "InputFinder")]
        [TestCase("Resources.RuleObjects.InsertCharacters.json", TestName = "InsertCharacters")]
        [TestCase("Resources.RuleObjects.IntegerInputValidator.json", TestName = "IntegerInputValidator")]
        [TestCase("Resources.RuleObjects.LabDEOrderMapper.json", TestName = "LabDEOrderMapper")]
        [TestCase("Resources.RuleObjects.LearningMachineOutputHandler.json", TestName = "LearningMachineOutputHandler")]
        [TestCase("Resources.RuleObjects.LimitAsLeftPart.json", TestName = "LimitAsLeftPart")]
        [TestCase("Resources.RuleObjects.LimitAsMidPart.json", TestName = "LimitAsMidPart")]
        [TestCase("Resources.RuleObjects.LimitAsRightPart.json", TestName = "LimitAsRightPart")]
        [TestCase("Resources.RuleObjects.LocateImageRegion.json", TestName = "LocateImageRegion")]
        [TestCase("Resources.RuleObjects.LoopFinder.json", TestName = "LoopFinder")]
        [TestCase("Resources.RuleObjects.LoopPreprocessor.json", TestName = "LoopPreprocessor")]
        [TestCase("Resources.RuleObjects.MergeAttributes.json", TestName = "MergeAttributes")]
        [TestCase("Resources.RuleObjects.MergeAttributeTrees.json", TestName = "MergeAttributeTrees")]
        [TestCase("Resources.RuleObjects.MERSHandler.json", TestName = "MERSHandler")]
        [TestCase("Resources.RuleObjects.MicrFinderV1.json", TestName = "MicrFinderV1")]
        [TestCase("Resources.RuleObjects.MicrFinderV2.1.json", TestName = "MicrFinderV2")]
        [TestCase("Resources.RuleObjects.ModifyAttributeValueOH.json", TestName = "ModifyAttributeValueOH")]
        [TestCase("Resources.RuleObjects.ModifySpatialMode.json", TestName = "ModifySpatialMode")]
        [TestCase("Resources.RuleObjects.MoveAndModifyAttributes.json", TestName = "MoveAndModifyAttributes")]
        [TestCase("Resources.RuleObjects.MoveOrCopyAttributes.json", TestName = "MoveOrCopyAttributes")]
        [TestCase("Resources.RuleObjects.MultipleCriteriaSelector.json", TestName = "MultipleCriteriaSelector")]
        [TestCase("Resources.RuleObjects.NERFinder.json", TestName = "NERFinder")]
        [TestCase("Resources.RuleObjects.NumericSequencer.json", TestName = "NumericSequencer")]
        [TestCase("Resources.RuleObjects.OCRArea.json", TestName = "OCRArea")]
        [TestCase("Resources.RuleObjects.OutputHandlerSequence.json", TestName = "OutputHandlerSequence")]
        [TestCase("Resources.RuleObjects.OutputToVOA.json", TestName = "OutputToVOA")]
        [TestCase("Resources.RuleObjects.OutputToXML.json", TestName = "OutputToXML")]
        [TestCase("Resources.RuleObjects.PadValue.json", TestName = "PadValue")]
        [TestCase("Resources.RuleObjects.PersonNameSplitter.json", TestName = "PersonNameSplitter")]
        [TestCase("Resources.RuleObjects.QueryBasedAS.json", TestName = "QueryBasedAS")]
        [TestCase("Resources.RuleObjects.ReformatPersonNames.json", TestName = "ReformatPersonNames")]
        [TestCase("Resources.RuleObjects.RegExprInputValidator.json", TestName = "RegExprInputValidator")]
        [TestCase("Resources.RuleObjects.RegExprRule.json", TestName = "RegExprRule")]
        [TestCase("Resources.RuleObjects.RemoveCharacters.json", TestName = "RemoveCharacters")]
        [TestCase("Resources.RuleObjects.RemoveEntriesFromList.json", TestName = "RemoveEntriesFromList")]
        [TestCase("Resources.RuleObjects.RemoveInvalidEntries.json", TestName = "RemoveInvalidEntries")]
        [TestCase("Resources.RuleObjects.RemoveSpatialInfo.json", TestName = "RemoveSpatialInfo")]
        [TestCase("Resources.RuleObjects.RemoveSubAttributes.json", TestName = "RemoveSubAttributes")]
        [TestCase("Resources.RuleObjects.ReplaceStrings.json", TestName = "ReplaceStrings")]
        [TestCase("Resources.RuleObjects.REPMFinder.json", TestName = "REPMFinder")]
        [TestCase("Resources.RuleObjects.RSDDataScorer.json", TestName = "RSDDataScorer")]
        [TestCase("Resources.RuleObjects.RSDFileCondition.json", TestName = "RSDFileCondition")]
        [TestCase("Resources.RuleObjects.RSDSplitter.json", TestName = "RSDSplitter")]
        [TestCase("Resources.RuleObjects.RunObjectOnAttributes.json", TestName = "RunObjectOnAttributes")]
        [TestCase("Resources.RuleObjects.SelectOnlyUniqueValues.json", TestName = "SelectOnlyUniqueValues")]
        [TestCase("Resources.RuleObjects.SelectPageRegion.json", TestName = "SelectPageRegion")]
        [TestCase("Resources.RuleObjects.SelectUsingMajority.json", TestName = "SelectUsingMajority")]
        [TestCase("Resources.RuleObjects.SetDocumentTags.json", TestName = "SetDocumentTags")]
        [TestCase("Resources.RuleObjects.ShortInputValidator.json", TestName = "ShortInputValidator")]
        [TestCase("Resources.RuleObjects.SpatialContentBasedAS.json", TestName = "SpatialContentBasedAS")]
        [TestCase("Resources.RuleObjects.SpatiallySortAttributes.json", TestName = "SpatiallySortAttributes")]
        [TestCase("Resources.RuleObjects.SpatialProximityAS.json", TestName = "SpatialProximityAS")]
        [TestCase("Resources.RuleObjects.SplitRegionIntoContentAreas.json", TestName = "SplitRegionIntoContentAreas")]
        [TestCase("Resources.RuleObjects.SSNFinder.json", TestName = "SSNFinder")]
        [TestCase("Resources.RuleObjects.StringTokenizerModifier.json", TestName = "StringTokenizerModifier")]
        [TestCase("Resources.RuleObjects.StringTokenizerSplitter.json", TestName = "StringTokenizerSplitter")]
        [TestCase("Resources.RuleObjects.TemplateFinder.json", TestName = "TemplateFinder")]
        [TestCase("Resources.RuleObjects.TranslateToClosestValueInList.json", TestName = "TranslateToClosestValueInList")]
        [TestCase("Resources.RuleObjects.TranslateValue.json", TestName = "TranslateValue")]
        [TestCase("Resources.RuleObjects.TranslateValueToBestMatch.json", TestName = "TranslateValueToBestMatch")]
        [TestCase("Resources.RuleObjects.ValueAfterClue.json", TestName = "ValueAfterClue")]
        [TestCase("Resources.RuleObjects.ValueBeforeClue.json", TestName = "ValueBeforeClue")]
        [TestCase("Resources.RuleObjects.ValueConditionSelector.json", TestName = "ValueConditionSelector")]
        [TestCase("Resources.RuleObjects.ValueFromList.json", TestName = "ValueFromList")]
        public static void ExampleRuleObjects(string resourceName)
        {
            var fname = _testFiles.GetFile(resourceName);
            var serializer = JsonSerializer.CreateDefault(RuleObjectJsonSerializer.Settings);

            int objectsCompared = 0;
            using (var stream = new FileStream(fname, FileMode.Open))
            using (StreamReader streamReader = new StreamReader(stream))
            using (JsonReader jsonReader = new JsonTextReader(streamReader))
            {
                while (jsonReader.Read())
                {
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        var objectFromReader = JObject.Load(jsonReader);
                        var dtoFromJson = objectFromReader.ToObject<Dto.ObjectWithDescription>(serializer);
                        var domain = (IObjectWithDescription)Domain.RuleObjectConverter.ConvertFromDto(dtoFromJson);
                        var (roundTripJson, _) = RuleObjectJsonSerializer.Serialize<IObjectWithDescription, Dto.ObjectWithDescription>(domain);

                        Assert.AreEqual(objectFromReader.ToString(), roundTripJson);

                        objectsCompared++;
                    }
                }
            }
            Assert.Greater(objectsCompared, 0);
        }

        /// <summary>
        /// Load from legacy version and current version json into DTO and compare
        /// </summary>
        [Category("LegacyRulesJsonDeserialization")]
        [TestCase("Resources.RuleObjects.FSharpPreprocessor.json", "Resources.RuleObjects.FSharpPreprocessorV2.json", TestName = "LegacyFSharpPreprocessor")]
        [TestCase("Resources.RuleObjects.MicrFinderV2.json", "Resources.RuleObjects.MicrFinderV2.1.json", TestName = "LegacyMICRFinder")]
        [TestCase("Resources.RuleObjects.DataQueryRuleObject.json", "Resources.RuleObjects.DataQueryRuleObjectV1.1.json", TestName = "LegacyDataQueryRuleObject")]
        public static void LegacyVersionRuleObjects(string legacyResourceName, string currentVersionResourceName)
        {
            var legacyJsonFile = _testFiles.GetFile(legacyResourceName);
            var currentVersionJsonFile = _testFiles.GetFile(currentVersionResourceName);
            var serializer = JsonSerializer.CreateDefault(RuleObjectJsonSerializer.Settings);

            int objectsCompared = 0;
            using var legacyStream = new FileStream(legacyJsonFile, FileMode.Open);
            using var legacyStreamReader = new StreamReader(legacyStream);
            using var legacyJsonReader = new JsonTextReader(legacyStreamReader);

            using var currentVersionStream = new FileStream(currentVersionJsonFile, FileMode.Open);
            using var currentVersionStreamReader = new StreamReader(currentVersionStream);
            using var currentVersionJsonReader = new JsonTextReader(currentVersionStreamReader);

            while (legacyJsonReader.Read())
            {
                Assert.That(currentVersionJsonReader.Read());

                if (legacyJsonReader.TokenType == JsonToken.StartObject)
                {
                    Assert.That(currentVersionJsonReader.TokenType == JsonToken.StartObject);

                    var dtoInputFromJson = serializer.Deserialize<Dto.ObjectWithDescription>(legacyJsonReader);
                    var dtoExpectedResultFromJson = serializer.Deserialize<Dto.ObjectWithDescription>(currentVersionJsonReader);
                    var domain = (IObjectWithDescription)Domain.RuleObjectConverter.ConvertFromDto(dtoInputFromJson);
                    var (json, dtoFromDomain) = RuleObjectJsonSerializer.Serialize<IObjectWithDescription, Dto.ObjectWithDescription>(domain);

                    Assert.AreEqual(dtoExpectedResultFromJson, dtoFromDomain);

                    objectsCompared++;
                }
            }
            Assert.Greater(objectsCompared, 0);
        }
        #endregion
    }
}
