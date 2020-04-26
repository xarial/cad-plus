param ([string]$version)
$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$rootDir = Split-Path -Path $scriptDir -Parent
$targFile = "$rootDir\common\ProductInfo.cs"
(Get-Content $targFile) `
    -replace '(?<=\[assembly: AssemblyVersion\(\"\d+\.\d+\.).*(?=\"\)])', "$version" `
    -replace '(?<=\[assembly: AssemblyFileVersion\(\"\d+\.\d+\.).*(?=\"\)])', "$version" |
Out-File $targFile