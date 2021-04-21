echo This file needs its paths fixed. TODO:
@echo off
if exist "../bin/Release/DSAHelper/NetSparkle.DSAHelper.exe" (
	if exist "../bin/Release/DSAHelper/SampleDownloadedExecutable/NetSparkleUpdate.exe" (
		"../bin/Release/DSAHelper/NetSparkle.DSAHelper.exe" /sign_update "../bin/Release/SampleDownloadedExecutable/NetSparkleUpdate.exe" NetSparkle_DSA.priv > hash-demo-file-release.txt
	)
	if exist "../bin/Debug/DSAHelper/SampleDownloadedExecutable/NetSparkleUpdate.exe" (
		"../bin/Release/DSAHelper/NetSparkle.DSAHelper.exe" /sign_update "../bin/Debug/SampleDownloadedExecutable/NetSparkleUpdate.exe" NetSparkle_DSA.priv > hash-demo-file-debug.txt
	)
) else if exist "../bin/Debug/DSAHelper/NetSparkle.DSAHelper.exe" (
	if exist "../bin/Release/DSAHelper/SampleDownloadedExecutable/NetSparkleUpdate.exe" (
		"../bin/Debug/DSAHelper/NetSparkle.DSAHelper.exe" /sign_update "../bin/Release/SampleDownloadedExecutable/NetSparkleUpdate.exe" NetSparkle_DSA.priv > hash-demo-file-release.txt
	) 
	if exist "../bin/Debug/SampleDownloadedExecutable/NetSparkleUpdate.exe" (
		"../bin/Debug/DSAHelper/NetSparkle.DSAHelper.exe" /sign_update "../bin/Debug/SampleDownloadedExecutable/NetSparkleUpdate.exe" NetSparkle_DSA.priv > hash-demo-file-debug.txt
	)
)