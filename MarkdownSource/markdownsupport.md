Markdown support
================

`DocNet` uses markdown files as input. The markdown is parsed with the markdown parser from StackExchange (called '[MarkdownSharp](https://github.com/KyleGobel/MarkdownSharp-GithubCodeBlocks)'). It supports the default markdown statements as well as github style code block markers and specific extensions for writing documentation, which are described below. 

##Highlighting code
The markdown parser has been extended with GitHub code specifications, so it's easy to specify a specific code beautifying based on a language. This feature uses the [Highlight.js](https://highlightjs.org/) javascript library and most popular languages are included. 

Example: to specify a codeblock as C#, append `cs` after the first ` ``` ` marker:

```cs
var i=42;
```

To specify a block of text in a fixed sized font but not specify any language highlighting, specify `nohighlight` as language name:

```nohighlight
this is a simple <pre> block
```

##Linking
`Docnet` doesn't transform links. This means that any link to any document in your documentation has to use the url it will get in the destination folder. Example: you want to link to the file `How to\AddEntity.md` from a page. In the result site this should be the link `How%20to/AddEntity.htm`, which you should specify in your markdown. In the future it might be `docnet` will be updated with link transformation, at the moment it doesn't convert any links besides the usual markdown ones. The markdown parser also doesn't allow spaces to be present in the urls. If you need a space in the url, escape it with `%20`. 