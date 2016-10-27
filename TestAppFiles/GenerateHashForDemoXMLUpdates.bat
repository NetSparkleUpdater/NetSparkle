@echo off
if exist "../bin/Release/NetSparkle.DSAHelper.exe" (
    "../bin/Release/NetSparkle.DSAHelper.exe" /sign_update "appcast.xml" NetSparkle_DSA.priv > appcast.xml.dsa
) else (
	if exist "../bin/Debug/NetSparkle.DSAHelper.exe" (
		"../bin/Debug/NetSparkle.DSAHelper.exe" /sign_update "appcast.xml" NetSparkle_DSA.priv > appcast.xml.dsa
	)
)