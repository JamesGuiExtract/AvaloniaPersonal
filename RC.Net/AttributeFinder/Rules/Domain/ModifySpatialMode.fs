namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules

module ModifySpatialModeAction =
  let toDto = function
  | ModifySpatialModeAction.DowngradeToHybrid -> Dto.ModifySpatialModeAction.DowngradeToHybrid
  | ModifySpatialModeAction.ConvertToPseudoSpatial -> Dto.ModifySpatialModeAction.ConvertToPseudoSpatial
  | ModifySpatialModeAction.Remove -> Dto.ModifySpatialModeAction.Remove
  | other -> failwithf "Not a valid ModifySpatialModeAction! %A" other

  let fromDto = function
  | Dto.ModifySpatialModeAction.DowngradeToHybrid -> ModifySpatialModeAction.DowngradeToHybrid
  | Dto.ModifySpatialModeAction.ConvertToPseudoSpatial -> ModifySpatialModeAction.ConvertToPseudoSpatial
  | Dto.ModifySpatialModeAction.Remove -> ModifySpatialModeAction.Remove
  | other -> failwithf "Not a valid ModifySpatialModeAction! %A" other

module ModifySpatialModeRasterZoneCountCondition =
  let toDto = function
  | ModifySpatialModeRasterZoneCountCondition.Single -> Dto.ModifySpatialModeRasterZoneCountCondition.Single
  | ModifySpatialModeRasterZoneCountCondition.Multiple -> Dto.ModifySpatialModeRasterZoneCountCondition.Multiple
  | ModifySpatialModeRasterZoneCountCondition.SinglePage -> Dto.ModifySpatialModeRasterZoneCountCondition.SinglePage
  | other -> failwithf "Not a valid ModifySpatialModeRasterZoneCountCondition! %A" other

  let fromDto = function
  | Dto.ModifySpatialModeRasterZoneCountCondition.Single -> ModifySpatialModeRasterZoneCountCondition.Single
  | Dto.ModifySpatialModeRasterZoneCountCondition.Multiple -> ModifySpatialModeRasterZoneCountCondition.Multiple
  | Dto.ModifySpatialModeRasterZoneCountCondition.SinglePage -> ModifySpatialModeRasterZoneCountCondition.SinglePage
  | other -> failwithf "Not a valid ModifySpatialModeRasterZoneCountCondition! %A" other

module ModifySpatialMode =
  type ModifySpatialModeClass = ModifySpatialMode
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IModifySpatialMode): Dto.ModifySpatialMode =
    { ModifySpatialModeAction = domain.ModifySpatialModeAction |> ModifySpatialModeAction.toDto
      ModifyRecursively = domain.ModifyRecursively
      ZoneCountCondition = domain.ZoneCountCondition |> ModifySpatialModeRasterZoneCountCondition.toDto
      UseCondition = domain.UseCondition }

  let fromDto (dto: Dto.ModifySpatialMode) =
    ModifySpatialModeClass
      ( ModifySpatialModeAction=(dto.ModifySpatialModeAction |> ModifySpatialModeAction.fromDto),
        ModifyRecursively=dto.ModifyRecursively,
        ZoneCountCondition=(dto.ZoneCountCondition |> ModifySpatialModeRasterZoneCountCondition.fromDto),
        UseCondition=dto.UseCondition )

type ModifySpatialModeConverter() =
  inherit RuleObjectConverter<ModifySpatialMode, IModifySpatialMode, Dto.ModifySpatialMode>()
  override _.toDto _mc domain = domain |> ModifySpatialMode.toDto
  override _.fromDto _mc dto = dto |> ModifySpatialMode.fromDto
