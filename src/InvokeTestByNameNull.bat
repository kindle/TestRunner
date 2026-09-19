
@echo off

if %PROCESSOR_ARCHITECTURE% == x86 (
goto :x86
) else (
goto :x64
)

:x86
call "C:\Program Files\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"


:x64
call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"



cd tests


