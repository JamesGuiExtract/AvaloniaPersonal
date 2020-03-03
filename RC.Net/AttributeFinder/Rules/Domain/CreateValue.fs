namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFVALUEFINDERSLib

module CreateValue =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: ICreateValue): Dto.CreateValue =
    { ValueString = domain.ValueString
      TypeString = domain.TypeString }

  let fromDto (dto: Dto.CreateValue) =
    CreateValueClass
      ( ValueString=dto.ValueString,
        TypeString=dto.TypeString )

type CreateValueConverter() =
  inherit RuleObjectConverter<CreateValueClass, ICreateValue, Dto.CreateValue>()
  override _.toDto _mc domain = domain |> CreateValue.toDto
  override _.fromDto _mc dto = dto |> CreateValue.fromDto
