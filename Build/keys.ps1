# Note: these values may only change during major release
$Keys = @{
	'net35-client' = 'd693b28384d1c375'
	'net40-client' = 'fd26f941b2df1ee3'
	'net45' = 'fd26f941b2df1ee3'
	'netcore45' = 'fd26f941b2df1ee3'
	'portable-net40' = 'fd26f941b2df1ee3'
	'portable-net45' = 'fd26f941b2df1ee3'
}

function Resolve-FullPath() {
	param([string]$Path)
	[System.IO.Path]::GetFullPath((Join-Path (pwd) $Path))
}
