Themes
======

`Docnet` uses themes to produce output in a certain form. A theme is a folder within the `Themes` folder which contains a `PageTemplate.htm` file and a `Destination` folder which contains zero or more folders and files which have to be copied to the `Destination` folder specified in the `docnet.json` file. 

## Themes folder
`Docnet` expects the `Themes` folder to be located in the folder where the executable is started from. This means that if you build `Docnet` from source, you have to manually copy the Themes folder to the folder your binary is located. To make development easier, you could create a `junction` in the bin\debug or bin\release folder to the Themes folder in the source repository, using `mklink` on a windows command prompt.

## Default theme
The default theme is called `Default` and is chosen if no theme has been specified in the [docnet.json](docnetjson.htm) file. It is based on the theme from ReadTheDocs, and is created from the one shipped with MkDocs.

## PageTemplate.htm
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
