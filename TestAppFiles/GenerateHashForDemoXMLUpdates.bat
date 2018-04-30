@echo off
if exist "../bin/Release/DSAHelper/NetSparkle.DSAHelper.exe" (
    "../bin/Release/DSAHelper/NetSparkle.DSAHelper.exe" /sign_update "appcast.xml" NetSparkle_DSA.priv > appcast.xml.dsa
) else (
	if exist "../bin/Debug/DSAHelper/NetSparkle.DSAHelper.exe" (
		"../bin/Debug/DSAHelper/NetSparkle.DSAHelper.exe" /sign_update "appcast.xml" NetSparkle_DSA.priv > appcast.xml.dsa
	)
)