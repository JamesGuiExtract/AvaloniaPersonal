namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

module DataEntryPreloaderConverter =
  let toDto (domain: IDataEntryPreloader): Dto.DataEntryPreloader =
    { ConfigFileName = domain.ConfigFileName }

  let fromDto (dto: Dto.DataEntryPreloader) =
    new DataEntryPreloader
      ( ConfigFileName=dto.ConfigFileName )

type DataEntryPreloaderConverter() =
  inherit RuleObjectConverter<DataEntryPreloader, IDataEntryPreloader, Dto.DataEntryPreloader>()
  override _.toDto _mc domain = domain |> DataEntryPreloaderConverter.toDto
  override _.fromDto _mc dto = dto |> DataEntryPreloaderConverter.fromDto
