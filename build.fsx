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

let release = ReadFile "./RELEASE_NOTES.md" |> ReleaseNotesHelper.parseReleaseNotes

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

Target "CreatePackage" (fun _ ->
    // Copy all the package files into a package folder
    //!! "./build/*" |> CopyFiles deployDir

    NuGetPack (fun p -> 
            {p with
                Authors = authors
                Project = projectName
                Description = projectDescription
                OutputPath = deployDir
                Summary = projectSummary
                WorkingDir = deployDir
                Files = [ ("../build/*", None, None) ]
                Dependencies = dependencies 
                Version = version }) 
                "picdoc.nuspec")

// Build order
"Clean"
  ==> "Build"
  ==> "CreatePackage"

// start build
//RunTargetOrDefault "Build"
RunTargetOrDefault "CreatePackage"