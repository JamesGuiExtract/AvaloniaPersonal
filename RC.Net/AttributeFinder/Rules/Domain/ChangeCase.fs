namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEMODIFIERSLib

module ChangeCaseType =
  let toDto = function
  | EChangeCaseType.kNoChangeCase -> Dto.ChangeCaseType.NoChangeCase
  | EChangeCaseType.kMakeUpperCase -> Dto.ChangeCaseType.MakeUpperCase
  | EChangeCaseType.kMakeLowerCase -> Dto.ChangeCaseType.MakeLowerCase
  | EChangeCaseType.kMakeTitleCase -> Dto.ChangeCaseType.MakeTitleCase
  | other -> failwithf "Not a valid EChangeCaseType! %A" other

  let fromDto = function
  | Dto.ChangeCaseType.NoChangeCase -> EChangeCaseType.kNoChangeCase
  | Dto.ChangeCaseType.MakeUpperCase -> EChangeCaseType.kMakeUpperCase
  | Dto.ChangeCaseType.MakeLowerCase -> EChangeCaseType.kMakeLowerCase
  | Dto.ChangeCaseType.MakeTitleCase -> EChangeCaseType.kMakeTitleCase
  | other -> failwithf "Not a valid ChangeCaseType! %A" other

module ChangeCase =
  let toDto (domain: IChangeCase): Dto.ChangeCase =
    { CaseType = domain.CaseType |> ChangeCaseType.toDto }

  let fromDto (dto: Dto.ChangeCase) =
    ChangeCaseClass
      ( CaseType=(dto.CaseType |> ChangeCaseType.fromDto) )

type ChangeCaseConverter() =
  inherit RuleObjectConverter<ChangeCaseClass, IChangeCase, Dto.ChangeCase>()
  override _.toDto _mc domain = domain |> ChangeCase.toDto
  override _.fromDto _mc dto = dto |> ChangeCase.fromDto
