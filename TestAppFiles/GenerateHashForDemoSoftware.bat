@echo off
if exist "../bin/Release/NetSparkle.DSAHelper.exe" (
	if exist "../bin/Release/NetSparkleUpdate.exe" (
		"../bin/Release/NetSparkle.DSAHelper.exe" /sign_update "../bin/Release/NetSparkleUpdate.exe" NetSparkle_DSA.priv > hash-demo-file-release.txt
	)
	if exist "../bin/Debug/NetSparkleUpdate.exe" (
		"../bin/Release/NetSparkle.DSAHelper.exe" /sign_update "../bin/Debug/NetSparkleUpdate.exe" NetSparkle_DSA.priv > hash-demo-file-debug.txt
	)
) else if exist "../bin/Debug/NetSparkle.DSAHelper.exe" (
	if exist "../bin/Release/NetSparkleUpdate.exe" (
		"../bin/Debug/NetSparkle.DSAHelper.exe" /sign_update "../bin/Release/NetSparkleUpdate.exe" NetSparkle_DSA.priv > hash-demo-file-release.txt
	) 
	if exist "../bin/Debug/NetSparkleUpdate.exe" (
		"../bin/Debug/NetSparkle.DSAHelper.exe" /sign_update "../bin/Debug/NetSparkleUpdate.exe" NetSparkle_DSA.priv > hash-demo-file-debug.txt
	)
)