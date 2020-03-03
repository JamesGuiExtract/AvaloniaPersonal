namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

module LearningMachineOutputHandler =
  type LearningMachineOutputHandlerClass = LearningMachineOutputHandler
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: ILearningMachineOutputHandler): Dto.LearningMachineOutputHandler =
    { SavedMachinePath = domain.SavedMachinePath
      PreserveInputAttributes = domain.PreserveInputAttributes }

  let fromDto (dto: Dto.LearningMachineOutputHandler) =
    LearningMachineOutputHandlerClass
      ( SavedMachinePath=dto.SavedMachinePath,
        PreserveInputAttributes=dto.PreserveInputAttributes )

type LearningMachineOutputHandlerConverter() =
  inherit RuleObjectConverter<LearningMachineOutputHandler, ILearningMachineOutputHandler, Dto.LearningMachineOutputHandler>()
  override _.toDto _mc domain = domain |> LearningMachineOutputHandler.toDto
  override _.fromDto _mc dto = dto |> LearningMachineOutputHandler.fromDto
