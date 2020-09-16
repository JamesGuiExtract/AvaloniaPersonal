module MetadataExtractor

open System
open System.IO
open org.apache.pdfbox.pdmodel
open COSUtils

type Metadata = FileMetadata of string * string | PdfMetadata of string * string

type MetadataExtractor() =

  let getPdfMetadata (pdfPath: string) =
    use document = PDDocument.load(java.io.File pdfPath)
    let info = document.getDocumentInformation()
    info.getMetadataKeys().iterator()
    |> Seq.fromIterator
    |> Seq.map (fun key ->
      let value = info.getCustomMetadataValue key
      PdfMetadata (key, value)
    )

  let addStamp (inputPdfPath: string) (outputPath: string) (stampName: string) (stampValue: string) =
    use document = PDDocument.load(java.io.File inputPdfPath)
    let info = document.getDocumentInformation()
    let dic = info.getCOSObject()
    // let stampName = "eflexStampFiling-105003"
    // let stampValue = DateTime.Now.ToString("yyyy-MM-dd")
    dic.setString(stampName, stampValue)
    let ap = org.apache.pdfbox.pdmodel.encryption.AccessPermission()
    ap.setCanModify(false)
    ap.setCanAssembleDocument(false)
    ap.setCanExtractContent(false)
    ap.setCanFillInForm(false)
    ap.setCanModifyAnnotations(false)
    let spp = org.apache.pdfbox.pdmodel.encryption.StandardProtectionPolicy("a", "", ap)
    spp.setEncryptionKeyLength 128
    document.protect spp
    document.save(java.io.File outputPath)

  let getFileDetails (filePath: string) =
    let results = ref List.empty
    let thread = Threading.Thread(fun () ->
      let shell = Shell32.ShellClass()
      let filePath = Path.GetFullPath filePath
      let dirname = Path.GetDirectoryName filePath
      let filename = Path.GetFileName filePath
      let folder = shell.NameSpace dirname
      let file = folder.ParseName filename
      let rec getDetails acc idx =
        let key = folder.GetDetailsOf(folder.Items, idx)
        if String.IsNullOrWhiteSpace key
        then acc
        else
          let value = folder.GetDetailsOf(file, idx)
          let acc =
            if String.IsNullOrWhiteSpace value
            then acc
            else FileMetadata(key, value)::acc
          getDetails acc (idx + 1)

      results := (getDetails [] 0)
    )
    thread.SetApartmentState(Threading.ApartmentState.STA)
    thread.Start()
    thread.Join()
    results.Value

  member __.GetMetadata(filePath: string) =
    let ext = Path.GetExtension filePath
    seq {
      if ext.ToLowerInvariant() = ".pdf"
      then yield! getPdfMetadata filePath
      yield! getFileDetails filePath
    }

  member __.AddStampToPdf pdfPath stampName stampValue =
    addStamp pdfPath stampName stampValue
