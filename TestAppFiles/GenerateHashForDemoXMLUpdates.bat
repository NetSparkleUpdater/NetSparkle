@echo off
if exist "../bin/Release/NetSparkle.DSAHelper.exe" (
    "../bin/Release/NetSparkle.DSAHelper.exe" /sign_update "appcast.xml" NetSparkle_DSA.priv > hash-demo-updates-file.txt
) else (
	if exist "../bin/Debug/NetSparkle.DSAHelper.exe" (
		"../bin/Debug/NetSparkle.DSAHelper.exe" /sign_update "appcast.xml" NetSparkle_DSA.priv > hash-demo-updates-file.txt
	)
)