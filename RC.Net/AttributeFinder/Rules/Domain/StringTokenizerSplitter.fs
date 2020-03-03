namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFSPLITTERSLib
open UCLID_COMUTILSLib

module StringTokenizerSplitType =
  let toDto = function
  | EStringTokenizerSplitType.kEachTokenAsSubAttribute -> Dto.StringTokenizerSplitType.EachTokenAsSubAttribute
  | EStringTokenizerSplitType.kEachTokenAsSpecified -> Dto.StringTokenizerSplitType.EachTokenAsSpecified
  | other -> failwithf "Not a valid EStringTokenizerSplitType! %A" other

  let fromDto = function
  | Dto.StringTokenizerSplitType.EachTokenAsSubAttribute -> EStringTokenizerSplitType.kEachTokenAsSubAttribute
  | Dto.StringTokenizerSplitType.EachTokenAsSpecified -> EStringTokenizerSplitType.kEachTokenAsSpecified
  | other -> failwithf "Not a valid StringTokenizerSplitType! %A" other

module StringTokenizerSplitter =
  open Extract.AttributeFinder.Rules.Dto

  module AttributeFromToken =
    let toDto (domain: IStringPair): Dto.AttributeFromToken =
      { Name = domain.StringKey
        Value = domain.StringValue }

    let fromDto (dto: Dto.AttributeFromToken) =
      StringPairClass
        ( StringKey=dto.Name,
          StringValue=dto.Value )

  let toDto (domain: IStringTokenizerSplitter): Dto.StringTokenizerSplitter =
    { Delimiter = domain.Delimiter |> char
      SplitType = domain.SplitType |> StringTokenizerSplitType.toDto
      FieldNameExpression = domain.FieldNameExpression
      AttributeNameAndValueExprVector = domain.AttributeNameAndValueExprVector |> IUnknownVector.toDto AttributeFromToken.toDto }

  let fromDto (dto: Dto.StringTokenizerSplitter) =
    StringTokenizerSplitterClass
      ( Delimiter=(dto.Delimiter |> int16),
        SplitType = (dto.SplitType |> StringTokenizerSplitType.fromDto),
        FieldNameExpression=dto.FieldNameExpression,
        AttributeNameAndValueExprVector=(dto.AttributeNameAndValueExprVector |> IUnknownVector.fromDto AttributeFromToken.fromDto) )

type StringTokenizerSplitterConverter() =
  inherit RuleObjectConverter<StringTokenizerSplitterClass, IStringTokenizerSplitter, Dto.StringTokenizerSplitter>()
  override _.toDto _mc domain = domain |> StringTokenizerSplitter.toDto
  override _.fromDto _mc dto = dto |> StringTokenizerSplitter.fromDto
