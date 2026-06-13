param(
	[ValidateSet("Debug", "Release")]
	[string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$solutionRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectPath = Join-Path $solutionRoot "workspace\DHInfoDesk.SmokeTest\DHInfoDesk.SmokeTest.csproj"
$msbuildPath = "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
$exePath = Join-Path $solutionRoot "output\SmokeTest\$Configuration\DHInfoDesk.SmokeTest.exe"
$resultPath = Join-Path $solutionRoot "test-results\sysinfo-smoke-test.txt"

if ((Test-Path -LiteralPath $msbuildPath) -eq $false) {
	throw "MSBuild not found: $msbuildPath"
}

& $msbuildPath $projectPath /t:Rebuild /p:Configuration=$Configuration /p:Platform=AnyCPU /m
if ($LASTEXITCODE -ne 0) {
	exit $LASTEXITCODE
}

& $exePath $resultPath
exit $LASTEXITCODE
