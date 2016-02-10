#DocNet

DocNet is a simple static documentation generator, using markdown files to build the content. 

##Usage
The usage is simple:

docnet *options* *folder*

*options* can be:
* -c : clear destination folder. By default `docnet` doesn't remove any file in the destination folder. When specified it will remove all files and folders recursively in the specified `Destination` folder in the found docnet.json. So use this option with caution, as it won't check whether this is the folder your family photos or precious sourcecode are located! 

*folder* is the folder where a docnet.json file is expected to be present. 

##docnet.json
DocNet uses a json file to determine what to do in what form. The format is straight forward and described below

```json
{
    "Name" : "name of site",
    "Source" : "sourcefolder",
    "Destination" : "destinationfolder",
    "Theme" : "themefolder",
    "SourceFoldersToCopy" : ["folder1", "foldern"],
    "Footer" : "footer text or HTML",
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
        }
    }
}
```

The order in which the pages are specified is the order in which they'll appear in the navigation. 

* `Name` specifies the name of the site to generate. 
* `Source` specifies the root of the tree of folders in which markdown files are expected. `sourcefolder` can be a relative path or an absolute path and is used as the root folder for filenames specified in `Pages`. 
* `Destination` specifies the folder where the output will be written to (.htm files). The output will be written to this folder only, all files from subfolders in `Source` will be written to the folder specified in `Destination`. with the same structure as the *navigation* in `Pages`. `destinationfolder` can be a relative path or an absolute path.
* `Theme` specifies the folder within the `Themes` folder in the folder the `docnet` executable is located which is used as the theme for the pages to generate. `Docnet` expects a file called `PageTemplate.htm` within the specified `Theme` folder, which contains the HTML which is used as the wrapper file for the HTML generated from the markdown. It has to contain a couple of marker, which are described later in this document. If `Theme` isn't specified, `Default` is assumed.
* `SourceFoldersToCopy`. This is an optional directive with, if specified, one or more folder names relative to `Source`, which contain files to copy to the `Destination` folder. E.g. image files used in the markdown files, located in an `Images` folder can be copied this way to the output folder reliably. All folders specified are copied recursively.
* `Footer`. This is text and/or HTML which is placed in the footer of each page, using a _marker_ (see below).
* `Pages` contains the pages to generate into the output, in the order and structure in which you want them to appear in the navigation. The name given is the value used in the navigation tree and has to be unique per level. The value specified with each name is the markdown file to load, parse and generate as .htm file in the output. The markdown file is relative to the path specified in `Source`. A file `foo.md` will be generated as `foo.htm` in the output. 

Paths are expected to use `\` characters, and as it's json, you have to escape them, so the path `.\foo` becomes `.\\foo`.

Each level, starting with the root, has a special page defined, `__index`. This page is the page shown when the level header (in the example above this is e.g. _Sub Header 1_) is clicked in the navigation. If `__index` is specified in the root level, it's assigned to the navigation name `Home`. If there's no `__index` specified, there will still be a page generated but it will have default content (See below). The names of the elements are case sensitive.

### Levels without __index defined
If a level has no `__index` defined, `DocNet` will create a `__index` entry for the level and will specify as target `<path to index of parent>/nameoflevel.md`. If the page exists it will be loaded as the content for the index of the level, if it doesn't exist, the HTML will simply contain the topictitle and a list of all the sub topics in the level. This guarantees the tree can always be navigated in full.  

## Automatic H2 level ToC entry discovery
`Docnet` will automatically add all H2 (`##` marked) headers to the ToC as sub navigation elements below a page ToC item. It will automatically add anchors to these H2 headers in the HTML output for the page as well. This makes it very easy to create a fine-grained ToC for easy discovery.

## Themes
`Docnet` uses themes to produce output in a certain form. A theme is a folder within the `Themes` folder which contains a `PageTemplate.htm` file and a `Destination` folder which contains zero or more folders and files which have to be copied to the `Destination` folder specified in the `docnet.json` file. 

### Themes folder
`Docnet` expects the `Themes` folder to be located in the folder where the executable is started from. This means that if you build `Docnet` from source, you have to manually copy the Themes folder to the folder your binary is located. To make development easier, you could create a `junction` in the bin\debug or bin\release folder to the Themes folder in the source repository, using `mklink` on a windows command prompt.

### PageTemplate.htm
The `PageTemplate.htm` file is a simple HTML file, located in each `theme` folder, which is used as the template for all generated `.htm` files. You can place whatever you like in there, including references to css/js files, headers, footers etc. DocNet however expects a couple of *markers* which are replaced with the data created from the markdown files. These markers are described below. The markers have to be specified as-is.

* `{{Name}}`. This is replaced with the value specified in `Name` in the `docnet.json` file.
* `{{Content}}`. This is replaced with the HTML generated from the markdown file. 
* `{{ToC}}`. This is replaced with a `<ul><li></li></ul>` tree built from the names and structure given to pages in `Pages`.
* `{{TopicTitle}}`. This is replaced with the title of the page, which is the value specified as name in the `Pages` tree. 
* `{{Footer}}`. This is replaced with the value specified in `Footer` in the docnet.json file. 
* `{{Breadcrumbs}}`. This is replaced with a / delimited list of names making up the bread crumbs representing the navigation through the ToC to reach the current page. 
* `{{ExtraScript}}`. This is replaced with extra script definitions / references required by some pages, like the search page. It's `docnet` specific and if this marker isn't present, search won't work.
* `{{Path}}`. This is used to fill in the relative path to reach css/js files in hard-coded URLs in the `PageTemplate` file. This means that specifying a css URL in `PageTemplate` should look like:
```HTML
<link rel="stylesheet" href="{{Path}}css/theme.css" type="text/css" />
```
`Docnet` will then replace `{{Path}}` with e.g. '../../' to get to the css file from the location of the .htm file loaded.

##Markdown extensions
`Docnet` defines the following markdown extensions. 

### Alert boxes
To quickly define alert boxes, `Docnet` defines the `@alert` element. Three types of alerts are defined: `danger` (displayed in red), `warning` (displayed in yellow) and `info` or `neutral`, which is displayed in blue. You specify the type of the alert after the `@alert` statement using `@alert name`. Close the `@alert` with `@end`. 

Example:
```html
@alert warning
This is a warning text, it will be displayed in a warning note
@end
```

### Font Awesome icons
To specify a font-awesome icon, use `@fa-iconname`, where _iconname_ is the name of the font-awesome icon.

Example:
```
This will display the font-awesome icon for anchor:  @fa-anchor
```

##Search
`Docnet` will generate a search_data.json file in the root of the destination folder which is used with the javascript based search. It's a simple text search which can locate pages based on the word/sentence specified and will list them in first come first served order. For general purposes of locating a general piece of documentation regarding a topic it's good enough.

*NOTE*: Search locally on a file:/// served site won't work in Chrome, due to cross-origin protection because the search loads the search index and a template from disk in javascript. Either use Firefox or use the site with a server.

##Linking
`Docnet` doesn't transform links. This means that any link to any document in your documentation has to use the url it will get in the destination folder. Example: you want to link to the file `How to\AddEntity.md` from a page. In the result site this should be the link `How%20to/AddEntity.htm`, which you should specify in your markdown. In the future it might be `docnet` will be updated with link transformation, at the moment it doesn't convert any links besides the usual markdown ones. The markdown parser also doesn't allow spaces to be present in the urls. If you need a space in the url, escape it with `%20`. 

##Acknowledgements
This application wouldn't be possible without the work of others. The (likely incomplete) list below contains the work `Docnet` is based on / builds upon. 

* [MkDocs](http://www.mkdocs.org/). `Docnet` borrows a great deal from `MkDocs`: the theme css `Docnet` uses is based on a cleaned up version of their `Readthedocs` theme, as well as the javascript based search is from MkDocs. 
* [MarkdownSharp](https://github.com/KyleGobel/MarkdownSharp-GithubCodeBlocks). The markdown parser is an extended version of the `MarkdownSharp` parser from StackExchange with the extensions added by Kyle Gobel. 
* [Json.NET](http://www.newtonsoft.com/json). The reading/writing of json files is important for `Docnet` and it uses Json.NET for that. 

I wrote the initial version in roughly 7-8 days, to meet our needs for generating a static searchable documentation site. It therefore might not match what you need or not flexible enough. Feel free to fork the repo and adjust it accordingly!

##License
The MIT License (MIT)

Copyright (c) 2016 Frans Bouma

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

