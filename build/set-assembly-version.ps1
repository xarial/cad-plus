param ([string]$version)
$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$rootDir = Split-Path -Path $scriptDir -Parent
$targFile = "$rootDir\common\ProductInfo.cs"
(Get-Content $targFile) `
    -replace '(?<=\[assembly: AssemblyVersion\(\").*(?=\"\)])', "$version" `
    -replace '(?<=\[assembly: AssemblyFileVersion\(\").*(?=\"\)])', "$version" |
Out-File $targFile