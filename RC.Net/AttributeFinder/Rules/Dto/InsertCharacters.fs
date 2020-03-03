namespace Extract.AttributeFinder.Rules.Dto

type InsertCharsLengthType =
| AnyLength = 0
| Equal = 1
| LessThanEqual = 2
| LessThan = 3
| GreaterThanEqual = 4
| GreaterThan = 5
| NotEqual = 6

type InsertCharacters = {
  AppendToEnd: bool
  CharsToInsert: string
  InsertAt: int
  LengthType: InsertCharsLengthType
  NumOfCharsLong: int
}