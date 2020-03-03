namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEMODIFIERSLib

module InsertCharsLengthType =
  let toDto = function
  | EInsertCharsLengthType.kAnyLength -> Dto.InsertCharsLengthType.AnyLength
  | EInsertCharsLengthType.kEqual -> Dto.InsertCharsLengthType.Equal
  | EInsertCharsLengthType.kLessThanEqual -> Dto.InsertCharsLengthType.LessThanEqual
  | EInsertCharsLengthType.kLessThan -> Dto.InsertCharsLengthType.LessThan
  | EInsertCharsLengthType.kGreaterThanEqual -> Dto.InsertCharsLengthType.GreaterThanEqual
  | EInsertCharsLengthType.kGreaterThan -> Dto.InsertCharsLengthType.GreaterThan
  | EInsertCharsLengthType.kNotEqual -> Dto.InsertCharsLengthType.NotEqual
  | other -> failwithf "Not a valid EInsertCharsLengthType! %A" other

  let fromDto = function
  | Dto.InsertCharsLengthType.AnyLength -> EInsertCharsLengthType.kAnyLength
  | Dto.InsertCharsLengthType.Equal -> EInsertCharsLengthType.kEqual
  | Dto.InsertCharsLengthType.LessThanEqual -> EInsertCharsLengthType.kLessThanEqual
  | Dto.InsertCharsLengthType.LessThan -> EInsertCharsLengthType.kLessThan
  | Dto.InsertCharsLengthType.GreaterThanEqual -> EInsertCharsLengthType.kGreaterThanEqual
  | Dto.InsertCharsLengthType.GreaterThan -> EInsertCharsLengthType.kGreaterThan
  | Dto.InsertCharsLengthType.NotEqual -> EInsertCharsLengthType.kNotEqual
  | other -> failwithf "Not a valid InsertCharsLengthType! %A" other

module InsertCharacters =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IInsertCharacters): Dto.InsertCharacters =
    { AppendToEnd = domain.AppendToEnd
      CharsToInsert = domain.CharsToInsert
      InsertAt = domain.InsertAt
      LengthType = domain.LengthType |> InsertCharsLengthType.toDto
      NumOfCharsLong = domain.NumOfCharsLong }

  let fromDto (dto: Dto.InsertCharacters) =
    let domain =
      InsertCharactersClass
        ( AppendToEnd=dto.AppendToEnd,
          CharsToInsert=dto.CharsToInsert,
          LengthType=(dto.LengthType |> InsertCharsLengthType.fromDto) )

    if dto.InsertAt > 0
    then domain.InsertAt <- dto.InsertAt

    if dto.NumOfCharsLong > 0
    then domain.NumOfCharsLong <- dto.NumOfCharsLong

    domain

type InsertCharactersConverter() =
  inherit RuleObjectConverter<InsertCharactersClass, IInsertCharacters, Dto.InsertCharacters>()
  override _.toDto _mc domain = domain |> InsertCharacters.toDto
  override _.fromDto _mc dto = dto |> InsertCharacters.fromDto
