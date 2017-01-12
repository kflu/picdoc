namespace picdoc

module renderer = 

    open Nustache.Core
    open core

    module Markdown =

        let template = """
{{#images}}
![{{& title}}]({{& path}})

{{#title}}
**{{& title}}**

{{/title}}
{{#description}}
{{& description}}

{{/description}}
{{/images}}
    """
        let createData (infos: ImageInfo seq) =
            Map.ofList
                [ "images", infos |> Seq.map (fun info -> info.Props) |> Array.ofSeq ]

        let render (infos: ImageInfo seq) =
            Nustache.Core.Render.StringToString(template, createData infos)
