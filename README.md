A command line tool to generate markdown doc by extracting metadata from image files.

## Install

Download from [Github Release](https://github.com/kflu/picdoc/releases) (portable executable).

Or with Nuget:

    PM> Install-Package picdoc

## Usage

    $ picdoc --help

    USAGE: picdoc.exe [--help] [--filepattern <pattern>] [--linkprefix <prefix>] <path>

    INPUTDIR:

        <path>                input directory

    OPTIONS:

        --filepattern, -p <pattern>
                            pattern of files to be included
        --linkprefix, -l <prefix>
                            string to prefix the link to the images
        --help                display this list of options.

