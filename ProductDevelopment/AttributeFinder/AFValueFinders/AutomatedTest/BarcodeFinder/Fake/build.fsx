#r "paket:
nuget Argu
nuget FSharp.Core 4.5.0.0
nuget FSharp.Data
nuget Fake.Core.Target
nuget Fake.DotNet.CLI
nuget Fake.DotNet.MSBuild
nuget Fake.DotNet.NuGet
nuget Fake.IO.FileSystem
nuget Fake.IO.Zip
nuget Fake.Tools.Git
nuget System.Data.SqlClient
//"

#I @"..\..\..\..\..\Utils\TestingUtils\TestingUtils"
#load @"..\..\..\..\..\Utils\TestingUtils\TestingUtils\TestingUtils.fsx"

#if !FAKE
#load "./.fake/build.fsx/intellisense.fsx"
#endif

open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.IO.FileSystemOperators
open Fake.Core
open TestingUtils

open System

// Properties
let serverName = getServerNameOrDefault "(local)"
let projectDir = Path.getFullName (__SOURCE_DIRECTORY__ </> "..")

let projectName = System.IO.Path.GetFileName(projectDir)
let dbName = sprintf "%s_%s" projectName "Memory_Leak"
let actionName = "Test"
let testArea = projectDir </> "TestArea"
let inputFolder = projectDir </> "ImagesWithUSS"
let documentFolder = testArea </> "Source"

let rsds =
  !! (projectDir </> "test*.rsd")
  |> Seq.map(fun path -> sprintf @"<FPSFileDir>\%s" (path |> Path.toRelativeFrom testArea))
  |> Seq.toList

let famPath = testArea </> "MemoryLeak.fps"

// Targets
Target.create "ListTargets" (fun _ ->
  Target.listAvailable()
)

Target.create "SetupTest" (fun _ ->
  Shell.mkdir documentFolder
  FamDB.createDB serverName dbName actionName
  Fam.createFAM (fun defaults -> {
    defaults with
      serverName=serverName
      dbName=dbName
      actionName=actionName
      famPath=famPath
      rulesPaths=rsds
      imageFolders=[@"<FPSFileDir>\Source"]
  })
)

Target.create "RunFam" (fun _ ->
  Fam.runFAM famPath
)

Target.create "CopyFiles" (fun _ ->
  let names = [|"Tamera_Prom"; "Janel_Pero"; "Sharleen_Poling"; "Vernice_Shi"; "Sheron_Hommel"; "Helen_Shelly"; "Tamie_Verdugo"; "Shakira_Petermann"; "Reid_Elson"; "Deloras_Sturrock"; "Buddy_Sitler"; "Tammi_Weedman"; "Bethany_Clayton"; "Greta_Carrion"; "Lavonda_Sutherland"; "Abigail_Harward"; "Bernita_Perrino"; "Shanelle_Grimshaw"; "Wayne_Lightford"; "Jules_Worman"; "Angelina_Mchenry"; "Tera_Mee"; "Lorette_Bausch"; "Lyndsey_Leja"; "Rosio_Fencl"; "Nikki_Weick"; "Tiffiny_Setton"; "Yoshie_Arruda"; "Isis_Docherty"; "Steffanie_Woodson"; "Louis_Waters"; "Jacqulyn_Ignacio"; "Rodger_Momon"; "Jackson_Cravey"; "Alease_Dority"; "Slyvia_Sherrer"; "Terrilyn_Brigmond"; "Lesa_Hiller"; "Salena_Pineda"; "Frederica_Afanador"; "Trisha_Monterrosa"; "Phylis_Spier"; "Samira_Lawver"; "Aurelia_Cedillo"; "Merri_Cuesta"; "Christel_Feely"; "Jerrica_Foltz"; "Mercedes_Guillaume"; "Lecia_Selph"; "Dung_Byington" |]
  let phrases = [|"Quick and dirty"; "Cry wolf"; "All greek to me"; "On cloud nine"; "Read 'em and weep"; "Jumping the gun"; "High and dry"; "Par for the course"; "In the red"; "Tough it out"; "Give a man a fish"; "Not the sharpest tool in the shed"; "Elvis has left the building"; "Jaws of death"; "What am i, chopped liver"; "Money doesn't grow on trees"; "A chip on your shoulder"; "Right off the bat"; "Talk the talk"; "Cup of joe"; "Down to earth"; "Beating a dead horse"; "Plot thickens - the"; "Drawing a blank"; "Knock your socks off"; "Jaws of life"; "Ugly duckling"; "Don't look a gift horse in the mouth"; "Right out of the gate"; "Quality time"; "It's not brain surgery"; "Knuckle down"; "Lickety split"; "Easy as pie"; "Between a rock and a hard place"; "Playing for keeps"; "Under your nose"; "Break the ice"; "Off one's base"; "Shot in the dark"; "Close but no cigar"; "Son of a gun"; "Roll with the punches"; "Foaming at the mouth"; "Happy as a clam"; "Wild goose chase"; "Put a sock in it"; "Curiosity killed the cat"; "In a pickle"; "Know the ropes"; "Scot-free"; "Head over heels"; "Ring any bells"; "Barking up the wrong tree"; "Hear, hear"; "What goes up must come down"; "Every cloud has a silver lining"; "When the rubber hits the road"; "Greased lightning"; "Top drawer"; "Flea market"; "Rain on your parade"; "You can't judge a book by its cover"; "If you can't stand the heat, get out of the kitchen"; "Quick on the draw"; "A piece of cake"; "Ride him, cowboy!"; "Lovey dovey"; "Beating around the bush"; "Cut the mustard"; "Fit as a fiddle"; "Swinging for the fences"; "Playing possum"; "Keep your eyes peeled"; "Keep your shirt on"; "Mouth-watering"; "My cup of tea"; "Go for broke"; "Birds of a feather flock together"; "Drive me nuts"; "Heads up"; "On the same page"; "A fool and his money are soon parted"; "Hit below the belt"; "Yada yada"; "Cut to the chase"; "Throw in the towel"; "Love birds"; "Dropping like flies"; "Under the weather"; "Fool's gold"; "No ifs, ands, or buts"; "Like father like son"; "Jack of all trades master of none"; "Mountain out of a molehill"; "Wake up call"; "Up in arms"; "Eat my hat"; "An arm and a leg"; "Everything but the kitchen sink"; "Needle in a haystack"; "Goody two-shoes"; "Back to square one"; "Short end of the stick"; "No-brainer"; "Two down, one to go"; "Hard pill to swallow"; "Fish out of water"; "Go out on a limb"; "Long in the tooth"; "Down and out"; "Man of few words"; "Wouldn't harm a fly"; "Elephant in the room"; "Hands down"; "It's not all it's cracked up to be"; "Down for the count"; "Poke fun at"; "Tug of war"; "Cry over spilt milk"; "Jig is up"; "There's no i in team"; "Raining cats and dogs"; "Keep on truckin'"; "I smell a rat"; "On the ropes"; "Fight fire with fire"; "A dime a dozen"; "You can't teach an old dog new tricks"; "Down to the wire"; "Let her rip"; "Burst your bubble"; "Back to the drawing board"; "Don't count your chickens before they hatch"|]
  use cancelTokenSource = new System.Threading.CancellationTokenSource()
  cancelTokenSource.CancelAfter(TimeSpan.FromHours 8.0)
  let subFolderCount = ref 0
  let everyFewSeconds = Async.DoPeriodicWork (System.TimeSpan.FromSeconds 30.0) cancelTokenSource.Token
  let rng = Random()
  (fun () ->
    async {
      do subFolderCount := !subFolderCount + 1
      let subFolder = (documentFolder </> (sprintf "%03d" !subFolderCount))
      do Shell.mkdir subFolder
      do !! (inputFolder </> "**/*.tif")
         ++ (inputFolder </> "**/*.pdf")
         |> Seq.iter (fun filename ->
            let randName = names.[rng.Next(names.Length)]
            let randComment = phrases.[rng.Next(phrases.Length)]
            let dest = subFolder </> randName </> randComment
            filename |> Shell.copyFileWithSubfolder inputFolder dest
            let ussFile = filename + ".uss"
            if (Shell.testFile ussFile)
            then ussFile |> Shell.copyFileWithSubfolder inputFolder dest
         )
  })
  |> everyFewSeconds
  |> Async.Start
)

Target.create "LogProcessStats" (fun _ ->
  async {
    do LogProcessStats.run ["ProcessFiles"; "SSOCR2"] 5 (testArea </> "Stats")
  }
  |> Async.Start
)

Target.create "DropDBs" (fun _ ->
  FamDB.dropAllReferencedDBs testArea
)
Target.create "DeleteFiles" (fun _ ->
  Shell.cleanDir testArea
)
Target.create "Clean" ignore
Target.create "RunMemoryLeakTest" ignore

// Dependencies
open Fake.Core.TargetOperators


"BuildTestingUtils" // defined in TestingUtils/Fake/buildUtils.fsx
  ==> "DropDBs"
  ==> "DeleteFiles"
  ==> "Clean"
  ==> "SetupTest"
  ==> "CopyFiles"
  ==> "LogProcessStats"
  ==> "RunFAM"
  ==> "RunMemoryLeakTest"

Target.runOrDefaultWithArguments "ListTargets"
