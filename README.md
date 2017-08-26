<img src="https://www.heffaypresents.com/images/logos/logo-50px-prod.png" align=right alt="Heffay Presents" width="64px" height="64px">

# Cryengine Converter

[Cryengine Converter](https://www.heffaypresents.com/GitHub) is a C# program to help convert Cryengine assets into a more portable format. Currently it supports `.obj` and `.dae` (Collada) to some extent, although work is in progress to allow exporting Cryengine assets into FBX format.

How do you use it?  Well, here is the output from the current Usage:

```powershell
PS D:\scripts> cgf-converter

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

I'm going to assume you've already taken a Cryengine based game (Mechwarrior Online, Star Citizen, etc) and extracted the `.pak` files into a directory structure that essentially mimics a Cryengine ... layout?  Sure, let's say layout.

```powershell
PS D:\Blender Projects\Star Citizen> dir

    Directory: D:\Blender Projects\Star Citizen

Mode                LastWriteTime         Length Name
----                -------------         ------ ----
d-----        5/20/2017   7:59 AM                Animations
d-----        5/20/2017   7:59 AM                Entities
d-----        5/20/2017   7:59 AM                Levels
d-----        5/20/2017   8:00 AM                Libs
d-----        5/20/2017   7:59 AM                Materials
d-----        5/20/2017   8:43 AM                Objects
d-----        5/20/2017   8:00 AM                Prefabs
d-----        5/20/2017   8:00 AM                Scripts
d-----        5/20/2017   8:54 AM                Sounds
d-----        5/20/2017   9:04 AM                Textures
d-----        5/20/2017   9:05 AM                UI
```

This is pretty close to what a game like Star Citizen will look like after you extract all the `.pak` files, using a utility like 7zip.  The important directories here are Objects (generally contains the `.cga/.cgam/.cgf/.skin` files) and the Textures directory.  You *generally* don't need to worry about the directory structure unless you're using my asset-importer.ps1 or mech-importer.ps1 script, but let's just call this the root directory for Cryengine assets.


> **Aside:**  When compiled (or you just download the .exe), this program is easiest to use when you put it into a dedicated directory that is in the path.  I won't go into [how to modify the path on your computer](http://lmgtfy.com/?q=changing+path+on+a+windows+computer), but I have my own `d:\scripts` directory with cgf-converter.exe in it, along with a few other commonly used scripts and programs.  I recommend you do the same (or something similar), as from now on the Powershell commands I type out will assume that the programs are in the path.  If they aren't, **the commands I list will not work.**

> **Aside 2.0:**  Be careful exporting stuff to **`.obj`** files, which is currently the default method.  A Cryengine file (in incredibly simplified terms) consists of a geometry file (ends in `.cga/.cgaf/.cgam`) and the related material file (ends in `.mtl`).  The Cryengine material file is an XML file that contains material info.  This program will take that file and convert it by default to an .obj material file with the *same name*, which is not ideal.  Use the `-noconflict` argument to make it write to a similar name that won't conflict.

To convert a single `.cga/.cgf/.skin` file to an `.obj` file, using Powershell:

```powershell
PS D:\Blender Projects\Star Citizen\Objects\Spaceships.ships\AEGS\gladius\>cgf-converter AEGS_Gladius.cga
```

This will create a couple of files in that directory:
* `AEGS_Gladius.obj`
* `AEGS_Gladius.mtl`

Since the Cryengine `.mtl` file has a good chance of being in that directory, it will overwrite it unless you use the `-noconflict` argument.

```powershell
PS D:\Blender Projects\Star Citizen\Objects\Spaceships.ships\AEGS\gladius\>cgf-converter -noconflict AEGS_Gladius.cga
```
Instead of an `AEGS_Gladius.mtl` file, it will create an `AEGS_Gladius_mtl.mtl` file, and leave the original `.mtl` file as is.

There are occasions where you may want to overwrite the file, but generally you are going to want to use the `-noconflict` argument.

Collada format (v1.4.1) may be a better option for most game assets.  On top of having cleaner geometry, the material file is included into the Collada (`.dae`) file, so you only have one file to worry about (so no need to use `-noconflict`).  In addition, if there is an armature, Collada files can also contain all that information as well (Not Yet Implemented).

To write to a Collada format, you run:

```Powershell
PS D:\Blender Projects\Star Citizen\Objects\Spaceships.ships\AEGS\gladius\>cgf-converter -dae AEGS_Gladius.cga
```

You can replae the `-dae` with `-collada` as well.

If you want to convert all the files in a directory, you can use a loop in Powershell (directory info removed for readibility):

```Powershell
foreach ($file in (get-childitem *.cg*)) { cgf-converter -dae $file }
```

This will take every file in the directory where the command is being run and convert it to Collada format.

If you want to convert all the files in a directory as well as all the directories below it, you can use the `-recurse` option to make it traverse:

```Powershell
foreach ($file in (get-childitem -recures *.cg*)) { cgf-converter -dae $file }
```

> **NOTE:** If you run this on the `Objects` directory, it will convert EVERY file in the game.  This can take a very long time, and takes a lot of disk space.

Finally, the converter does support being run through the Windows Explorer, so you can just drag a `.cga` file or files onto `cgf-converter.exe` and it'll do a default conversion (to `.obj`).  This isn't ideal, but it is the quick and dirty way if you are morally opposed to using a prompt. :+1:

Questions?  Feel free to contact me and I'll be happy to provide some additional help.
