#r "packages/FAKE/tools/FakeLib.dll"
#r "packages/FantomasCLI/lib/FantomasLib.dll"

open Fake
open Fake.FileHelper
open Fake.ProcessHelper
open Fantomas.FakeHelpers
open Fantomas.FormatConfig
open System

let cwd = getBuildParam "cwd"
let srcDir = "src/ConnectFour"
let testDir = "tests/ConnectFour.Tests"

let projFile = srcDir @@ "ConnectFour.fsproj"

let testProjFile = testDir @@ "ConnectFour.Tests.fsproj"

let fantomasConfig = 
    { FormatConfig.Default with PageWidth = 120
                                ReorderOpenDeclaration = true }

let npmBinDir = 
    ExecProcessAndReturnMessages (fun info -> 
        info.FileName <- "yarn"
        info.Arguments <- "bin") (TimeSpan.FromSeconds 1.0)
    |> (fun result -> result.Messages.[0])

let npmBin cmd = npmBinDir @@ cmd
let fable = npmBin "fable"
let webpack = npmBin "webpack"
let webpackDevServer = npmBin "webpack-dev-server"
let mocha = npmBin "mocha"

let checkExitCode code = 
    if code <> 0 then failwith "Process returned with a non-zero exit code"

Target "Clean" (fun _ -> CleanDirs [ "fable-out"; "fable-test-out"; "dist" ])
Target "Fable" 
    (fun _ -> 
    Shell.Exec(fable, sprintf "%s --sourceMaps true -m commonjs -o ./fable-out" projFile, cwd) |> checkExitCode)
Target "Webpack" (fun _ -> 
    Environment.SetEnvironmentVariable("NODE_ENV", "production")
    Shell.Exec(webpack) |> checkExitCode)
Target "Watch" (fun _ -> 
    let fableWatch = 
        Shell.AsyncExec(fable, sprintf "%s --sourceMaps true -m commonjs -o ./fable-out -w" projFile, cwd)
    let webpackServer = Shell.AsyncExec(webpackDevServer, "--inline --hot --port 8080 --open")
    [ fableWatch; webpackServer ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore)
Target "FableTest" 
    (fun _ -> 
    Shell.Exec
        (fable, 
         sprintf "%s %s -m commonjs -o ./fable-test-out --plugins ./node_modules/fable-plugins-nunit/Fable.Plugins.NUnit.dll" projFile testProjFile, 
         cwd) |> checkExitCode)
Target "Test" (fun _ -> Shell.Exec(mocha, "./fable-test-out/ConnectFour.Tests", cwd) |> ignore)

let repo = "git@github.com:jmmk/ConnectFour.git"
let branch = "gh-pages"

Target "Deploy" (fun _ -> 
    let tempDir = 
        ExecProcessAndReturnMessages (fun info -> 
            info.FileName <- "mktemp"
            info.Arguments <- "-d /tmp/ConnectFour.XXXXX") (TimeSpan.FromSeconds 1.0)
        |> (fun result -> result.Messages.[0])
    Git.Repository.clone tempDir repo tempDir
    Git.Branches.checkoutBranch tempDir branch
    Git.Repository.fullclean tempDir
    CopyRecursive "dist" tempDir true |> ignore
    Git.Staging.StageAll tempDir
    let latestCommit = Git.Information.getCurrentSHA1 "."
    Git.Commit.Commit tempDir (sprintf "Update to commit %s" latestCommit)
    Git.Branches.pushBranch tempDir "origin" branch
    DeleteDir tempDir
    latestCommit |> ignore)
Target "Format" (fun _ -> 
    !!"build.fsx" ++ "src/**/*.fs" ++ "tests/**/*.fs"
    |> formatCode fantomasConfig
    |> ignore)
Target "Build" DoNothing
"Clean" ==> "FableTest" ==> "Test"
"Clean" ==> "Watch"
"Fable" ==> "Webpack" ==> "Build" ==> "Deploy"
RunTargetOrDefault "Watch"
