The configuration file: 'docnet.json'
=====================================

DocNet uses a json file to determine what to do in what form. The format is straight forward and described below

```json
{
    "Name" : "name of site",
    "Source" : "sourcefolder",
    "Destination" : "destinationfolder",
	"IncludeSource": "includefolder",
    "Theme" : "themefolder",
    "SourceFoldersToCopy" : ["folder1", "foldern"],
    "Footer" : "footer text or HTML",
	"ConvertLocalLinks: "true" | "false",
    "Pages" : 
    {
        "__index" : "index.md",
        "Title Page 1": "filename_page1.md", 
        "Title Page 2": "filename_page2.md",
        "Sub Header 1": 
        {
            "__index" : "index.md",
            "Title Page 3" : "filename_page3.md",
            "Title Page 4" : "filename_page4.md"
        },
        "Auto generated" : "reference/**"
    }
}
```

The order in which the pages are specified is the order in which they'll appear in the navigation. 

* `Name` specifies the name of the site to generate. 
* `Source` specifies the root of the tree of folders in which markdown files are expected. `sourcefolder` can be a relative path or an absolute path and is used as the root folder for filenames specified in `Pages`. 
* `Destination` specifies the folder where the output will be written to (.htm files). The output will be written to this folder only, all files from subfolders in `Source` will be written to the folder specified in `Destination`. with the same structure as the *navigation* in `Pages`. `destinationfolder` can be a relative path or an absolute path.
* `IncludeSource` specifies the folder where the files specified to be included using [@@include directives](markdownextensions.htm#include-files) are located. If `IncludeSource` isn't specified, the value `Includes` is assumed. 
* `Theme` specifies the folder within the `Themes` folder in the folder the `docnet` executable is located which is used as the theme for the pages to generate. `Docnet` expects a file called `PageTemplate.htm` within the specified `Theme` folder, which contains the HTML which is used as the wrapper file for the HTML generated from the markdown. It has to contain a couple of marker, which are described later in this document. If `Theme` isn't specified, `Default` is assumed.
* `SourceFoldersToCopy`. This is an optional directive with, if specified, one or more folder names relative to `Source`, which contain files to copy to the `Destination` folder. E.g. image files used in the markdown files, located in an `Images` folder can be copied this way to the output folder reliably. All folders specified are copied recursively.
* `ConvertLocalLinks`. This is an optional directive which, if specified and set to `"true"`, will make DocNet convert all local links to `.md` suffixed files into `.htm` files. Example: `(local link)[somemarkdownfile.md]` will be converted to `<a href="somemarkdownfile.htm">local link</a>`. Non-local urls are not converted. Default: `"false"`. 
* `Footer`. This is text and/or HTML which is placed in the footer of each page, using a _marker_ (see below).
* `Pages` contains the pages to generate into the output, in the order and structure in which you want them to appear in the navigation. The name given is the value used in the navigation tree and has to be unique per level. The value specified with each name is the markdown file to load, parse and generate as .htm file in the output. The markdown file is relative to the path specified in `Source`. A file `foo.md` will be generated as `foo.htm` in the output. 

Paths are expected to use `\` characters, and as it's json, you have to escape them, so the path `.\foo` becomes `.\\foo`.

Each level, starting with the root, has a special page defined, `__index`. This page is the page shown when the level header (in the example above this is e.g. _Sub Header 1_) is clicked in the navigation. If `__index` is specified in the root level, it's assigned to the navigation name `Home`. If there's no `__index` specified, there will still be a page generated but it will have default content (See below). The names of the elements are case sensitive.

## Levels without __index defined
If a level has no `__index` defined, `DocNet` will create a `__index` entry for the level and will specify as target `<path to index of parent>/nameoflevel.md`. If the page exists it will be loaded as the content for the index of the level, if it doesn't exist, the HTML will simply contain the topictitle and a list of all the sub topics in the level. This guarantees the tree can always be navigated in full.  

## Use of wildcard inclusions
If a level has a string value ending with `**`, it will process all .md files in the folders and subfolders and generate htm files from them. It will use the following process:

1. Search (recursively) for all markdown (`.md`) files inside the specified folder (e.g. `reference`)
2. Generate index (htm) files for all folders
3. The title of the markdown files will be retrieved from the actual markdown files (so the first non-empty line will be used as title)

@alert info
The wildcard inclusion feature is extremely useful for referene or API documentation (mostly generated by a 3rd party tool)
@end 