@echo off

echo Cleanup NuGet environment
rmdir /S /Q Nuget\core\lib
rmdir /S /Q Nuget\core\content
rmdir /S /Q Nuget\core\tools
del \q Nuget\core\*.nupkg
rmdir /S /Q Nuget\tools\lib
rmdir /S /Q Nuget\tools\content
rmdir /S /Q Nuget\tools\tools
del \q Nuget\tools\*.nupkg

echo Create NuGet tree for core
mkdir Nuget\core\lib
mkdir Nuget\core\content
mkdir Nuget\core\tools

echo Create NuGet tree for tools
mkdir Nuget\tools\lib
mkdir Nuget\tools\content
mkdir Nuget\tools\content\Extras
mkdir Nuget\tools\tools

echo Copy Core Buildoutput to Nuget dir
xcopy /s /q bin\Release\NetSparkle\* Nuget\core\lib\net45\
del /q Nuget\core\lib\*.pdb

echo Copy Tools Buildoutput to Nuget dir
xcopy /s /q /y bin\Release\NetSparkleChecker\* Nuget\tools\tools\
xcopy /s /q /y bin\Release\DSAHelper\* Nuget\tools\tools\
xcopy /s /q /y Extras\* Nuget\tools\content\Extras\
del /q Nuget\tools\tools\*.config
del /q Nuget\tools\tools\*.pdb
del /q Nuget\tools\tools\*.xml
del /q Nuget\tools\tools\*.manifest
del /q Nuget\tools\tools\*.vshost.*

echo Moving to release directory
cd Nuget

echo Packing core nuget package
cd core
..\nuget pack NetSparkle.nuspec -Version %1
cd ..

echo Packing tools nuget package
cd tools
..\nuget pack NetSparkle.Tools.nuspec -Version %1
cd ..

echo Pushing nuget package 
nuget Push core\NetSparkle.New.%1.nupkg
nuget Push tools\NetSparkle.New.Tools.%1.nupkg

echo Leaving directories
cd ..