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

$oMName = $project + "_oM"
$engineName = $project + "_Engine"
$adapterName = $project + "_Adapter"

Foreach($diff in $diffFiles)
{
	$folderPathParts = $diff.split("/")

	If($diff -like "*_oM*")
	{
		$index = [array]::indexof($folderPathParts, $oMName)
		$savePath = $pathOM
		For($x = $index + 1; $x -lt $folderPathParts.Count - 1; $x++)
		{
			$savePath = $savePath + "\" + $folderPathParts[$x]
		}
		If(!(test-path $savePath))
		{
			New-Item -ItemType Directory -Force -Path $savePath
		}

		Write-Output("Copying " + $diff + " to " + $savePath)
		Copy-Item $diff -Destination $savePath
	}

	If($diff -like "*_Engine*")
	{
		$index = [array]::indexof($folderPathParts, $engineName)
		$savePath = $pathEngine
		For($x = $index + 1; $x -lt $folderPathParts.Count - 1; $x++)
		{
			$savePath = $savePath + "\" + $folderPathParts[$x]
		}
		If(!(test-path $savePath))
		{
			New-Item -ItemType Directory -Force -Path $savePath
		}

		Write-Output("Copying " + $diff + " to " + $savePath)
		Copy-Item $diff -Destination $savePath
	}

	If($diff -like "*_Adapter*")
	{
		$index = [array]::indexof($folderPathParts, $adapterName)
		$savePath = $pathAdapter
		For($x = $index + 1; $x -lt $folderPathParts.Count - 1; $x++)
		{
			$savePath = $savePath + "\" + $folderPathParts[$x]
		}
		If(!(test-path $savePath))
		{
			New-Item -ItemType Directory -Force -Path $savePath
		}

		Write-Output("Copying " + $diff + " to " + $savePath)
		Copy-Item $diff -Destination $savePath
	}
}