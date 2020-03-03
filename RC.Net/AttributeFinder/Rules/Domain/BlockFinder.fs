namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open Extract.Utilities
open UCLID_AFVALUEFINDERSLib

module DefineBlocksType =
  let toDto = function
  | EDefineBlocksType.kSeparatorString -> Dto.DefineBlocksType.SeparatorString
  | EDefineBlocksType.kBeginAndEndString -> Dto.DefineBlocksType.BeginAndEndString
  | other -> failwithf "Not a valid EDefineBlocksType! %A" other

  let fromDto = function
  | Dto.DefineBlocksType.SeparatorString -> EDefineBlocksType.kSeparatorString
  | Dto.DefineBlocksType.BeginAndEndString -> EDefineBlocksType.kBeginAndEndString
  | other -> failwithf "Not a valid DefineBlocksType! %A" other

module BlockFinder =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IBlockFinder): Dto.BlockFinder =
    { BlockBegin = domain.BlockBegin
      BlockEnd = domain.BlockEnd
      BlockSeparator = domain.BlockSeparator
      Clues = domain.Clues |> VariantVector.toList
      DefineBlocksType = domain.DefineBlocksType |> DefineBlocksType.toDto
      FindAllBlocks = domain.FindAllBlocks
      GetMaxOnly = domain.GetMaxOnly
      InputAsOneBlock = domain.InputAsOneBlock
      IsCluePartOfAWord = domain.IsCluePartOfAWord
      IsClueRegularExpression = domain.IsClueRegularExpression
      MinNumberOfClues = domain.MinNumberOfClues
      PairBeginAndEnd = domain.PairBeginAndEnd }

  let fromDto (dto: Dto.BlockFinder) =
    let domain =
      BlockFinderClass
        ( Clues=dto.Clues.ToVariantVector(),
          DefineBlocksType=(dto.DefineBlocksType |> DefineBlocksType.fromDto),
          FindAllBlocks=dto.FindAllBlocks,
          GetMaxOnly=dto.GetMaxOnly,
          InputAsOneBlock=dto.InputAsOneBlock,
          IsCluePartOfAWord=dto.IsCluePartOfAWord,
          IsClueRegularExpression=dto.IsClueRegularExpression,
          PairBeginAndEnd=dto.PairBeginAndEnd )

    if dto.BlockBegin |> String.length > 0
    then domain.BlockBegin <- dto.BlockBegin

    if dto.BlockEnd |> String.length > 0
    then domain.BlockEnd <- dto.BlockEnd

    if dto.BlockSeparator |> String.length > 0
    then domain.BlockSeparator <- dto.BlockSeparator

    if dto.MinNumberOfClues > 0
    then domain.MinNumberOfClues <- dto.MinNumberOfClues

    domain
          

type BlockFinderConverter() =
  inherit RuleObjectConverter<BlockFinderClass, IBlockFinder, Dto.BlockFinder>()
  override _.toDto _mc domain = domain |> BlockFinder.toDto
  override _.fromDto _mc dto = dto |> BlockFinder.fromDto
