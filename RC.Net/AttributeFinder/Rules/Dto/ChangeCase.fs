namespace Extract.AttributeFinder.Rules.Dto

type ChangeCaseType =
| NoChangeCase = 0
| MakeUpperCase = 1
| MakeLowerCase = 2
| MakeTitleCase = 3

type ChangeCase = {
  CaseType: ChangeCaseType
}