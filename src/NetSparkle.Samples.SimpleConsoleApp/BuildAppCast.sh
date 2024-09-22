
outputDir="appcast-output"
rm -rf "${outputDir}" # remove any old files
mkdir -p $outputDir
for config in "osx-arm64" "osx-x64" "win-arm64" "win-x64" "linux-x64"; do
    if [[ "$config" == osx* ]]; then
        ext=""
        if [[ "$config" == "osx-arm64" ]]; then
            os="macos-arm64"
            version=2.0 # since app cast gen doesn't like items with same version right now
        else
            os="macos-x64"
            version=2.1
        fi
        aot="true"
        singlefile="false"
    elif [[ "$config" == win* ]]; then
        ext=".exe"
        if [[ "$config" == "win-arm64" ]]; then
            os="windows-arm64"
            version=2.2
        else
            os="windows-x64"
            version=2.3
        fi
        aot="false"
        singlefile="true"
    else
        ext=""
        version=2.4
        os="linux-x64"
        aot="false"
        singlefile="true"
    fi
    filename="${outputDir}/${config}/SimpleNetSparkleConsoleApp${ext}"
    filenameNxt="${outputDir}/${config}/SimpleNetSparkleConsoleApp_${os}${ext}"
    dotnet publish -c Release --self-contained -r "${config}" -o "${outputDir}/${config}" "-p:PublishAot=${aot}" "-p:PublishSingleFile=${singlefile}"
    mv "${filename}" "${filenameNxt}"
    netsparkle-generate-appcast -n "Sparkle Console App" --os "${os}" --appcast-output-directory "${outputDir}" --single-file "${filenameNxt}" --reparse-existing --file-version "${version}" -u "https://netsparkleupdater.github.io/NetSparkle/files/console-sample-app"
    mv "${filenameNxt}" "${outputDir}/SimpleNetSparkleConsoleApp_${os}${ext}"
done 