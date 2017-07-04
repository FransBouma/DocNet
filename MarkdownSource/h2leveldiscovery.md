Automatic H2 level ToC entry discovery
======================================

`Docnet` will automatically add all H2 (`##` marked) headers to the ToC as sub navigation elements below a page ToC item. It will automatically add anchors to these H2 headers in the HTML output for the page as well. This makes it very easy to create a fine-grained ToC for easy discovery.

This behavior can be overridden by the `MaxLevelInToC` [configuration option](./docnetjson.md).

@alert info
Note that level 1 headings (titles) are never shown in the ToC).
@end