namespace Extract.AttributeFinder.Rules.Domain

open System
open Extract

type IRuleObjectConverter =
  abstract member convertsDto: Type with get
  abstract member convertsDomain: Type with get
  abstract member toDto: IMasterRuleObjectConverter -> obj -> obj
  abstract member fromDto: IMasterRuleObjectConverter -> obj -> obj

[<AbstractClass>]
type RuleObjectConverter<'DomainClass, 'DomainInterface, 'Dto>() =
  abstract member toDto: IMasterRuleObjectConverter -> 'DomainInterface -> 'Dto
  abstract member fromDto: IMasterRuleObjectConverter -> 'Dto -> 'DomainClass
  interface IRuleObjectConverter with
    member _.convertsDto = typeof<'Dto>
    member _.convertsDomain = typeof<'DomainClass>
    member this.toDto mc domain =
      try downcast domain |> this.toDto mc |> box
      with
      | e ->
        let uex = ExtractException.AsExtractException("ELI49673", e)
        uex.AddDebugData ("Converter domain interface type", typeof<'DomainInterface>.FullName)
        uex.AddDebugData ("Converter dto type", typeof<'Dto>.FullName)
        raise uex
    member this.fromDto mc dto = downcast dto |> this.fromDto mc |> box
