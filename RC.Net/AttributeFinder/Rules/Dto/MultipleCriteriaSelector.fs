namespace Extract.AttributeFinder.Rules.Dto

type CombinationType =
| Chain = 0
| Union = 1

type SelectType =
| Matching = 0
| NonMatching = 1

type Selector = {
  Select: SelectType  
  With: ObjectWithDescription 
}

type MultipleCriteriaSelector = {
  Selectors: Selector list
  CombineBy: CombinationType
}