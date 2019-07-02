namespace Extract.Utilities.FSharp.NERAnnotation
open UCLID_AFCORELib
open UCLID_RASTERANDOCRMGMTLib

type Entity =
  {
      ExpectedValue: string option
      Zones: RasterZone list
      ValueComponents: IAttribute list
      SpatialString: SpatialString option
      Category: string
  }

type EntitiesAndPage =
  {
    Entities: Entity list
    Page: AFDocument
  }

type LabeledToken =
  {
      Token: string
      Label: string
      StartOfEntity: bool
      EndOfEntity: bool
      EndOfSentence: bool
      EndOfPage: bool
  }
