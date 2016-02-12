Markdown extensions
===================

`Docnet` defines the following markdown extensions to make writing  

## Alert boxes
To quickly define alert boxes, `Docnet` defines the `@alert` element. Three types of alerts are defined: *danger* (displayed in red), *warning* (displayed in yellow) and *info* or *neutral*, which is displayed in blue. You specify the type of the alert after the `@alert` statement using &#64;alert *name*. Close the `@alert` with `@end`. 

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
@alert info
This is an info text, it will be displayed in an info alert box!
@end
```

Results in
  
@alert info
This is an info text, it will be displayed in an info alert box!
@end

## Font Awesome icons
To specify a font-awesome icon, use `@fa-iconname`, where _iconname_ is the name of the font-awesome icon.

Example: To specify the font awesome icon for GitHub, use `@fa-github`, which will result in: @fa-github  

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

