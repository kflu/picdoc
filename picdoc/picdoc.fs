namespace picdoc

module picdoc =
    #if INTERACTIVE
    #r "../packages/MetadataExtractor/lib/net45/MetadataExtractor.dll"
    #r "../packages/XmpCore/lib/net35/XmpCore.dll"
    #r "../packages/Nustache/lib/net20/Nustache.Core.dll"
    #endif

    open System
    open System.IO
    open Argu
    open MetadataExtractor

    open core
    open core.utils
    open renderer

    type Options =
        | [<MainCommand; ExactlyOnce>] InputDir of path:string
        | [<AltCommandLine("-p")>] FilePattern of pattern:string
        | [<AltCommandLine("-l")>] LinkPrefix of prefix:string
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | InputDir _ -> "input directory"
                | FilePattern _ -> "pattern of files to be included"
                | LinkPrefix _ -> "string to prefix the link to the images"

    [<EntryPoint>]
    let main argv =
        let parser = ArgumentParser.Create<Options>(errorHandler=ProcessExiter())
        let opts = parser.Parse(argv)

        let targetDir = opts.GetResult(<@ InputDir @>) |> Path.GetFullPath
        let filePattern = opts.GetResult(<@ FilePattern @>, defaultValue="*")
        let prefix = opts.GetResult(<@ LinkPrefix @>, defaultValue="")

        let infos = 
            System.IO.Directory.GetFiles(targetDir)
            |> Array.map 
                (fun fn -> { ImageInfo.Default with Path = fn |> Path.GetFullPath; Tags = getTags fn })
            |> Array.map // set the default manipulators
                (Manipulator.setImageUrl prefix
                >> Manipulator.fromExtractor Extractor.Title
                >> Manipulator.fromExtractor Extractor.Description)
        
        let rendered = Markdown.render infos // render

        printfn "%s" rendered

        0 // return an integer exit code
