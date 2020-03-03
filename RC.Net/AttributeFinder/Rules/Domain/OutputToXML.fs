namespace Extract.AttributeFinder.Rules.Domain

open Extract.AttributeFinder.Rules
open UCLID_AFOUTPUTHANDLERSLib

module XMLOutputFormat =
  let toDto = function
  | EXMLOutputFormat.kXMLOriginal -> Dto.XMLOutputFormat.XMLOriginal
  | EXMLOutputFormat.kXMLSchema -> Dto.XMLOutputFormat.XMLSchema
  | other -> failwithf "Not a valid EXMLOutputFormat! %A" other

  let fromDto = function
  | Dto.XMLOutputFormat.XMLOriginal -> EXMLOutputFormat.kXMLOriginal
  | Dto.XMLOutputFormat.XMLSchema -> EXMLOutputFormat.kXMLSchema
  | other -> failwithf "Not a valid XMLOutputFormat! %A" other

module OutputToXML =
  open Extract.AttributeFinder.Rules.Dto

  let toDto (domain: IOutputToXML): Dto.OutputToXML =
    { FileName = domain.FileName
      Format = domain.Format |> XMLOutputFormat.toDto
      NamedAttributes = domain.NamedAttributes
      UseSchemaName = domain.UseSchemaName
      SchemaName = domain.SchemaName
      ValueAsFullText = domain.ValueAsFullText
      RemoveEmptyNodes = domain.RemoveEmptyNodes
      RemoveSpatialInfo = domain.RemoveSpatialInfo }

  let fromDto (dto: Dto.OutputToXML) =
    OutputToXMLClass
      ( FileName=dto.FileName,
        Format=(dto.Format |> XMLOutputFormat.fromDto),
        NamedAttributes=dto.NamedAttributes,
        UseSchemaName=dto.UseSchemaName,
        SchemaName=dto.SchemaName,
        ValueAsFullText=dto.ValueAsFullText,
        RemoveEmptyNodes=dto.RemoveEmptyNodes,
        RemoveSpatialInfo=dto.RemoveSpatialInfo )

type OutputToXMLConverter() =
  inherit RuleObjectConverter<OutputToXMLClass, IOutputToXML, Dto.OutputToXML>()
  override _.toDto _mc domain = domain |> OutputToXML.toDto
  override _.fromDto _mc dto = dto |> OutputToXML.fromDto
