Writing content using Markdown
==============================

`DocNet` uses markdown files as input. The markdown is parsed with the markdown parser from StackExchange (called '[MarkdownSharp](https://github.com/KyleGobel/MarkdownSharp-GithubCodeBlocks)'). It supports the default markdown statements as well as github style code block markers and specific extensions for writing documentation, which are described below. 

##Markdown extensions

`Docnet` defines the following markdown extensions to make writing  

### Alert boxes
To quickly define alert boxes, `Docnet` defines the **&#64;alert** element. Three types of alerts are defined: *danger* (displayed in red), *warning* (displayed in yellow) and *info* or *neutral*, which is displayed in blue. You specify the type of the alert after the **&#64;alert** statement using &#64;alert *name*. Close the &#64;alert with &#64;end. 

Below are examples for each alert box and the markdown used to create them. 

The markdown: 

&#64;alert danger

This is a dangerous text, it will be displayed in a danger alert box!

&#64;end

Results in
  
@alert danger
This is a dangerous text, it will be displayed in a danger alert box!
@end

The markdown: 

&#64;alert warning

This is a warning text, it will be displayed in a warning alert box!

&#64;end

@alert warning
This is a warning text, it will be displayed in a warning alert box!
@end

The markdown: 

&#64;alert warning\
This is an info text, it will be displayed in an info alert box!\
&#64;end


Results in
  
@alert info
This is an info text, it will be displayed in an info alert box!
@end

### Font Awesome icons
To specify a font-awesome icon, use `@fa-iconname`, where _iconname_ is the name of the font-awesome icon.

Example:
```
This will display the font-awesome icon for anchor:  @fa-anchor
```

### Tabs
It's very easy with `Docnet` to add a tab control with one or more tabs to the HTML with a simple set of markdown statements. The tab statements are converted into pure CSS3/HTML tabs, based on the work of Joseph Fusco (http://codepen.io/fusco/pen/Wvzjrm)

To start a Tab control, start with `@tabs` and end the tabs definition with `@endtabs`. Between those two statements, which each need to be suffixed with a newline, you define one or more tabs using `@tab` followed by the label text for that tab, followed by a newline. End your tab contents with `@end`.

The following example shows two tabs, one with label 'C#' and one with 'VB.NET':
   
    @tabs
    @tab C#
    Content 1 2 3
    ```cs
    var text = DoTabsBlocks("text");
    ```
    @end
    @tab VB.NET1
    Content 1 2 3
    ```vb
    Dim text = DoTabsBlocks("text")
    ```
    Additional text
    @end
    @endtabs

##Search
`Docnet` will generate a search_data.json file in the root of the destination folder which is used with the javascript based search. It's a simple text search which can locate pages based on the word/sentence specified and will list them in first come first served order. For general purposes of locating a general piece of documentation regarding a topic it's good enough.

*NOTE*: Search locally on a file:/// served site won't work in Chrome, due to cross-origin protection because the search loads the search index and a template from disk in javascript. Either use Firefox or use the site with a server.

##Linking
`Docnet` doesn't transform links. This means that any link to any document in your documentation has to use the url it will get in the destination folder. Example: you want to link to the file `How to\AddEntity.md` from a page. In the result site this should be the link `How%20to/AddEntity.htm`, which you should specify in your markdown. In the future it might be `docnet` will be updated with link transformation, at the moment it doesn't convert any links besides the usual markdown ones. The markdown parser also doesn't allow spaces to be present in the urls. If you need a space in the url, escape it with `%20`. 

##Requirements
`Docnet` is a .NET full application (using .NET 4.6.1) and requires .NET full to run. Not tested on Mono but it's highly likely it works on Mono without a problem. The code uses .NET 4.6.1 but it can be compiled against lower versions of .NET full, it doesn't use .NET 4.6 specific features but as Microsoft supports only the latest .NET 4.x versions, it was a logical choice to use .NET 4.6.1.

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

