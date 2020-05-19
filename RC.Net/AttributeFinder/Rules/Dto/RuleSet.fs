namespace Extract.AttributeFinder.Rules.Dto

open System.Collections.Generic

type Counter = {
  ID: int
  Name: string
  ByPage: bool
  Enabled: bool
}

type OCRParameter = KeyValuePair<obj, obj>

type RuleSetRunMode =
| PassInputVOAToOutput = 0
| RunPerDocument = 1
| RunPerPage = 2

type RuleSet = {
  SavedWithSoftwareVersion: string
  Comments: string
  Counters: Counter list
  FKBVersion: string
  ForInternalUseOnly: bool
  IsSwipingRule: bool
  OCRParameters: OCRParameter list
  RunMode: RuleSetRunMode
  InsertAttributesUnderParent: bool
  InsertParentName: string
  InsertParentValue: string
  DeepCopyInput: bool
  GlobalDocPreprocessor: ObjectWithDescription
  IgnorePreprocessorErrors: bool
  AttributeNameToInfoMap: Map<string, AttributeFindInfo>
  GlobalOutputHandler: ObjectWithDescription
  IgnoreOutputHandlerErrors: bool
}

module RuleSet =
  let version = 
    let assembly = typeof<RuleSet>.Assembly
    if not (assembly = null)
    then assembly.GetName().Version.ToString()
    else "Unknown";

  let GetWithCurrentSoftwareVersion (ruleSet: RuleSet) = { ruleSet with SavedWithSoftwareVersion = version }
