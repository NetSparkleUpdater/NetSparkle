@echo off

if "%~1"=="" (
	echo Error: A version number is required
	echo Usage: "NetSparklePackNuget.cmd [VersionNumber]"
	exit
)

echo Cleanup NuGet environment
rmdir /S /Q nuget\core\lib
rmdir /S /Q nuget\core\content
rmdir /S /Q nuget\core\tools
del \q nuget\core\*.nupkg
rmdir /S /Q nuget\tools\lib
rmdir /S /Q nuget\tools\content
rmdir /S /Q nuget\tools\tools
del \q nuget\tools\*.nupkg

echo Create NuGet tree for core
mkdir nuget\core\lib
mkdir nuget\core\content
mkdir nuget\core\tools

echo Create NuGet tree for tools
mkdir nuget\tools\lib
mkdir nuget\tools\content
mkdir nuget\tools\content\Extras
mkdir nuget\tools\tools

echo Copy Core Buildoutput to Nuget dir
xcopy /s /q bin\Release\NetSparkle\* nuget\core\lib\net45\
del /q nuget\core\lib\net45\*.pdb

echo Copy Tools Buildoutput to Nuget dir
xcopy /s /q /y bin\Release\NetSparkleChecker\* nuget\tools\tools\
xcopy /s /q /y bin\Release\DSAHelper\* nuget\tools\tools\
xcopy /s /q /y bin\Release\NetSparkleGenerator\* nuget\tools\tools\
xcopy /s /q /y Extras\* nuget\tools\content\Extras\
del /q nuget\tools\tools\*.config
del /q nuget\tools\tools\*.pdb
del /q nuget\tools\tools\*.xml
del /q nuget\tools\tools\*.manifest
del /q nuget\tools\tools\*.vshost.*

echo Moving to release directory
cd nuget

echo Packing core nuget package
cd core
..\nuget pack NetSparkle.nuspec -Version %1
cd ..

echo Packing tools nuget package
cd tools
..\nuget pack NetSparkle.Tools.nuspec -Version %1
cd ..

echo Pushing nuget package 
nuget Push core\NetSparkle.New.%1.nupkg -Source https://www.nuget.org/api/v2/package
nuget Push tools\NetSparkle.New.Tools.%1.nupkg -Source https://www.nuget.org/api/v2/package

echo Leaving directories
cd ..