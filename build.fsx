// include Fake libs
#r "./packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.NuGetHelper
let dependencies = 
    Fake.Paket.GetDependenciesForReferencesFile "./picdoc/paket.references"
    |> List.ofSeq

// Directories
let buildDir  = "./build/"
let deployDir = "./deploy/"
let releaseNotes = "./RELEASE_NOTES.md"

let release = ReleaseNotesHelper.LoadReleaseNotes releaseNotes

let authors = ["KL"]
let projectName = "picdoc"
let projectDescription = "Generate markdown by extracting EXIF data from images"
let projectSummary = "picdoc"
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
    // compile all projects below src/app/
    MSBuildDebug buildDir "Build" appReferences
    |> Log "AppBuild-Output: "
)

let getNugetParam param =
    {param with
        Authors = authors
        Project = projectName
        Description = projectDescription
        OutputPath = deployDir
        Summary = projectSummary
        WorkingDir = deployDir
        Files = [ ("../build/*", None, None) ]
        Dependencies = dependencies
        ReleaseNotes = System.IO.File.ReadAllText releaseNotes
        Version = version }

let setNugetKey = fun p -> { p with AccessKey = getBuildParam "nugetkey" }

Target "Pack" (fun _ -> NuGetPack getNugetParam "picdoc.nuspec")
Target "Publish" (fun _ -> NuGetPublish (getNugetParam >> setNugetKey)) // publish doesn't work yet

// Build order
"Clean"
  ==> "Build"
  ==> "Pack"
  ==> "Publish"

// start build
//RunTargetOrDefault "Build"
RunTargetOrDefault "Pack"