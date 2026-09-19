
@echo off

if exist %SystemRoot%\SysWow64 goto x64
if exist %SystemRoot%\System32 goto x86
echo Couldn't find %SystemRoot%\System32
goto end

:x86

echo.
echo switch to x86...
call "C:\Program Files\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"
goto runcase

:x64

echo.
echo switch to x64
call "C:\Program Files (x86)\Microsoft Visual Studio 10.0\VC\vcvarsall.bat"

goto runcase

:runcase

cd tests

mstest /testcontainer:%1 /test:%2 /unique

