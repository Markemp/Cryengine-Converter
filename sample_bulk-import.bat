@echo off
cls
set PATH=%PATH%;D:\SC_PAK
FOR /F "tokens=*" %%G IN ('dir /b /S *.cg?') DO (
  echo Convert "%%G"
  pushd %%~dpG
  cgf-converter.exe %%~nxG -objectdir D:\SC_PAK -outputfile D:\SC_PAK\_Converted\%%~pG\%%~nG.obj -obj
  popd
)
