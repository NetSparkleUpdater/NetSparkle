echo This file needs its paths fixed. TODO:
@echo off
if exist "../bin/Release/DSAHelper/NetSparkle.DSAHelper.exe" (
    "../bin/Release/DSAHelper/NetSparkle.DSAHelper.exe" /sign_update "2.0-release-notes.md" NetSparkle_DSA.priv > 2.0-release-notes.md.dsa
) else (
	if exist "../bin/Debug/DSAHelper/NetSparkle.DSAHelper.exe" (
		"../bin/Debug/DSAHelper/NetSparkle.DSAHelper.exe" /sign_update "2.0-release-notes.md" NetSparkle_DSA.priv > 2.0-release-notes.md.dsa
	)
)