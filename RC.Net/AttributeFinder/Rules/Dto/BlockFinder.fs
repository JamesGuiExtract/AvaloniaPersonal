namespace Extract.AttributeFinder.Rules.Dto

type DefineBlocksType =
| SeparatorString = 0
| BeginAndEndString = 1

type BlockFinder = {
  BlockBegin: string
  BlockEnd: string
  BlockSeparator: string
  Clues: string list
  DefineBlocksType: DefineBlocksType
  FindAllBlocks: bool
  GetMaxOnly: bool
  InputAsOneBlock: bool
  IsCluePartOfAWord: bool
  IsClueRegularExpression: bool
  MinNumberOfClues: int
  PairBeginAndEnd: bool
}