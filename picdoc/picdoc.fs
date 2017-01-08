module picdoc

#if INTERACTIVE
#r "../packages/MetadataExtractor/lib/net45/MetadataExtractor.dll"
#r "../packages/XmpCore/lib/net35/XmpCore.dll"
#endif

open MetadataExtractor
open Argu

type ImageInfo = {
    Title : string
    Description: string
    Path : string
    Tags : Tag array
}
with 
    static member Default = 
        { Title = ""; Description = ""; Path = ""; Tags = Array.empty<Tag> }

type Options =
    | InputDir of path:string
    | FilePattern of pattern:string
with
    interface IArgParserTemplate with
        member s.Usage = ""

let getTags (fn:string) =
    ImageMetadataReader.ReadMetadata(fn)
    |> Seq.collect (fun (d: Directory) -> d.Tags)
    |> Array.ofSeq

let toLower (x:string) = x.ToLower()

let titles = ["Windows XP Title"] |> List.map toLower

let descriptions = ["Windows XP Comment"] |> List.map toLower

let getField (picks: string list) (tags: Tag seq) : (string option) =
    tags 
    |> Seq.tryFind 
        (fun t -> List.contains (t.Name.ToLower()) picks)
    |> Option.map (fun t -> t.Description)

let extract f info tags = f info tags

let getTitle info = 
    match getField titles (info.Tags) with
    | None -> info
    | Some(v) -> {info with Title = v}

let getDesc info = 
    match getField descriptions (info.Tags) with
    | None -> info
    | Some(v) -> {info with Description = v}

module Markdown =

    // FIXME content needs to be escaped
    let renderOne (info: ImageInfo) =
        sprintf """
**%s**

%s

![%s](%s)""" (info.Title) info.Description info.Title info.Path

    let renderAll (infos: ImageInfo seq) =
        [ for i in infos do
                yield renderOne i 
                yield ""
        ]
        |> String.concat (System.Environment.NewLine)

open System.IO
open System


let GetRelativePath (filespec: string) (folder :string) =
    let mutable folderUri = new Uri(filespec);
    // Folders must end in a slash
    let mutable folder = folder
    if not (folder.EndsWith(Path.DirectorySeparatorChar.ToString())) then 
        folder <- folder + Path.DirectorySeparatorChar.ToString()
    else folderUri <- new Uri(folder)
    Uri.UnescapeDataString(folderUri.MakeRelativeUri(folderUri).ToString().Replace('/', Path.DirectorySeparatorChar))

[<EntryPoint>]
let main argv =
    let parser = ArgumentParser.Create<Options>(errorHandler=ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red))
    let opts = parser.Parse(argv)

    let relativeTo = System.Environment.CurrentDirectory
    let targetDir = opts.GetResult(<@ InputDir @>) |> Path.GetFullPath
    let filePattern = opts.GetResult(<@ FilePattern @>)
    printfn "in: %s pat: %s" targetDir filePattern
    let rendered = 
        System.IO.Directory.GetFiles(targetDir, filePattern)
        |> Array.map 
            (fun fn -> 
                { ImageInfo.Default with 
                    Path = fn // FIXME: this doesn't work: GetRelativePath (Path.GetFullPath(fn)) targetDir
                    Tags = getTags fn })
        |> Array.map (getTitle >> getDesc)
        |> Markdown.renderAll

    printfn "%s" rendered

    0 // return an integer exit code
