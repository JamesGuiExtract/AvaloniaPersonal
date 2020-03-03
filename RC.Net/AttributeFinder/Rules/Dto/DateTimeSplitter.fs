namespace Extract.AttributeFinder.Rules.Dto

type DateTimeSplitter = {
  MinimumTwoDigitYear: int
  OutputFormat: string
  ShowFormattedOutput: bool
  SplitDayOfWeek: bool
  SplitDefaults: bool
  SplitFourDigitYear: bool
  SplitMilitaryTime: bool
  SplitMonthAsName: bool
  TwoDigitYearBeforeCurrent: bool
}