module core
open MetadataExtractor

type ImageInfo = {
    Path : string
    Tags : Tag array
    Props : Map<string, string>
}
with 
    static member Default = 
        { Path = ""; Tags = Array.empty<Tag>; Props = Map.empty<string,string> }

type extractor = Tag seq -> (string * (string option))
type manipulator = ImageInfo -> ImageInfo

module utils =
    let getTags (fn:string) =
        ImageMetadataReader.ReadMetadata(fn)
        |> Seq.collect (fun (d: Directory) -> d.Tags)
        |> Array.ofSeq

    let eqi x y = System.String.Equals(x, y, System.StringComparison.CurrentCultureIgnoreCase)

open utils
module Extractor = 
    /// From tags get the value from the first occurrence of name that's in the fields
    let fromOneOfFields (name, oneOfFields: string seq) : extractor =
        fun tags ->
            tags 
            |> Seq.tryFind (fun t -> oneOfFields |> Seq.tryFind (fun name -> eqi name (t.Name)) |> Option.isSome) // TODO: this could be simplified
            |> (function
                | Some(t) -> name, t.Description |> Some
                | _ -> name, None)
    
    let Title : extractor = fromOneOfFields ("title", [ "Windows XP Title" ])
    let Description : extractor = fromOneOfFields ("description", [ "Windows XP Comment" ])

module Manipulator =
    let fromExtractor (extract: extractor) : manipulator =
        fun info ->
            match extract info.Tags with
            | (name, Some(v)) -> { info with Props = info.Props |> Map.add name v }
            | _ -> info

    let setImageUrl (prefix: string) : manipulator =
        fun info -> 
            let prefix = 
                if System.String.IsNullOrEmpty prefix then "" 
                else
                    if prefix.EndsWith("/") then prefix
                    else prefix + "/"
            
            let fn = System.IO.Path.GetFileName(info.Path)
            { info with 
                Props = info.Props |> Map.add "path" (sprintf "%s%s" prefix fn) }