#r "packages/FAKE/tools/FakeLib.dll"
#r "packages/FantomasCLI/lib/FantomasLib.dll"

open Fake
open Fake.FileHelper
open Fake.ProcessHelper
open Fantomas.FakeHelpers
open Fantomas.FormatConfig
open System

let cwd = getBuildParam "cwd"
let srcDir = cwd @@ "src/ConnectFour"
let testDir = cwd @@ "tests/ConnectFour.Tests"
let buildDir = cwd @@ "dist"
let staticDir = cwd @@ "static"
let fableOut = cwd @@ "fable-out"
let fableTestOut = cwd @@ "fable-test-out"
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
let execCwd cmd args =
    Shell.Exec(cmd, args, cwd) |> checkExitCode

Target "Clean" (fun _ -> CleanDirs [ fableOut; fableTestOut; buildDir ])

let fableArgs = sprintf "%s --sourceMaps true -o %s" projFile fableOut
Target "Fable" (fun _ -> 
    execCwd fable fableArgs)

Target "Webpack" (fun _ ->
    Environment.SetEnvironmentVariable("NODE_ENV", "production")
    execCwd webpack "")

Target "Watch" (fun _ -> 
    let fableWatch = 
        Shell.AsyncExec(fable, sprintf "%s --watch" fableArgs, cwd)
    let webpackServer = Shell.AsyncExec(webpackDevServer, "--inline --hot --port 8080 --open")
    [ fableWatch; webpackServer ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore)

Target "FableTest" (fun _ -> 
    execCwd fable (sprintf "%s %s -m commonjs -o %s --plugins ./node_modules/fable-plugins-nunit/Fable.Plugins.NUnit.dll" projFile testProjFile fableTestOut))

Target "Test" (fun _ -> 
    execCwd mocha (fableTestOut @@ "ConnectFour.Tests"))

let repo = "git@github.com:jmmk/ConnectFour.git"
let deployBranch = "gh-pages"

Target "Deploy" (fun _ -> 
    let tempDir = 
        ExecProcessAndReturnMessages (fun info -> 
            info.FileName <- "mktemp"
            info.Arguments <- "-d /tmp/ConnectFour.XXXXX") (TimeSpan.FromSeconds 1.0)
        |> (fun result -> result.Messages.[0])
    Git.Repository.clone tempDir repo tempDir
    Git.Branches.checkoutBranch tempDir deployBranch
    Git.Repository.fullclean tempDir
    CopyRecursive buildDir tempDir true |> ignore
    Git.Staging.StageAll tempDir
    let latestCommit = Git.Information.getCurrentSHA1 "."
    Git.Commit.Commit tempDir (sprintf "Update to commit %s" latestCommit)
    Git.Branches.pushBranch tempDir "origin" deployBranch
    DeleteDir tempDir
    latestCommit |> ignore)

Target "Format" (fun _ -> 
    !!"build.fsx" ++ "src/**/*.fs" ++ "tests/**/*.fs"
    |> formatCode fantomasConfig
    |> ignore)

Target "Build" DoNothing

"Clean" ==> "FableTest" ==> "Test"
"Clean" ==> "Fable" ==> "Watch"
"Clean" ==> "Fable" ==> "Webpack" ==> "Build" ==> "Deploy"
RunTargetOrDefault "Watch"