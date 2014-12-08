# Note: these values may only change during major release

If ($Version.Contains('-')) {

	# Use the development keys
	$Keys = @{
		'net35-client' = 'bb62785d398726f0'
		'net40-client' = '2c5e0ae13bddb49e'
		'net45' = '2c5e0ae13bddb49e'
		'netcore45' = '2c5e0ae13bddb49e'
		'portable-net40' = '2c5e0ae13bddb49e'
		'portable-net45' = '2c5e0ae13bddb49e'
	}

} Else {

	# Use the final release keys
	$Keys = @{
		'net35-client' = 'd693b28384d1c375'
		'net40-client' = 'fd26f941b2df1ee3'
		'net45' = 'fd26f941b2df1ee3'
		'netcore45' = 'fd26f941b2df1ee3'
		'portable-net40' = 'fd26f941b2df1ee3'
		'portable-net45' = 'fd26f941b2df1ee3'
	}

}

function Resolve-FullPath() {
	param([string]$Path)
	[System.IO.Path]::GetFullPath((Join-Path (pwd) $Path))
}
