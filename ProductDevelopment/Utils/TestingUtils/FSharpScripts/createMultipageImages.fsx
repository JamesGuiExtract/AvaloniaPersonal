#I @"C:\Program Files (x86)\Extract Systems\CommonComponents"
#r "Extract.Utilities.FSharp.dll"

open System
open System.IO
open Extract.Utilities.FSharp

let createMultipageImageExe = @"C:\Program Files (x86)\Extract Systems\CommonComponents\CreateMultipageImage.exe"

// Append lines to text file that describe the pages to be combined for a single output image
let appendToImageCreationList (rng: Random) (singlePageImages: string array) numPages outputImageName imageCreationList =
  let lines =
    [1..numPages]
    |> Seq.map(fun _ ->
      let singlePageImage = singlePageImages.[rng.Next(0, singlePageImages.Length)]
      sprintf "%s;%s" singlePageImage outputImageName
    )
  File.AppendAllLines(imageCreationList, lines)

let createImages singlePageImageFolder minPages maxPages totalPages outputFolder =
  use tmpFile = new TempFile()
  let inputFilePath = tmpFile.FileName
  let args = sprintf """"%s" ; 2 1 "%s" """ inputFilePath outputFolder

  let singlePageImages =
    Directory.GetFiles(singlePageImageFolder, "*.*", SearchOption.AllDirectories)
    |> Array.filter (fun fileName ->
      match Path.GetExtension(fileName).ToLowerInvariant() with
      | ".tif" | ".tiff" -> true
      | _ -> false
    )

  let rng = Random()
  let rec loop pagesUsed =
    if pagesUsed >= totalPages then ()
    else
      let numPages = rng.Next(minPages, maxPages + 1)
      let outputImageName = 
        ( Path.GetRandomFileName()
          |> Seq.filter ((<>) '.')
          |> Seq.toArray
          |> String
        ) + ".tif"
      appendToImageCreationList rng singlePageImages numPages outputImageName inputFilePath
      loop (pagesUsed + numPages)
  loop 0

  printfn "Running CreateMultipageImage.exe to create files in %s" outputFolder
  let exitCode, output, errors = Utils.runProc createMultipageImageExe args None

  if exitCode = 0 then
    output |> Seq.iter (printfn "%s")
  else
    eprintfn "CreatMultipageImage failed with exit code %d. First error: %s" exitCode (errors |> Seq.head)


do
  // USAGE: Supply input folder and output folder, e.g.:
  //  fsi C:\Demo_IDShield\Input D:\tmp\MultipageImages
  // NOTE: The input folder should contain single-page images as only the first page of the files will be used
  let argv : string array = fsi.CommandLineArgs |> Array.tail
  let singlePageImageFolder = argv.[0]
  let outputFolder = argv.[1]

  let lockStdOut = obj()

  [|"Small", 1, 10, 10000
    "Medium", 11, 100, 10000
    "Large", 101, 2000, 10000|]
  |> Array.Parallel.iter (fun (name, minPages, maxPages, totalPages) ->
    lock lockStdOut (fun () ->
      printfn "Creating list for %s: %d-%d page images, totaling %d pages" name minPages maxPages totalPages
    )
    let outputFolderForSet = Directory.CreateDirectory(Path.Combine(outputFolder, name)).FullName
    createImages singlePageImageFolder minPages maxPages totalPages outputFolderForSet
  )
