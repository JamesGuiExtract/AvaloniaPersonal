namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

type InputFinderConverter() =
  inherit RuleObjectConverter<InputFinder, IInputFinder, Dto.InputFinder>()
  override _.toDto _mc _domain = Dto.InputFinder.InputFinder
  override _.fromDto _mc _dto = InputFinder ()
