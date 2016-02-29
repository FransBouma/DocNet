Markdown support
================

`DocNet` uses markdown files as input. The markdown is parsed with the markdown parser from topten software (called '[MarkdownDeep](http://www.toptensoftware.com/markdowndeep/)'). It supports the default markdown statements as well as github style code block markers and specific extensions for writing documentation, which are described below.

##Standard Markdown
The standard markdown syntax as defined by [John Gruber](https://daringfireball.net/projects/markdown/) is supported in full. 

##Php Markdown Extra
MarkdownDeep supports [PHP Markdown Extra](https://michelf.ca/projects/php-markdown/extra/). PHP Markdown Extra comes with a set of neat extensions for markdown to define e.g. tables, footnotes and more. Please see the link above for all the syntax examples. Additionally, you can look at the [unit test files](https://github.com/FransBouma/DocNet/tree/master/src/MarkdownDeepTests/testfiles/extramode) for MarkdownDeep in the [DocNet respository at GitHub](https://github.com/FransBouma/DocNet).  

A couple of examples are given below

###Footnotes
MarkdownDeep supports Footnotes, which can be added through the following system: To specify a footnote marker, specify `[^1]`, which will result in:[^1]

The actual footnote text is then specified with `[^1]:` following the text of the actual footnote. Click on the superscript `1` link above to go to the footnote rendered at the bottom of this page. 

[^1]: And this is the footnote related to the example above.

###Definition lists
To specify simple definition lists, simply do:

```
Item one
:   this is the description of item one

Item Two
:   this is the text of item 2
```

which results in: 

Item one
:   this is the description of item one

Item Two
:   this is the text of item 2

###Tables
There's basic support for defining tables. 

Specifying: 
```
Fruit|Color
--|--
Apples|Red
Pears|Green
Bananas|Yellow
Bongo|Bongo... it's a strange color, do you have a minute? It's a bit like the sea, but also a bit like the beach. You know how it is... oh and a bit like the wind too? You see it? Hey! Where're you going?! 
```

results in:

Fruit|Color
--|--
Apples|Red
Pears|Green
Bananas|Yellow
Bongo|Bongo... it's a strange color, do you have a minute? It's a bit like the sea, but also a bit like the beach. You know how it is... oh and a bit like the wind too? You see it? Hey! Where're you going?!

###Special attributes
DocNet supports special attributes for Links and Images. Currently this is supported on normal links/image specifications only, e.g.:
```
![id text](imageurl){.cssclass1 .cssclass2 #idvalue name=value}
```
which will result in:
```html
<img src="imageurl" alt="id text" id="idvalue" class="cssclass1 cssclass2" name="value" />
```

###Image rendering
By default images have no special rendering applied to them. To apply a shadow, specify '.shadowed' as css class in a special attribute specification. 
If you want to have an image rendered centered with a note below it, simply specify a title for the image: 

```
![](mycenteredpicture.jpg "this is a picture")
```

will be rendered as: (xxx and yyy are the width/height values of mycenteredpicture.jpg)
```html
<div class="figure">
<img src="mycenteredpicture.jpg" title="this is a picture" width="xxx" height="yyy"/>
<p>this is a picture</p>
</div>
```

All images rendered contain the width/height of the picture file included in the html.

###Abbreviations
There's also support for abbreviations, using the `<abbr>` HTML tag. 

Specifying:
```
*[FuBar]: F**ked Up Beyond Any Repair.
```
*[FuBar]: F**ked Up Beyond Any Repair.

gives an abbreviation link in the following sentence: This is a test for abbreviations: FuBar.

##Highlighting code
The markdown parser has been extended with GitHub code specifications, so it's easy to specify a specific code beautifying based on a language. This feature uses the [Highlight.js](https://highlightjs.org/) javascript library and most popular languages are included. 

Example: to specify a codeblock as C#, append `cs` after the first ``` marker:

```cs
var i=42;
```

To specify a block of text in a fixed sized font but not specify any language highlighting, specify `nohighlight` as language name:

```nohighlight
this is a simple <pre> block
```

##Linking
`Docnet` doesn't transform links. This means that any link to any document in your documentation has to use the url it will get in the destination folder. Example: you want to link to the file `How to\AddEntity.md` from a page. In the result site this should be the link `How%20to/AddEntity.htm`, which you should specify in your markdown. In the future it might be `docnet` will be updated with link transformation, at the moment it doesn't convert any links besides the usual markdown ones. The markdown parser also doesn't allow spaces to be present in the urls. If you need a space in the url, escape it with `%20`. 


