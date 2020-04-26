param ([string]$build, [string]$revision)
$scriptDir = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$rootDir = Split-Path -Path $scriptDir -Parent
$targFile = "$rootDir\common\ProductInfo.cs"
(Get-Content $targFile) `
    -replace '(?<=\[assembly: AssemblyVersion\(\"\d+\.\d+\.).*(?=\"\)])', "$build.$revision" `
    -replace '(?<=\[assembly: AssemblyFileVersion\(\"\d+\.\d+\.).*(?=\"\)])', "$build.$revision" |
Out-File $targFile