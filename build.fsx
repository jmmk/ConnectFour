#r "packages/FAKE/tools/FakeLib.dll"
#r "packages/Fantomas/lib/FantomasLib.dll"
#r "packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"

open Fake
open Fake.ProcessHelper
open Fake.Testing
open Fantomas.FakeHelpers
open Fantomas.FormatConfig
open System

let srcDir = "src/ConnectFour"
let testDir = "tests/ConnectFour.Tests"
let fantomasConfig = 
    { FormatConfig.Default with PageWidth = 120
                                ReorderOpenDeclaration = true }

Target "Clean" (fun _ -> CleanDirs ["public"; srcDir + "out"; testDir + "out"])
Target "Build" (fun _ -> 
    execProcess (fun info ->
        info.FileName <- "npm"
        info.WorkingDirectory <- srcDir
        info.Arguments <- "run build") (TimeSpan.FromMinutes 5.0)
        |> ignore)

Target "Test" (fun _ -> 
    execProcess (fun info ->
        info.FileName <- "npm"
        info.WorkingDirectory <- testDir
        info.Arguments <- "run test") (TimeSpan.FromMinutes 5.0)
        |> ignore)

Target "Format" (fun _ -> 
    !!"build.fsx" ++ "src/**/*.fs" ++ "tests/**/*.fs"
    |> formatCode fantomasConfig
    |> ignore)

Target "Default" DoNothing

"Clean" 
    ==> "Build"
    ==> "Default"
    ==> "Test"

RunTargetOrDefault "Default"