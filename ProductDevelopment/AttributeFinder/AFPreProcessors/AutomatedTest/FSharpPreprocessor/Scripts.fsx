#I @"C:\Engineering\Binaries\Debug"
#I @"C:\Program Files (x86)\Extract Systems\CommonComponents"
#r "Extract.AttributeFinder.dll"
#r "Extract.Utilities.Parsers.dll"
#r "Interop.UCLID_AFCORELib.dll"
#r "Interop.UCLID_AFVALUEFINDERSLib.dll"
#r "Interop.UCLID_COMUTILSLib.dll"
#r "Interop.UCLID_RASTERANDOCRMGMTLib.dll"

open System
open System.IO
open UCLID_AFCORELib
open UCLID_AFVALUEFINDERSLib
open UCLID_RASTERANDOCRMGMTLib

module Seq =
  open UCLID_COMUTILSLib
  open System.Collections

  let ofUV<'t> (uv : IIUnknownVector) =
    seq { for i in 0 .. (uv.Size()-1) -> uv.At(i)} |> Seq.cast<'t>

  let ofVOA = ofUV<IAttribute>

  let toGenericList<'t> (x : 't seq) =
    Generic.List<'t>(x)

  let toUV (x : 'a seq when 'a :> obj) =
      let uv = IUnknownVectorClass()
      x |> Seq.iter (fun li -> uv.PushBack li)
      uv

module List =
  let ofUV<'t> = Seq.ofUV >> Seq.toList
  let ofVOA = Seq.ofVOA >> Seq.toList

module Regex =
  open System.Text.RegularExpressions

  let replace pat rep inp =
    Regex.Replace(input=inp, pattern=pat, replacement=rep)

module AFDoc =
  let sortText (doc: AFDocument): AFDocument =
    let lines = doc.Text.GetLines();
    if lines.Size() > 0
    then
      let sorter = SpatiallyCompareStringsClass()
      lines.Sort(sorter)
      doc.Text.CreateFromSpatialStrings(lines, true);
    doc

module CharReplacement =
  let makeDatesGeneric (text: string): string =
    text
    |> Regex.replace "[0-8]" "9"
    |> Regex.replace "-" "/"

type NoCase(value) =
  member val Value = value

  override this.Equals(that) =
    match that with 
    | :? NoCase as other -> StringComparer.InvariantCultureIgnoreCase.Equals(this.Value, other.Value)
    | _ -> false

  override this.GetHashCode() =
    StringComparer.InvariantCultureIgnoreCase.GetHashCode(this.Value)

  interface System.IComparable with
    member this.CompareTo obj =
        let other : NoCase = downcast obj
        StringComparer.InvariantCultureIgnoreCase.Compare(this.Value, other.Value)
  
  override this.ToString() =
    this.Value.ToString()
 

type MaybeBuilder() =
  member this.Bind(m, f) = Option.bind f m
  member this.Return(x) = Some x
  member this.Zero() = None

let maybe = new MaybeBuilder()

module NERAnnotation = 
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

open NERAnnotation

module Names =
  open System.Text.RegularExpressions

  // Use the NoCase wrapper on string to get case-insensitive record comparisons
  // This way a found match can be used to look up the original expected value
  type NameType =
    {
      First: NoCase
      Last: NoCase
      Middle: NoCase option
      Suffix: NoCase option
      Formatted: NoCase option
    }

  // Module to help parse entities into NameTypes
  module Name =
    let personSuffix =
      @"\b(
          Jr
        | Sr
        | II
        | III
        | IV
        | M\.?\x20?D
        | P\.?\x20?A
        | D\.?\x20?O
        | TRE
        | APRN
      )\b"

    let formattedPattern =
      sprintf """(?inxsm)\A\W*
  (?'last'[A-Z]+),\s*
  (?'first'[A-Z]+)
  (\s*(?'middle'[A-Z]+)\.?)?
  (\s*(?'suffix'%s))?
  \W*\z""" personSuffix

    let parseFormatted formatted =
      match formatted with
      | None -> None
      | Some f ->
        let groupToOption (g: Group) =
          if g.Success then Some(g.Value |> NoCase)
          else None

        match Regex.Match(f, formattedPattern) with
        | m when m.Success ->
            Some {
              First = m.Groups.["first"].Value |> NoCase;
              Last = m.Groups.["last"].Value |> NoCase;
              Middle = m.Groups.["middle"] |> groupToOption;
              Suffix = m.Groups.["suffix"] |> groupToOption;
              Formatted = Some(NoCase f);
            }
        | _ -> 
            None


    let parseAttributes (valueComponents: IAttribute list) =
      let lookup =
        valueComponents
        |> Seq.groupBy (fun subattr -> subattr.Name)
        |> dict

      let getAsOption k =
        match lookup.TryGetValue k with
        | found, attrr when found -> 
            let first = (attrr |> Seq.head).Value.String.Trim(' ', '\r', '\n', '\t', '.')
            if String.IsNullOrEmpty first
            then None
            else Some(first |> NoCase)
        | _ -> None

      let optionAsString prefix (s : NoCase option) =
        match s with
        | Some s -> prefix + s.Value
        | None -> ""

      let first = "First" |> getAsOption
      let last = "Last" |> getAsOption
      let middle = "Middle" |> getAsOption;
      let middleInitial =
        maybe {
          let! middle = middle
          return middle.Value.Substring(0, 1) |> NoCase
        }
      let suffix = "Suffix" |> getAsOption;
      match first, last with
      | Some f, Some l ->
        Some {
            First = f;
            Last = l;
            Middle = middleInitial;
            Suffix = suffix;
            Formatted = sprintf "%s, %s%s%s" l.Value f.Value (middleInitial |> optionAsString " ") (suffix |> optionAsString " ")
                        |> NoCase
                        |> Some
          }
      | _ -> None
  // End of Name module

  // Create canonical format of name from its attributes.
  let setExpectedValuesFromNameDefinitions (entitiesAndPage: EntitiesAndPage): EntitiesAndPage =
    let entities =
      entitiesAndPage.Entities
      |> Seq.choose (fun e ->
        maybe {
          let! parsed = Name.parseAttributes e.ValueComponents
          let! formatted = parsed.Formatted
          return { e with ExpectedValue = Some(formatted.Value) }
        }
      )
      |> Seq.toList
    { entitiesAndPage with Entities = entities }

  // Search for the expected value on the supplied page
  let resolveNamesToPage (entitiesAndPage: EntitiesAndPage): EntitiesAndPage =
    let namesToEntities =
      entitiesAndPage.Entities
      |> Seq.choose (fun e ->
        match Name.parseFormatted e.ExpectedValue with
        | Some parsed -> Some(parsed, e)
        | None -> None
      )
      |> Seq.groupBy (fun (parsed, _) -> parsed)
      |> dict

    let distinctNames = namesToEntities.Keys

    // Build patterns to get matches back as split attributes
    let getPatternForName (name: NameType) =
      let wordPattern = "[A-Z]+"
      let spacePattern = @"([,.]?\s+|\s?[,.]\s*)"
      let optWordPattern = sprintf "(%s%s)?" wordPattern spacePattern
      let optWordBeforeSpacePattern = sprintf "(%s)?(?=%s)" wordPattern spacePattern

      let first = Regex.Escape(name.First.Value)
      let last = Regex.Escape(name.Last.Value)
      let suffix = match name.Suffix with
                   | None -> ""
                   | Some s -> sprintf "%s%s" spacePattern (Regex.Escape s.Value)
      match name.Middle with
      | None ->
          sprintf """  (?'Last'%s)
  %s
  (?'Middle'%s)
  (?'First'%s)
  (?'Suffix'%s)"""
                   last spacePattern optWordPattern first suffix

          + sprintf """
|
  (?'First'%s)
  %s
  (?'Middle'%s)
  (?'Last'%s)
  (?'Suffix'%s)"""
                   first spacePattern optWordPattern last suffix
      | Some m ->
          let mi = Regex.Escape(m.Value.Substring(0, 1))
          let middle = Regex.Escape(m.Value)
          if middle = mi
          // Middle name is only an initial, so match that or a full name starting with initial
          then sprintf """  (?'Last'%s)
  %s
  (?'First'%s)
  (
    %s
    (?'Middle'%s
      %s\.?
    )
  )?
  (?'Suffix'%s)"""
                       last
                       spacePattern
                       first
                       spacePattern
                       mi
                       optWordBeforeSpacePattern
                       suffix

               + sprintf """
|
  (?'First'%s)
  %s
  (?'Middle'%s
    %s
    %s
  )?
  (?'Last'%s)
  (?'Suffix'%s)"""
                       first
                       spacePattern
                       mi
                       optWordPattern
                       spacePattern
                       last
                       suffix

          // Else there is a full middle name so match this or just the initial
          else sprintf """  (?'Last'%s)
  %s
  (?'First'%s)
  (
    %s
    (?'Middle'
        %s
      |
        %s
    )
  )?
  (?'Suffix'%s)"""
                       last
                       spacePattern
                       first
                       spacePattern
                       middle
                       mi
                       suffix

               + sprintf """
|
  (?'First'%s)
  %s
  (
    (?'Middle'
        %s
      |
        %s
    )
    %s
  )?
  (?'Last'%s)
  (?'Suffix'%s)"""
                       first
                       spacePattern
                       middle
                       mi
                       spacePattern
                       last
                       suffix

    let namePatterns =
      distinctNames
      |> Seq.map getPatternForName

    let indent amount s =
      s |> Regex.replace "(?m)^" (String(' ', amount))

    let namePattern =
      sprintf """(?inxsm)(?=\b|\W)(
%s
)(?<=\W|\b)""" (String.concat "\r\n|\r\n" namePatterns)// |> indent 2)

#if TEST
    printfn "%s" namePattern
#endif

    let regexRule = RegExprRuleClass()
    regexRule.Pattern <- namePattern;
    regexRule.CreateSubAttributesFromNamedMatches <- true;

    let attributeMatches = regexRule.ParseText(entitiesAndPage.Page, null)

    let newEntities =
      attributeMatches
      |> Seq.ofVOA
      |> Seq.choose (fun attribute ->
        maybe {
          let! parsed = Name.parseAttributes(attribute.SubAttributes |> Seq.ofVOA |> Seq.toList)
          let matches, entities = namesToEntities.TryGetValue parsed
          if matches
          then
            let _, sourceEntity = entities |> Seq.head
            return {
              sourceEntity with
                SpatialString = Some attribute.Value
                Zones = (attribute.Value.GetOCRImageRasterZones() |> Seq.ofUV |> Seq.toList)
            }
        }
      )
      |> Seq.toList

    { entitiesAndPage with Entities = newEntities }

  // Return all entities that match the expected value after processing
  let limitToFinishableNames (entitiesAndPage: EntitiesAndPage): EntitiesAndPage =
    // NOTE: __SOURCE_DIRECTORY__ wouldn't work if script loaded as text
    let rsdName = Path.Combine(__SOURCE_DIRECTORY__, @"..\patientName\nameFilter.rsd.etf");
    let ruleSet = RuleSetClass()
    ruleSet.LoadFrom(rsdName, false) |> ignore

    let finishable =
      entitiesAndPage.Entities
      |> Seq.choose (fun entity ->
        match (entity.SpatialString, entity.ExpectedValue) with
        // Process any entities that have spatial string and expected value
        // (should be all of them)
        | (Some(str), Some(eav)) -> 
            let afdoc = entitiesAndPage.Page
            afdoc.Attribute.SubAttributes <- [AttributeClass ( Name = "_", Value = str )] |> Seq.toUV
            let result = ruleSet.ExecuteRulesOnText(afdoc, null, "", null) |> Seq.ofVOA
            // Process first result
            match result |> Seq.tryHead with
            | None -> None
            | Some(attr) ->
              // Keep if values match
              let foundText = attr.Value.String
              if String.Equals(foundText, eav, StringComparison.OrdinalIgnoreCase)
              then Some(entity)
              else None
        | _ -> None
      )
      |> Seq.toList

    { entitiesAndPage with Entities = finishable }
// End of Names module

module Dates =
  open Extract.Utilities.Parsers

  let makeDatesGeneric (doc: AFDocument): AFDocument =
    let parser = DotNetRegexParser()
    doc.Text.Replace("[0-8]", "9", true, 0, parser)
    doc.Text.Replace("-", "/", true, 0, parser)
    doc

  let preprocessForNERF (doc: AFDocument): AFDocument =
    doc
    |> AFDoc.sortText
    |> makeDatesGeneric

  // Search for the expected value on the supplied page
  let resolveDatesToPage (entitiesAndPage: EntitiesAndPage): EntitiesAndPage =
    let separatorPattern =
      @"(
          / (?'allow_one')
        | [\x28\x29]
        | 1 (?'-allow_one') (?=\d{2}(\d{2})?\D) 
        | 1 (?=\d{2}/)
        | 1 (?=\d?\d/\d{2}\b)(?<=\b\d{2}1) 
        | (?>
              ( (?'-allow_newline')
                [\r\n]{1,4}
              )
            | [\W_-[\r\n]]
          ){1,3}
          (?(verbose_month)
            [\W_]{0,2}
          )
      )"

    let yearPattern year =
      sprintf @"((?'four_digit_year')%d|%d)" year (year % 1000)

    let oneOrTwoDigit number =
      if number > 0 && number < 10
      then sprintf @"0?%d" number
      else sprintf @"%d" number

    let verboseMonthPattern = function
      | 1 -> "(jan | jan(-\r\n)?u(-\r\n)?ar(-\r\n)?y)"
      | 2 -> "(feb | feb(-\r\n)?ru(-\r\n)?ar(-\r\n)?y)"
      | 3 -> "(mar | march)"
      | 4 -> "(apr | a(-\r\n)?pril)"
      | 5 -> "(may)"
      | 6 -> "(jun | june)"
      | 7 -> "(jul | ju(-\r\n)?ly)"
      | 8 -> "(aug | au(-\r\n)?gust)"
      | 9 -> "(sept? | sep(-\r\n)?tem(-\r\n)?ber)"
      | 10 -> "(oct | oc(-\r\n)?to(-\r\n)?ber)"
      | 11 -> "(nov | no(-\r\n)?vem(-\r\n)?ber)"
      | 12 -> "(dec | de(-\r\n)?cem(-\r\n)?ber)"
      | _ -> "(?!)"

    let monthPattern month =
      sprintf @"((?'verbose_month')%s|%s)" (verboseMonthPattern month) (oneOrTwoDigit month)

    let entities = entitiesAndPage.Entities

    let datesToEntities =
      entities
      |> Seq.choose(fun entity ->
        match entity.ExpectedValue with
        | None -> None
        | Some eav  ->
            match DateTime.TryParse eav with
            | (parsed, date) when parsed -> Some(date, entity)
            | _ -> None
      )
      |> Seq.groupBy (fun (date, _entity) -> date)
      |> dict

    let distinctDates =
      datesToEntities
      |> Seq.map (fun g -> g.Key)
      |> Seq.toList

    let datePatterns =
      distinctDates
      |> Seq.map (fun d ->
        // month d, [yy]yy 
        // m/d/[yy]yy
        // m-d-[yy]yy
        (monthPattern d.Month)
        + separatorPattern
        + (oneOrTwoDigit d.Day)
        + separatorPattern
        + (yearPattern d.Year)
        // d month [yy]yy
        // d-month-[yy]yy
        + " | "
        + (oneOrTwoDigit d.Day)
        + separatorPattern
        + (verboseMonthPattern d.Month)
        + separatorPattern
        + (yearPattern d.Year)
        // yyyy month d
        + sprintf " | %d" d.Year
        + separatorPattern
        + (verboseMonthPattern d.Month)
        + separatorPattern
        + (oneOrTwoDigit d.Day)
      )

    let datePattern = sprintf """(?inxsm)(?'allow_newline')\b(%s)\b""" (String.concat "\r\n| " datePatterns)
    let regexRule = RegExprRuleClass()
    regexRule.Pattern <- datePattern;

    let attributeMatches = regexRule.ParseText(entitiesAndPage.Page, null)

    let newEntities =
      attributeMatches
      |> Seq.ofVOA
      |> Seq.choose (fun attribute ->
        match DateTime.TryParse(attribute.Value.String) with
        | (parsed, date) when parsed && datesToEntities.ContainsKey(date) ->
            let _, sourceEntity = datesToEntities.[date] |> Seq.head
            Some({ sourceEntity with Zones = attribute.Value.GetOCRImageRasterZones() |> Seq.ofUV |> Seq.toList })
        | _ -> None
      )
      |> Seq.toList

    { entitiesAndPage with Entities = newEntities }

  // Return all entities that match the expected value after processing
  let limitToFinishableDates (entitiesAndPage: EntitiesAndPage): EntitiesAndPage =
    // NOTE: __SOURCE_DIRECTORY__ wouldn't work if script were loaded as text (e.g., from .etf file)
    let rsdName = Path.Combine(__SOURCE_DIRECTORY__, @"..\Dates\dateFilter.rsd.etf");
    let ruleSet = RuleSetClass()
    ruleSet.LoadFrom(rsdName, false) |> ignore

    let finishable =
      entitiesAndPage.Entities
      |> Seq.choose (fun entity ->
          match (entity.SpatialString, entity.ExpectedValue) with
          | (None, _) -> None
          | (_, None) -> None
          | (Some(str), Some(eav)) -> 
              let afdoc = entitiesAndPage.Page
              afdoc.Attribute.SubAttributes <- [AttributeClass ( Name = "_", Value = str )] |> Seq.toUV
              let result = ruleSet.ExecuteRulesOnText(afdoc, null, "", null) |> Seq.ofVOA
              match result |> Seq.tryHead with
              | Some(attr) ->
                  let foundText = attr.Value.String
                  if String.Equals(foundText, eav, StringComparison.OrdinalIgnoreCase)
                  then Some(entity)
                  else None
              | None -> None
      )
      |> Seq.toList

    { entitiesAndPage with Entities = finishable }
// End of Dates module

(*
  **************************************************************************************************** 
  Helper functions to handle delegating all categories
  ------------------------------------------------------------------------------------------------
*)
let private (|Date|Name|Unknown|) categoryString =
  match categoryString with
  | "DocumentDate" -> Date
  | "DOB" -> Date
  | "PatientName" -> Name
  | "Taxpayer" -> Name
  | _ -> Unknown

let private getByGroup (enp: EntitiesAndPage) =
  enp.Entities
  |> Seq.groupBy (fun entity -> entity.Category)
  |> Seq.map (fun (categoryString, entitiesOfCategory) ->
    categoryString, { enp with Entities = (entitiesOfCategory |> Seq.toList) }
  )

let private processGroups entitiesAndPage (funs: EntitiesAndPage -> string -> EntitiesAndPage option) =
  getByGroup entitiesAndPage
  |> Seq.choose (fun (category, enp) -> funs enp category)
  |> Seq.collect (fun enp -> enp.Entities)
  |> Seq.toList

let private setExpectedValuesToConcatenation (entitiesAndPage: EntitiesAndPage): EntitiesAndPage =
  let format (attributes: IAttribute list) =
    match String.concat "" (attributes |> Seq.map (fun a -> a.Value.String)) with
    | "" -> None
    | s -> Some(s)

  let entities =
    entitiesAndPage.Entities
    |> Seq.choose (fun e ->
      maybe {
        let! concat = format e.ValueComponents
        return { e with ExpectedValue = Some(concat) }
      }
    )
    |> Seq.toList
  { entitiesAndPage with Entities = entities }

(*
**************************************************************************************************** 
  Top-level functions for FindRepeats
  ------------------------------------------------------------------------------------------------
*)

(*
**************************************************************************************************** 
  Create canonical format of name from its attributes.
  ------------------------------------------------------------------------------------------------
*)
let setExpectedValuesFromDefinitions (entitiesAndPage: EntitiesAndPage): EntitiesAndPage =
  let funs enp = function
    | Name -> Some(Names.setExpectedValuesFromNameDefinitions enp)
    | _ -> Some(setExpectedValuesToConcatenation enp)
  let processed = processGroups entitiesAndPage funs
  { entitiesAndPage with Entities = processed }

(*
**************************************************************************************************** 
  Search for the expected value on the supplied page
  ------------------------------------------------------------------------------------------------
*)
let resolveToPage entitiesAndPage =
  let funs enp = function
    | Date -> Some(Dates.resolveDatesToPage enp)
    | Name -> Some(Names.resolveNamesToPage enp)
    | Unknown -> None
  let processed = processGroups entitiesAndPage funs
  { entitiesAndPage with Entities = processed }

(*
**************************************************************************************************** 
  Return all entities that match the expected value after processing
  ------------------------------------------------------------------------------------------------
*)
let limitToFinishable entitiesAndPage =
  let funs enp = function
    | Date -> Some(Dates.limitToFinishableDates enp)
    | Name -> Some(Names.limitToFinishableNames enp)
    | Unknown -> None
  let processed = processGroups entitiesAndPage funs
  { entitiesAndPage with Entities = processed }


// For automated test
let findRepeats (doc: AFDocument) =
    let entities =
      doc.Attribute.SubAttributes
      |> Seq.ofVOA
      |> Seq.map (fun attr ->
          {
            ExpectedValue = None
            Zones = []
            ValueComponents = attr.SubAttributes |> List.ofVOA
            SpatialString = None
            Category = attr.Name
          }
      )
      |> Seq.toList

    let foundEntities =
      { Entities = entities; Page = doc }
      |> setExpectedValuesFromDefinitions
      |> resolveToPage

    let attributesToReturn =
      foundEntities.Entities
      |> Seq.choose (fun entity ->
        match entity.SpatialString with
        | Some value -> Some (AttributeClass(Name = entity.Category, Value = value, Type = "Repeat"))
        | None -> None
      )
      |> Seq.toUV
    
    doc.Attribute.SubAttributes.Append(attributesToReturn)
    doc