namespace picdoc

module picdoc =
    #if INTERACTIVE
    #r "../packages/MetadataExtractor/lib/net45/MetadataExtractor.dll"
    #r "../packages/XmpCore/lib/net35/XmpCore.dll"
    #r "../packages/Nustache/lib/net20/Nustache.Core.dll"
    #r "../packages/Argu/lib/net40/Argu.dll"
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
        | Verbose
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | InputDir _ -> "input directory"
                | FilePattern _ -> "pattern of files to be included"
                | LinkPrefix _ -> "string to prefix the link to the images"
                | Verbose _ -> "Print out debug information"

    [<EntryPoint>]
    let main argv =
        if argv |> Array.contains "--pause-on-start" then 
            printfn "Press any key to continue..."
            System.Console.Read() |> ignore
        
        let argv = argv |> Seq.filter (fun x -> x <> "--pause-on-start") |> Array.ofSeq
        let parser = ArgumentParser.Create<Options>(errorHandler=ProcessExiter())
        let opts = parser.Parse(argv)

        let targetDir = opts.GetResult(<@ InputDir @>)
        let targetDir =
            try opts.GetResult(<@ InputDir @>) |> Path.GetFullPath with
            | _ -> 
                printf "Invalid input directory: %s" targetDir
                exit -1

        let filePattern = opts.GetResult(<@ FilePattern @>, defaultValue="*")
        let prefix = opts.GetResult(<@ LinkPrefix @>, defaultValue="")
        let debug = opts.Contains(<@ Verbose @>)
        
        let infos = 
            System.IO.Directory.GetFiles(targetDir, filePattern)
            |> Array.choose (fun fn -> 
                let tags = getTags fn
                match tags with 
                | None -> None
                | Some tags -> 
                    { ImageInfo.Default with Path = fn |> Path.GetFullPath; Tags = tags } |> Some)
            |> Array.map // set the default manipulators
                (Manipulator.setImageUrl prefix
                >> Manipulator.fromExtractor Extractor.Title
                >> Manipulator.fromExtractor Extractor.Description)
        
        if debug then 
            printfn "%A" infos
        
        let rendered = Markdown.render infos // render

        printfn "%s" rendered

        0 // return an integer exit code
