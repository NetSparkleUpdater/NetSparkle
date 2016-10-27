@echo off
if exist "../bin/Release/NetSparkle.DSAHelper.exe" (
	if exist "../bin/Release/SampleDownloadedExecutable.exe" (
		"../bin/Release/NetSparkle.DSAHelper.exe" /sign_update "../bin/Release/SampleDownloadedExecutable.exe" NetSparkle_DSA.priv > hash-demo-file-release.txt
	)
	if exist "../bin/Debug/SampleDownloadedExecutable.exe" (
		"../bin/Release/NetSparkle.DSAHelper.exe" /sign_update "../bin/Debug/SampleDownloadedExecutable.exe" NetSparkle_DSA.priv > hash-demo-file-debug.txt
	)
) else if exist "../bin/Debug/NetSparkle.DSAHelper.exe" (
	if exist "../bin/Release/NetSparkle.TestApp.exe" (
		"../bin/Debug/NetSparkle.DSAHelper.exe" /sign_update "../bin/Release/SampleDownloadedExecutable.exe" NetSparkle_DSA.priv > hash-demo-file-release.txt
	) 
	if exist "../bin/Debug/NetSparkle.TestApp.exe" (
		"../bin/Debug/NetSparkle.DSAHelper.exe" /sign_update "../bin/Debug/SampleDownloadedExecutable.exe" NetSparkle_DSA.priv > hash-demo-file-debug.txt
	)
)