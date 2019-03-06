The configuration file: 'docnet.json'
=====================================

DocNet uses a json file to determine what to do in what form. The format is straight forward and described below

```json
{
    "Name" : "name of site",
    "Source" : "sourcefolder",
    "Destination" : "destinationfolder",
    "IncludeSource": "includefolder",
    "Theme" : "themename",
	"ThemeFolder" : "themefolder",
    "SourceFoldersToCopy" : ["folder1", "foldern"],
    "Footer" : "footer text or HTML",
    "StartingPageName" : "Home",
    "ConvertLocalLinks" : "true" | "false",
    "PathSpecification" : "Full" | "Relative" | "RelativeAsFolder",
    "UrlFormatting" : "None" | "Strip" | "Dashes",
    "MaxLevelInToC" : "3",
    "StripIndexHtm" : "true" | "false",
    "Pages" : 
    {
        "__index" : "index.md",
        "Title Page 1" : "filename_page1.md", 
        "Title Page 2" : "filename_page2.md",
        "Sub Header 1" : 
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

### Name
Specifies the name of the site to generate. 

### Source
Specifies the root of the tree of folders in which markdown files are expected. `sourcefolder` can be a relative path or an absolute path and is used as the root folder for filenames specified in `Pages`.

### Destination
Specifies the folder where the output will be written to (.htm files). The output will be written to this folder only, all files from subfolders in `Source` will be written to the folder specified in `Destination`. with the same structure as the *navigation* in `Pages`. `destinationfolder` can be a relative path or an absolute path.

### IncludeSource
Specifies the folder where the files specified to be included using [@@include directives](markdownextensions.htm#include-files) are located. If `IncludeSource` isn't specified, the value `Includes` is assumed. 

### Theme
Specifies the name of the theme to use which is a folder within the `Themes` folder in the folder the `docnet` executable is located which is used as the theme for the pages to generate. `Docnet` expects a file called `PageTemplate.htm` within the specified `Theme` folder, which contains the HTML which is used as the wrapper file for the HTML generated from the markdown. It has to contain a couple of marker, which are described later in this document. If `Theme` isn't specified, `Default` is assumed.

If `ThemeFolder` is specified, the theme is looked up in that folder.

### ThemeFolder
Optional folder specification of the folder which contains the theme specified in `Theme`. If not specified, the `Themes` folder of the docnet executable is used. 

### SourceFoldersToCopy
This is an optional directive with, if specified, one or more folder names relative to `Source`, which contain files to copy to the `Destination` folder. E.g. image files used in the markdown files, located in an `Images` folder can be copied this way to the output folder reliably. All folders specified are copied recursively.

### ConvertLocalLinks
This is an optional directive which, if specified and set to `"true"`, will make DocNet convert all local links to `.md` suffixed files into `.htm` files. Example: `(local link)[somemarkdownfile.md]` will be converted to `<a href="somemarkdownfile.htm">local link</a>`. Non-local urls are not converted. Default: `"false"`. 

### PathSpecification 
Determines the way (md) paths are treated by the tooling. The default value is `Full` for backwards compatibility.

* `Full`: Assumes that all paths are full paths. All auto-generated index files will be placed in the root folder (this setting can lead to index clashes when reusing names in subfolders).
* `Relative`: Assumes that all paths are relative paths. All auto-generated index files will be put in the right (sub)folder.
* `RelativeAsFolder`: Behaves the same as `Relative`, but puts *every* source md in its own folder resulting in clean navigation urls (e.g. `/getting-started/introduction.htm` becomes `/getting-started/introduction/index.htm`)

### UrlFormatting
Determines how the urls are formatted. The default value is `None` which will only remove unsupported characters from the urls.

* `None`: Does not touch the urls except from removing unsupported characters from the urls.
* `Strip`: Replaces all non-text characters (e.g. spaces, dots, commas, etc) by an empty string (e.g. `/my-documentation/Some Introduction.md` results in `/mydocumentation/someintroduction.htm`)  
* `Dashes`: Replaces all non-text characters (e.g. spaces, dots, commas, etc) by a dash (`-`) (e.g. `/my-documentation/Some Introduction.md` results in `/my-documentation/some-introduction.htm`)  

### MaxLevelInToC
Sets the level of headings to show in the Table of Contents (ToC). The default value is `2`. To show one additional level, one would use `3` for this value. 

@alert info
Note that level 1 headings (titles) are never shown in the ToC).
@end

### StripIndexHtm
If set to `true`, the tool will strip `index.htm` from the end of urls. The default value is `false`. 

@alert tip
Combined with `PathSpecification` set to `RelativeAsFolder`, this will result in a 'folder-based' browsing experience (e.g. `/getting-started/introduction/`)
@end

@alert important
Note that setting this value to `true` will remove the possibility to view the docs off-line                          
@end

### Footer
This is text and/or HTML which is placed in the footer of each page, using a _marker_ (see below).

### StartingPageName
This is the name used for the home/starting page, i.e. the `__index` page at the root level. The default value is "Home".

### Pages
Contains the pages to generate into the output, in the order and structure in which you want them to appear in the navigation. The name given is the value used in the navigation tree and has to be unique per level. The value specified with each name is the markdown file to load, parse and generate as .htm file in the output. The markdown file is relative to the path specified in `Source`. A file `foo.md` will be generated as `foo.htm` in the output. 

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