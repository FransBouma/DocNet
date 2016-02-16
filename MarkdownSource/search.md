Search
======

`Docnet` will generate a search_data.json file in the root of the destination folder which is used with the javascript based search. It's a simple text search which can locate pages based on the word/sentence specified and will list them in first come first served order. For general purposes of locating a general piece of documentation regarding a topic it's good enough.

*NOTE*: Search locally on a file:/// served site won't work in Chrome, due to cross-origin protection because the search loads the search index and a template from disk in javascript. Either use Firefox or use the site with a server.
