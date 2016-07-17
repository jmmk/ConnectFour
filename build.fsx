#r "packages/FAKE/tools/FakeLib.dll"
#r "packages/Fantomas/lib/FantomasLib.dll"
#r "packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"

open Fake
open Fake.Testing
open Fantomas.FakeHelpers
open Fantomas.FormatConfig
open System

let tempDir = "temp"
let buildDir = tempDir + "/bin"
let testAssemblies = buildDir + "/*Tests*.dll"
let solutionFile = "ConnectFour.sln"

let fantomasConfig = 
    { FormatConfig.Default with PageWidth = 120
                                ReorderOpenDeclaration = true }

Target "Clean" (fun _ -> CleanDirs [ tempDir ])
Target "Build" (fun _ -> 
    !!solutionFile
    |> MSBuildRelease buildDir "Rebuild"
    |> ignore)
Target "Test" (fun _ -> 
    !!testAssemblies |> NUnit3(fun p -> 
                            { p with ShadowCopy = true
                                     TimeOut = TimeSpan.FromMinutes 20.
                                     Labels = LabelsLevel.All }))
Target "Format" (fun _ -> 
    !!"build.fsx" ++ "src/**/*.fs" ++ "tests/**/*.fs"
    |> formatCode fantomasConfig
    |> ignore)
Target "Default" DoNothing
"Clean" ==> "Build"
"Build" ==> "Default"
"Build" ==> "Test"
RunTargetOrDefault "Default"
