namespace Extract.AttributeFinder.Rules.Dto

type StringTokenizerSplitType =
| EachTokenAsSubAttribute = 0
| EachTokenAsSpecified = 1

type AttributeFromToken = {
  Name: string
  Value: string
}

type StringTokenizerSplitter = {
  Delimiter: char
  SplitType: StringTokenizerSplitType
  FieldNameExpression: string
  AttributeNameAndValueExprVector: AttributeFromToken list
}