// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.NuGetHelper

let dependencies = 
    Fake.Paket.GetDependenciesForReferencesFile "./picdoc/paket.references"
    |> List.ofSeq

// Directories
let buildDir  = "./picdoc/build/"
let deployDir = "./deploy/"
let releaseNotes = "./RELEASE_NOTES.md"

let release = ReleaseNotesHelper.LoadReleaseNotes releaseNotes
let version = release.AssemblyVersion

// Filesets
let appReferences  =
    !! "/**/*.csproj"
    ++ "/**/*.fsproj"

// Targets
Target "Clean" (fun _ ->
    CleanDirs [buildDir; deployDir]
)

Target "Build" (fun _ ->
    MSBuildDebug null "Build" appReferences
    |> Log "AppBuild-Output: "
)

let getNugetParam param =
    {param with
        OutputPath = deployDir
        WorkingDir = deployDir
        Dependencies = dependencies
        ReleaseNotes = System.IO.File.ReadAllText releaseNotes
        Version = version }

Target "Pack" (fun _ -> NuGetPack getNugetParam "./picdoc/picdoc.fsproj") // doesn't properly bundle picdoc.exe and its dependencies

// Build order
"Clean"
  ==> "Build"
  ==> "Pack"

// start build
RunTargetOrDefault "Build"