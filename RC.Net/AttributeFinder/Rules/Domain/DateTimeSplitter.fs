namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFSPLITTERSLib

module DateTimeSplitter =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IDateTimeSplitter): Dto.DateTimeSplitter =
    { MinimumTwoDigitYear = domain.MinimumTwoDigitYear
      OutputFormat = domain.OutputFormat
      ShowFormattedOutput = domain.ShowFormattedOutput
      SplitDayOfWeek = domain.SplitDayOfWeek
      SplitDefaults = domain.SplitDefaults
      SplitFourDigitYear = domain.SplitFourDigitYear
      SplitMilitaryTime = domain.SplitMilitaryTime
      SplitMonthAsName = domain.SplitMonthAsName
      TwoDigitYearBeforeCurrent = domain.TwoDigitYearBeforeCurrent }

  let fromDto (dto: Dto.DateTimeSplitter) =
    DateTimeSplitterClass
      ( MinimumTwoDigitYear=dto.MinimumTwoDigitYear,
        OutputFormat=dto.OutputFormat,
        ShowFormattedOutput=dto.ShowFormattedOutput,
        SplitDayOfWeek=dto.SplitDayOfWeek,
        SplitDefaults=dto.SplitDefaults,
        SplitFourDigitYear=dto.SplitFourDigitYear,
        SplitMilitaryTime=dto.SplitMilitaryTime,
        SplitMonthAsName=dto.SplitMonthAsName,
        TwoDigitYearBeforeCurrent=dto.TwoDigitYearBeforeCurrent )

type DateTimeSplitterConverter() =
  inherit RuleObjectConverter<DateTimeSplitterClass, IDateTimeSplitter, Dto.DateTimeSplitter>()
  override _.toDto _mc domain = domain |> DateTimeSplitter.toDto
  override _.fromDto _mc dto = dto |> DateTimeSplitter.fromDto
