# Note: these values may only change during minor release
$Keys = @{
	'net35-client' = '8b3790928cb57ea0'
	'net40-client' = 'b3a60e8d525c0432'
	'net45' = 'c5149d599dccb791'
	'netcore45' = 'c0beeaaf4ff18c32'
	'portable-net40' = '165cb04bbaa60d42'
	'portable-net45' = '7c9e9383f8e98b3a'
}

function Resolve-FullPath() {
	param([string]$Path)
	[System.IO.Path]::GetFullPath((Join-Path (pwd) $Path))
}
