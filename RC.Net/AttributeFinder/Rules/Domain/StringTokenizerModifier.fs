namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEMODIFIERSLib

module NumOfTokensType =
  let toDto = function
  | ENumOfTokensType.kAnyNumber -> Dto.NumOfTokensType.Any
  | ENumOfTokensType.kEqualNumber -> Dto.NumOfTokensType.Equal
  | ENumOfTokensType.kGreaterThanNumber -> Dto.NumOfTokensType.GreaterThan
  | ENumOfTokensType.kGreaterThanEqualNumber -> Dto.NumOfTokensType.GreaterThanOrEqual
  | other -> failwithf "Not a valid ENumOfTokensType! %A" other

  let fromDto = function
  | Dto.NumOfTokensType.Any -> ENumOfTokensType.kAnyNumber
  | Dto.NumOfTokensType.Equal -> ENumOfTokensType.kEqualNumber
  | Dto.NumOfTokensType.GreaterThan -> ENumOfTokensType.kGreaterThanNumber
  | Dto.NumOfTokensType.GreaterThanOrEqual -> ENumOfTokensType.kGreaterThanEqualNumber
  | other -> failwithf "Not a valid NumOfTokensType! %A" other

module StringTokenizerModifier =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IStringTokenizerModifier): Dto.StringTokenizerModifier =
    { Delimiter = domain.Delimiter
      ResultExpression = domain.ResultExpression
      TextInBetween = domain.TextInBetween
      NumberOfTokensType = domain.NumberOfTokensType |> NumOfTokensType.toDto
      NumberOfTokensRequired = domain.NumberOfTokensRequired }

  let fromDto (dto: Dto.StringTokenizerModifier) =
    StringTokenizerModifierClass
      ( Delimiter=dto.Delimiter,
        ResultExpression=dto.ResultExpression,
        TextInBetween=dto.TextInBetween,
        NumberOfTokensType=(dto.NumberOfTokensType |> NumOfTokensType.fromDto),
        NumberOfTokensRequired=dto.NumberOfTokensRequired )

type StringTokenizerModifierConverter() =
  inherit RuleObjectConverter<StringTokenizerModifierClass, IStringTokenizerModifier, Dto.StringTokenizerModifier>()
  override _.toDto _mc domain = domain |> StringTokenizerModifier.toDto
  override _.fromDto _mc dto = dto |> StringTokenizerModifier.fromDto
