# Cryengine-Converter
A c# program to convert Crytek files to something more usable (.obj and Collada, maybe .fbx if possible)

See the github-master for the actual code. Had some problems syncing up Visual Studio and github, 
so the master branch is a bit of a lost cause.

This is a refactoring of the PyFFI tool created for earlier versions of Cryengine. However, when trying 
to use it against updated versions of Cryengine games, it has its limitations, 
especially dealing with materials. So I'm rewriting it to handle the 3.4/3.5 data formats, and 3.6 and newer
data formats.

Output is to .obj and Collada (.dae), although I'd like to include .fbx.  A direct .blend file creator
would be nice too.

Get PyFFI from Sourceforge http://sourceforge.net/projects/pyffi/files/


