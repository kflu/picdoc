// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Paket
open System.IO
open Fake.ILMergeHelper
open Fake.FileUtils

let dependencies = 
    Fake.Paket.GetDependenciesForReferencesFile "./picdoc/paket.references"
    |> List.ofSeq

// Directories
let releaseNotes = "./RELEASE_NOTES.md"

let release = ReleaseNotesHelper.LoadReleaseNotes releaseNotes
let version = release.AssemblyVersion

// Filesets
let appReferences  =
    !! "/**/*.csproj"
    ++ "/**/*.fsproj"

let flavor = 
    match (getBuildParamOrDefault "flavor" "Debug").ToLower() with
    | "debug" -> "Debug"
    | "release" -> "Release"
    | _ -> failwith "Flavor not supported"

let buildCmd = 
    if flavor = "Debug" then MSBuildDebug
    else MSBuildRelease

// Targets
Target "Clean" (fun _ ->
    appReferences |> Seq.map (fun r -> Path.Combine(Path.GetDirectoryName r, "bin")) |> CleanDirs
)

Target "Build" (fun _ ->

    // update assembly versions
    let assemblyInfos = !! "**/AssemblyInfo.fs"
    AssemblyInfoHelper.ReplaceAssemblyInfoVersionsBulk assemblyInfos (fun p ->
        { p with AssemblyVersion = version; 
                 AssemblyFileVersion = version; 
                 AssemblyInformationalVersion = version })

    // build
    buildCmd "" "Build" appReferences
    |> Log "AppBuild-Output: "
)

Target "Pack" (fun _ -> 
    Fake.Paket.Pack (fun p -> 
        { p with WorkingDir="./"
                 LockDependencies=true
                 Version=version
                 OutputPath="nugets" }))

Target "Merge" (fun _ ->
    mkdir "picdoc/bin.merged/"
    ILMerge 
        (fun p ->
            { p with TargetKind = TargetKind.Exe;
                     Libraries = (!! "picdoc/bin/*.dll") })
        "picdoc/bin.merged/picdoc.exe"
        "picdoc/bin/picdoc.exe"
)

Target "All" DoNothing

// Build order
"Clean"
  ==> "Build"
  ==> "Merge"
  ==> "Pack"
  ==> "All"
  
// start build
RunTargetOrDefault "Merge"