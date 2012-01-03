@echo off

echo Cleanup NuGet environment
rmdir /S /Q Nuget\lib
rmdir /S /Q Nuget\content
rmdir /S /Q Nuget\tools

echo Create NuGet tree
mkdir Nuget\lib
mkdir Nuget\content
mkdir Nuget\content\Extras
mkdir Nuget\tools

echo Copy Buildoutput to Nuget dir
xcopy /s /q NetSparkle\Release\lib\net40-full\* Nuget\lib\net40-full\
del /q Nuget\lib\*.pdb
xcopy /s /q NetSparkleChecker\bin\Release\* Nuget\content\
xcopy /s /q NetSparkleDSAHelper\bin\Release Nuget\content\
xcopy /s /q NetSparkle\trunk\Extras\* Nuget\content\Extras\
del /q Nuget\content\*.config
del /q Nuget\content\*.pdb
del /q Nuget\content\*.manifest
del /q Nuget\content\*.dll
del /q Nuget\content\*.vshost.*


echo Moving to release directory
cd Nuget\Content

echo Packing nuget package
..\nuget pack ..\NetSparkle.nuspec -Version %1

echo Pushing nuget package 
..\nuget Push AppLimit.NetSparkle.%1.nupkg

echo Leaving directories
cd ..\..