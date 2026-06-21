Cryengine Converter
===================

Cryengine Converter is a command-line tool. It is now installed and (if you
chose the option during install) added to your PATH.

QUICK START
-----------

  1. Open a NEW terminal window. Existing terminals will not pick up the
     PATH change automatically.

     We recommend Windows Terminal with a PowerShell tab:
       https://aka.ms/terminal

  2. Verify the install by running:

       cgf-converter -usage

     You should see the full usage banner.

  3. Convert your first asset:

       cgf-converter <path-to-asset.cgf> -objectdir <path-to-data-directory>

     The -objectdir argument is required for materials and textures to
     resolve correctly. Always pass it.


DOCUMENTATION
-------------

  Full documentation, examples, and the complete CLI reference:
    https://github.com/Markemp/Cryengine-Converter

  Tutorial videos:
    https://github.com/Markemp/Cryengine-Converter#tutorial-videos


SUPPORT
-------

  Report bugs:
    https://github.com/Markemp/Cryengine-Converter/issues


UNINSTALL
---------

  Use Settings -> Apps -> Installed Apps, find "Cryengine Converter",
  and click Uninstall. The uninstaller will remove the program files and
  also remove this directory from your PATH.
