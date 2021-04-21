@echo off
if exist "../bin/Debug/NetSparkle.Tools.DSAHelper/netcoreapp3.0/NetSparkle.DSAHelper.exe" (
    "../bin/Debug/NetSparkle.Tools.DSAHelper/netcoreapp3.0/NetSparkle.DSAHelper.exe" /sign_update "appcast.xml" NetSparkle_DSA.priv > appcast.xml.dsa
	echo appcast.xml.dsa generated
) else (
	if exist "../bin/Debug/NetSparkle.Tools.DSAHelper/netcoreapp3.0/NetSparkle.DSAHelper.exe" (
		"../bin/Debug/NetSparkle.Tools.DSAHelper/netcoreapp3.0/NetSparkle.DSAHelper.exe" /sign_update "appcast.xml" NetSparkle_DSA.priv > appcast.xml.dsa
		echo appcast.xml.dsa generated
	) else (
		echo Error: cannot find NetSparkle.DSAHelper.exe
	)
)