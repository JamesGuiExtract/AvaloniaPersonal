namespace Extract.AttributeFinder.Rules.Dto

type ModifySpatialModeAction =
| DowngradeToHybrid = 0
| ConvertToPseudoSpatial = 1
| Remove = 2

type ModifySpatialModeRasterZoneCountCondition =
| Single = 0
| Multiple = 1
| SinglePage = 2

type ModifySpatialMode = {
  ModifySpatialModeAction: ModifySpatialModeAction
  ModifyRecursively: bool
  ZoneCountCondition: ModifySpatialModeRasterZoneCountCondition
  UseCondition: bool
}