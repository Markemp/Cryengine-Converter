# Cryengine Converter

[Cryengine Converter](https://www.heffaypresents.com/GitHub) is a C# program to help convert Cryengine assets into a more portable format. Currently it supports .obj and .dae (Collada) to some extent, although work is in progress to allow exporting Cryengine assets into FBX format.

How do you use it?  Well, here is the output from the current Usage:

```
cgf-converter [-usage] | <.cgf file> [-outputfile <output file>] [-objectdir <ObjectDir>] [-obj] [-blend] [-dae] [-smooth] [-throw]

-usage:           Prints out the usage statement

<.cgf file>:      Mandatory.  The name of the .cgf, .cga or .skin file to process
-outputfile:      The name of the file to write the output.  Default is [root].obj
-noconflict:      Use non-conflicting naming scheme (<cgf File>_out.obj)
-allowconflict:   Allows conflicts in .mtl file name
-objectdir:       The name where the base Objects directory is located.  Used to read mtl file
                  Defaults to current directory.
-obj:             Export Wavefront format files (Default: true)
-blend:           Export Blender format files (Not Implemented)
-dae:             Export Collada format files (Not Implemented)
-smooth:          Smooth Faces
-group:           Group meshes into single model

-throw:           Throw Exceptions to installed debugger
```

Ok, so how do you actually **USE** it?
