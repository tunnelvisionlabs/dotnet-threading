. .\version.ps1

If ($Version.EndsWith('-dev')) {
	Write-Host "Cannot push development version '$Version' to NuGet."
	Exit 1
}

..\.nuget\NuGet.exe 'push' ".\nuget\Rackspace.Threading.$Version.nupkg"
