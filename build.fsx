#r "packages/FAKE/tools/FakeLib.dll"
#r "packages/Fantomas/lib/FantomasLib.dll"
#r "packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"

open Fake
open Fake.ProcessHelper
open Fake.FileHelper
open Fake.NpmHelper
open Fantomas.FakeHelpers
open Fantomas.FormatConfig
open System

let srcDir = "src/ConnectFour"
let testDir = "tests/ConnectFour.Tests"
let fantomasConfig = 
    { FormatConfig.Default with PageWidth = 120
                                ReorderOpenDeclaration = true }

Target "Clean" (fun _ -> 
    DeleteFile "app.js"
    CleanDirs [srcDir + "/out"; testDir + "/out"])
Target "Build" (fun _ -> NpmHelper.run { defaultNpmParams with Command = (Run "build"); WorkingDirectory = srcDir })
Target "Watch" (fun _ -> NpmHelper.run { defaultNpmParams with Command = (Run "watch"); WorkingDirectory = srcDir })
Target "BuildTests" (fun _ -> NpmHelper.run { defaultNpmParams with Command = (Run "build"); WorkingDirectory = testDir })
Target "Test" (fun _ -> NpmHelper.run { defaultNpmParams with Command = (Run "test"); WorkingDirectory = testDir })

let repo = "git@github.com:jmmk/ConnectFour.git"
let branch = "gh-pages"
let files = ["app.js"; "app.css"; "index.html"]
Target "Deploy" (fun _ ->
    let tempDir = 
        ExecProcessAndReturnMessages (fun info ->
            info.FileName <- "mktemp"
            info.Arguments <- "-d /tmp/ConnectFour.XXXXX") (TimeSpan.FromSeconds 1.0)
        |> (fun { Messages = messages } -> messages.[0])

    Git.Repository.clone tempDir repo tempDir
    Git.Branches.checkoutBranch tempDir branch
    Git.Repository.fullclean tempDir
    Copy tempDir files
    Git.Staging.StageAll tempDir
    let latestCommit = Git.Information.getCurrentSHA1 "."
    Git.Commit.Commit tempDir (sprintf "Update to commit %s" latestCommit)
    Git.Branches.pushBranch tempDir "origin" branch
    DeleteDir tempDir
    latestCommit |> ignore
)

Target "Format" (fun _ -> 
    !!"build.fsx" ++ "src/**/*.fs" ++ "tests/**/*.fs"
    |> formatCode fantomasConfig
    |> ignore
)

Target "Default" DoNothing

"Clean" 
    ==> "Build"
    ==> "Default"
    ==> "BuildTests"
    ==> "Test"

"Build"
    ==> "Deploy"

RunTargetOrDefault "Default"