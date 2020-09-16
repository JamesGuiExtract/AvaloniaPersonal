module Preprocessor

#if NO_MKLINK_SLASH_J
#I @"C:\Engineering\Binaries\Debug"
#I @"C:\Engineering\Binaries\Release"
#endif

#if !FUNCTION_LOADER
#I @"C:\Program Files (x86)\Extract Systems\CommonComponents"
#I @"R:\Rules\ComponentData"
#endif

#r "IKVM.OpenJDK.Core.dll"
#r "IKVM.OpenJDK.SwingAWT.dll"
#r "Tabula.IKVM.exe"
#r "Interop.UCLID_AFCORELib.dll"
#r "Interop.UCLID_AFSPLITTERSLib.dll"
#r "Interop.UCLID_AFUTILSLib.dll"
#r "Interop.UCLID_AFVALUEFINDERSLib.dll"
#r "Interop.UCLID_COMUTILSLib.dll"
#r "Interop.UCLID_EXCEPTIONMGMTLib.dll"
#r "Interop.UCLID_IMAGEUTILSLib.dll"
#r "Interop.UCLID_RASTERANDOCRMGMTLib.dll"
#r "Newtonsoft.Json.dll"
#r "Interop.Shell32.dll"

#load @"ReusableComponents\FSX\Utils.fs"
#load @"ReusableComponents\FSX\AFUtils.fs"
#load @"ReusableComponents\FSX\COSUtils.fs"
#load "MetadataExtractor.fs"

open System
open MetadataExtractor
open UCLID_AFCORELib
open Utils

[<AutoOpen>]
module Utils =
  let makeSafe = Regex.replace """\W+""" "_"
  let createAttribute sdn name value typ =
    let a = AttributeClass(Name = (makeSafe name), Type = typ)
    a.Value.CreateNonSpatialString (value, sdn)
    a

let getMetadata (doc: AFDocument): AFDocument =
  let filename = doc.Text.SourceDocName

  if String.IsNullOrWhiteSpace doc.Text.String
  then doc.Text.CreateNonSpatialString (" ", filename)

  let extractor = MetadataExtractor()
  let metadata = extractor.GetMetadata filename

  let addAttribute =
    let subattrr = doc.Attribute.SubAttributes
    let createAttribute = createAttribute filename
    fun name value typ -> subattrr.PushBack (createAttribute name value typ)

  metadata
  |> Seq.iter (function
    | PdfMetadata (key, value) -> addAttribute key value "PDF"
    | FileMetadata (key, value) -> addAttribute key value "Shell"
  )

  doc
