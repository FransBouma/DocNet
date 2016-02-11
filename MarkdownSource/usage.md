Usage
=====

The usage is simple:

docnet *options* *folder*

*options* can be:

-   -c : clear destination folder. 

By default `docnet` doesn't remove any file in the destination folder. When specified it will remove all files and folders recursively in the specified `Destination` folder in the found docnet.json. So use this option with caution, as it won't check whether this is the folder your family photos or precious sourcecode are located! 

*folder* is the folder where a docnet.json file is expected to be present. 

##Requirements
`Docnet` is a .NET full application (using .NET 4.6.1) and requires .NET full to run. Not tested on Mono but it's highly likely it works on Mono without a problem. The code uses .NET 4.6.1 but it can be compiled against lower versions of .NET full, it doesn't use .NET 4.6 specific features but as Microsoft supports only the latest .NET 4.x versions, it was a logical choice to use .NET 4.6.1.
