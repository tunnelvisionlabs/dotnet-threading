param (
	[switch]$Debug
)

# build the solution
$SolutionPath = "..\Rackspace.Threading.sln"

# make sure the script was run from the expected path
if (!(Test-Path $SolutionPath)) {
	echo "The script was run from an invalid working directory."
	exit 1
}

. .\version.ps1

If ($Debug) {
	$BuildConfig = 'Debug'
} Else {
	$BuildConfig = 'Release'
}

# build the main project
$msbuild = "$env:windir\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe"

&$msbuild '/nologo' '/m' '/nr:false' '/t:rebuild' "/p:Configuration=$BuildConfig" $SolutionPath
if ($LASTEXITCODE -ne 0) {
	echo "Build failed, aborting!"
	exit $p.ExitCode
}

if (-not (Test-Path 'nuget')) {
	mkdir "nuget"
}

..\.nuget\NuGet.exe pack ..\Rackspace.Threading\Rackspace.Threading.nuspec -OutputDirectory nuget -Prop Configuration=$BuildConfig -Version $Version -Symbols
