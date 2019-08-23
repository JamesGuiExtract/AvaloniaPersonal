module Fam

open UCLID_FILEPROCESSINGLib
open EXTRACT_FILESUPPLIERSLib
open UCLID_COMUTILSLib
open UCLID_AFFILEPROCESSORSLib
open Extract.FileActionManager.FileProcessors
open UCLID_FILEPROCESSORSLib

let createFam serverName dbName actionName keepRunning =
  let fam = FileProcessingManagerClass()
  fam.DatabaseServer <- serverName
  fam.DatabaseName <- dbName
  fam.ActionName <- actionName
  let procRole = fam.FileProcessingMgmtRole
  procRole.KeepProcessingAsAdded <- keepRunning
  fam

let addTask (task: IFileProcessingTask) description (fam: FileProcessingManagerClass) =
  let procRole = fam.FileProcessingMgmtRole
  (procRole :?> IFileActionMgmtRole).Enabled <- true
  let owd = ObjectWithDescriptionClass(Object = task, Description = description)
  procRole.FileProcessors.PushBack owd

let addQueuer (queuer: IFileSupplier) description forceQueue (fam: FileProcessingManagerClass) =
  let suppRole = fam.FileSupplyingMgmtRole
  (suppRole :?> IFileActionMgmtRole).Enabled <- true
  let owd = ObjectWithDescriptionClass(Object = queuer, Description = description)
  let queuerWrapper = FileSupplierDataClass(FileSupplier = owd, ForceProcessing = forceQueue)
  suppRole.FileSuppliers.PushBack queuerWrapper

let createExecuteRules rulesPath inputVOAPath =
  let task = AFEngineFileProcessorClass()
  task.RuleSetFileName <- rulesPath
  match inputVOAPath with
  | Some path ->
      task.UseDataInputFile <- true
      task.DataInputFileName <- path
  | None -> ()
  task

let createCopyIfMissingTask sourcePath destPath =
  let task = CopyMoveDeleteFileProcessorClass()
  task.SetCopyFiles(sourcePath, destPath)
  task.SourceMissingType <- ECMDSourceMissingType.kCMDSourceMissingSkip
  task.ModifySourceDocName <- false
  task.CreateFolder <- true
  task.DestinationPresentType <- ECMDDestinationPresentType.kCMDDestinationPresentSkip
  task

let createStoreAttributes voaPath attributeSet expandAttributes =
  let task = StoreAttributesInDBTask()
  task.VOAFileName <- voaPath
  task.AttributeSetName <- attributeSet
  task.StoreDiscreteData <- expandAttributes
  task

let createSetMetadataField name value =
  let task = new SetMetadataTask()
  task.FieldName <- name
  task.Value <- value
  task

let createSetToPending actionName =
  let task = SetActionStatusFileProcessorClass()
  task.ActionName <- actionName
  task.ActionStatus <- int EActionStatus.kActionPending
  task

let createFolderQueuer folder recurse watch =
  let queuer = FolderFSClass()
  queuer.FolderName <- folder
  queuer.FileExtensions <- "*.tif;*.pdf"
  queuer.RecurseFolders <- recurse
  queuer.AddedFiles <- watch
  queuer.ModifiedFiles <- watch
  queuer.TargetOfMoveOrRename <- watch
  queuer

let createFileListQueuer file =
  let queuer = DynamicFileListFSClass()
  queuer.FileName <- file
  queuer

let saveTo path (fam: FileProcessingManagerClass) =
  fam.SaveTo(path, true)

let loadFrom path =
  let fam = FileProcessingManagerClass()
  fam.LoadFrom(path, false)
  fam
