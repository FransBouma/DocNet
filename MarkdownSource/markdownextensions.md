DocNet Markdown extensions
==========================

`Docnet` defines the following markdown extensions to make writing documentation easier. 

## Alert boxes
To quickly define alert boxes, `Docnet` defines the `@alert` element. Three types of alerts are defined: *danger* (displayed in red), *warning* or *important* (displayed in yellow) and *info* or *neutral*, which is displayed in blue. You specify the type of the alert after the `@alert` statement using &#64;alert *name*. Close the `@alert` with `@end`.

Below are examples for each alert box and the markdown used to create them. 

The markdown:

```nohighlight
@alert danger
This is a dangerous text, it will be displayed in a danger alert box!
@end
```

results in
  
@alert danger
This is a dangerous text, it will be displayed in a danger alert box!
@end

The markdown:

```nohighlight
@alert warning
This is a warning text, it will be displayed in a warning alert box!
@end
```

results in

@alert warning
This is a warning text, it will be displayed in a warning alert box!
@end

The markdown:

```nohighlight
@alert important
This is an important text, it will be displayed in a warning/important alert box!
@end
```

results in

@alert important
This is an important text, it will be displayed in a warning/important alert box!
@end

The markdown:

```nohighlight
@alert info
This is an info text, it will be displayed in an info alert box!
@end
```

Results in

@alert info
This is an info text, it will be displayed in an info alert box!
@end

The markdown:

```nohighlight
@alert tip
This is a tip! It will be displayed in a tip alert box!
@end
```

Results in

@alert tip
This is a tip! It will be displayed in a tip alert box!
@end

## Font Awesome icons
To specify a font-awesome v4 icon, use `@fa-iconname`, where _iconname_ is the name of the font-awesome icon.

Example: To specify the font awesome icon for GitHub, use `@fa-github`, which will result in: @fa-github

To use font-awesome v6, you have to use either `@fabrands-iconname` or `@fasolid-iconname` and adjust the template html to include
v6 fontawesome assets. 

## Tabs
It's very easy with `Docnet` to add a tab control with one or more tabs to the HTML with a simple set of markdown statements. The tab statements are converted into pure CSS3/HTML tabs, based on the work of [Joseph Fusco](http://codepen.io/fusco/pen/Wvzjrm).

To start a Tab control, start with `@tabs` and end the tabs definition with `@endtabs`. Between those two statements, which each need to be suffixed with a newline, you define one or more tabs using `@tab` followed by the label text for that tab, followed by a newline. End your tab contents with `@end`.

The following example shows two tabs, one with label 'First Tab' and one with 'Second Tab':

```nohighlight
@tabs
@tab First Tab
This is the text for the first tab. It's nothing special

As you can see, it can deal with newlines as well. 
@end
@tab Second Tab
Now, the second tab however is very interesting. At least let's pretend it is!
@end
@endtabs
```

will result in: 

@tabs
@tab First Tab
This is the text for the first tab. It's nothing special

As you can see, it can deal with newlines as well. 
@end
@tab Second Tab
Now, the second tab however is very interesting. At least let's pretend it is!
@end
@endtabs


##Snippets
You can include snippets from other files as fenced code blocks using the directive `@snippet` which has the following syntax:

`@snippet language [file specification] pattern`

Here, _language_ can be one of `cs`, `txt` or `xml`. If an unknown language is specified, `txt` is chosen. _Pattern_ is used to determine which part of the file specified between `[]`
is to be embedded at the spot of the `@snippet` fragment. This code is based on [Projbook's extractor feature](http://defrancea.github.io/Projbook/projbook.html#Pageextractormd) and follows the same pattern system. 

Below, the method `GenerateToCFragment` is embedded in a C# fenced code block. This method is a DocNet method and is obtained directly from the source code. This shows the `@snippet`
feature's power as it keeps the documentation in sync with the embedded code without the necessity of updating things. 

The following snippet, if the DocNet sourcecode is located at the spot reachable by the path below:

```nohighlight
@snippet cs [../../DocNet/src/DocNet/NavigationLevel.cs] GenerateToCFragment
```

will result in:
@snippet cs [../../DocNet/src/DocNet/NavigationLevel.cs] GenerateToCFragment

##Include files
You can include other files in your markdown files using the directive `@@include("filename")`, where `filename` is the name of the file to include. The include system isn't recursive. 
The files to include are read from a special folder, specified under `IncludeSource` in the [docnet.json](docnetjson.htm) file. If no `IncludeSource` directive is
specified in the [docnet.json](docnetjson.htm) file, the folder `Includes` is assumed. 

The directive `@@include("somehtmlinclude.htm")`

results in the contents of `somehtmlinclude.htm` being included at the spot where the @@include statement is given, as shown below:

@@include("includedhtml.htm")

You can also include markdown, which is then processed with the other markdown as if it's part of it. 

@@include("includedmarkdown.md")

