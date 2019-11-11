Param([string]$repo)

Write-Output("Getting difference to master")

$cwd = Get-Location

Set-Location "$ENV:BUILD_SOURCESDIRECTORY\$repo"

$diffFiles = $(git diff origin/master --name-only)

$projectName = $ENV:PROJECTNAME
$parts = $projectName.split("_")
$project = $parts[0]

$pathOM = "$ENV:BUILD_SOURCESDIRECTORY\PRTestFiles\" + $projectName + "\" + $project + "_oM"
$pathEngine = "$ENV:BUILD_SOURCESDIRECTORY\PRTestFiles\" + $projectName + "\" + $project + "_Engine"
$pathAdatper = "$ENV:BUILD_SOURCESDIRECTORY\PRTestFiles\" + $projectName + "\" + $project+ "_Adapter"

If(!(test-path $pathOM))
{
	New-Item -ItemType Directory -Force -Path $pathOM
}

If(!(test-path $pathEngine))
{
	New-Item -ItemType Directory -Force -Path $pathEngine
}

If(!(test-path $pathAdatper))
{
	New-Item -ItemType Directory -Force -Path $pathAdatper
}

Foreach($diff in $diffFiles)
{
	If($diff -like "*_oM*")
	{
		Write-Output("Copying " + $diff + " to " + $pathOM)
		Copy-Item $diff -Destination $pathOM
	}

	If($diff -like "*_Engine*")
	{
		Write-Output("Copying " + $diff + " to " + $pathEngine)
		Copy-Item $diff -Destination $pathEngine
	}

	If($diff -like "*_Adapter*")
	{
		Write-Output("Copying " + $diff + " to " + $pathAdapter)
		Copy-Item $diff -Destination $pathAdapter
	}
}