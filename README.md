# Cryengine-Converter
A c# program to convert Crytek files to something more usable (.obj, maybe .fbx or collada if possible)

See the github-master for the actual code. Had some problems syncing up Visual Studio and github, 
so the master branch is a bit of a lost cause.

This is a refactoring of the PyFFI tool created for earlier versions of Cryengine. However, when trying 
to use it against updated versions of Cryengine games (MWO, Star Citizen), it has its limitations, 
especially dealing with materials. So I'm rewriting it to handle the 3.4/3.5 data formats (current MWO, 
early Star Citizen files), and 3.6 data formats (current Star Citizen).

Output will probably be .obj, although I'd like to include .fbx or collada.

Get PyFFI from Sourceforge http://sourceforge.net/projects/pyffi/files/
