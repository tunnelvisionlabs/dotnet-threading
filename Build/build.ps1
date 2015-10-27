param (
	[switch]$Debug,
	[string]$VisualStudioVersion = "14.0",
	[switch]$NoDocs,
	[string]$Verbosity = "normal",
	[string]$Logger,
	[switch]$InstallSHFB
)

# build the solution
$SolutionPath = "..\Rackspace.Threading.sln"

# make sure the script was run from the expected path
if (!(Test-Path $SolutionPath)) {
	$host.ui.WriteErrorLine('The script was run from an invalid working directory.')
	exit 1
}

. .\version.ps1

If ($Debug) {
	$BuildConfig = 'Debug'
} Else {
	$BuildConfig = 'Release'
}

If ($Version.Contains('-')) {
	$KeyConfiguration = 'Dev'
} Else {
	$KeyConfiguration = 'Final'
}

If ($NoDocs -and -not $Debug) {
	$SolutionBuildConfig = $BuildConfig + 'NoDocs'
} Else {
	$SolutionBuildConfig = $BuildConfig
}

# build the main project
$nuget = '..\.nuget\NuGet.exe'

if ($VisualStudioVersion -eq '4.0') {
	$msbuild = "$env:windir\Microsoft.NET\Framework64\v4.0.30319\msbuild.exe"
} Else {
	$msbuild = "${env:ProgramFiles(x86)}\MSBuild\$VisualStudioVersion\Bin\MSBuild.exe"
}

&$nuget 'restore' $SolutionPath

If ($InstallSHFB) {
	# This is the NuGet package name for the SHFB package
	$SHFBPackageName = 'EWSoftware.SHFB'
	# This is the version according to the NuGet package itself
	$SHFBVersion = '2014.11.22.0'

	$SHFBPackagePath = "..\.shfb\$SHFBPackageName.$SHFBVersion.nupkg"
	If (-not (Test-Path $SHFBPackagePath)) {
		If (-not (Test-Path '..\.shfb')) {
			mkdir '..\.shfb'
		}

		# This is the release name on GitHub where the NuGet package is attached
		$SHFBRelease = 'v2014.11.22.0-beta'

		$SHFBInstallerSource = "https://github.com/tunnelvisionlabs/SHFB/releases/download/$SHFBRelease/$SHFBPackageName.$SHFBVersion.nupkg"
		Invoke-WebRequest $SHFBInstallerSource -OutFile $SHFBPackagePath
		If (-not $?) {
			$host.ui.WriteErrorLine('Failed to download the SHFB NuGet package')
			Exit $LASTEXITCODE
		}
	}

	$SHFBPackages = [System.IO.Path]::GetFullPath((Join-Path (pwd) '..\.shfb'))
	$SHFBPackagesUri = [System.Uri]$SHFBPackages
	Echo "$nuget 'install' 'EWSoftware.SHFB' -Version $SHFBVersion -OutputDirectory '..\packages' -Source $SHFBPackagesUri"
	&$nuget 'install' $SHFBPackageName -Version $SHFBVersion -OutputDirectory '..\packages' -Source $SHFBPackagesUri
	If (-not $?) {
		$host.ui.WriteErrorLine('Failed to install the SHFB NuGet package')
		Exit $LASTEXITCODE
	}

	$env:SHFBROOT = [System.IO.Path]::GetFullPath((Join-Path (pwd) "..\packages\$SHFBPackageName.$SHFBVersion\tools"))
}

If (-not $NoDocs) {
	If ((-not $env:SHFBROOT) -or (-not (Test-Path $env:SHFBROOT))) {
		$host.ui.WriteErrorLine('Could not locate Sandcastle Help File Builder')
		Exit 1
	}
}

If ($Logger) {
	$LoggerArgument = "/logger:$Logger"
}

&$msbuild '/nologo' '/m' '/nr:false' '/t:rebuild' $LoggerArgument "/verbosity:$Verbosity" "/p:Configuration=$SolutionBuildConfig" "/p:Platform=Any CPU" "/p:VisualStudioVersion=$VisualStudioVersion" "/p:KeyConfiguration=$KeyConfiguration" $SolutionPath
if (-not $?) {
	$host.ui.WriteErrorLine('Build failed, aborting!')
	exit $LASTEXITCODE
}

# By default, do not create a NuGet package unless the expected strong name key files were used
if (-not $SkipKeyCheck) {
	. .\keys.ps1

	foreach ($pair in $Keys.GetEnumerator()) {
		$assembly = Resolve-FullPath -Path "..\Rackspace.Threading\bin\$($pair.Key)\$BuildConfig\Rackspace.Threading.dll"
		# Run the actual check in a separate process or the current process will keep the assembly file locked
		powershell -Command ".\check-key.ps1 -Assembly '$assembly' -ExpectedKey '$($pair.Value)' -Build '$($pair.Key)'"
		if (-not $?) {
			Exit $LASTEXITCODE
		}
	}
}

if (-not (Test-Path 'nuget')) {
	mkdir "nuget"
}

# The NuGet packages reference XML documentation which is post-processed by SHFB. If the -NoDocs flag is specified,
# these files are not created so packaging will fail.
If (-not $NoDocs) {
	&$nuget 'pack' '..\Rackspace.Threading\TunnelVisionLabs.Threading.nuspec' '-OutputDirectory' 'nuget' '-Prop' "Configuration=$BuildConfig" '-Version' "$Version" '-Symbols'
	Exit $LASTEXITCODE
}
