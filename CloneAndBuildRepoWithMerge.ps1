Param([string]$repo)

# **** Constants ****
$msbuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"

$parts = $repo.split("/")
$org = $parts[0]
$repo = $parts[1]

$slnPath = "$ENV:BUILD_SOURCESDIRECTORY\$repo\$repo.sln"

If(test-path "$ENV:BUILD_SOURCESDIRECTORY\$repo")
{
	Write-Output ("Deleting folder if it exists before cloning...")
	Remove-Item -LiteralPath "$ENV:BUILD_SOURCESDIRECTORY\$repo" -Force -Recurse
}

# **** Cloning Repo ****
Write-Output ("Cloning " + $repo + " to " + $ENV:BUILD_SOURCESDIRECTORY + "\" + $repo)
git clone -q --branch=master https://github.com/$org/$repo.git $ENV:BUILD_SOURCESDIRECTORY\$repo

If(-not $?)
{
	Write-Error  "##vso[task.logissue type=error;]Failed '$repo'"
	return
}

# Switch branch in case there are dependant PRs happening...
$branch = $ENV:SYSTEM_PULLREQUEST_SOURCEBRANCH
If ($branch -ne "master")
{
	$cwd = Get-Location
	Set-Location $ENV:BUILD_SOURCESDIRECTORY\$repo

	If((git rev-parse --verify --quiet ("origin/"+$branch)).length -gt 0)
	{
		Write-Output("Merging branch " + $branch + " in repo " + $repo + " to master")
		git merge $branch
	}
	Else
	{
		Write-Output("Staying on master branch for " + $repo)
	}

	Set-Location $cwd
}
Else
{
	Write-Output("Staying on master branch for " + $repo)
}


# **** Restore NuGet ****
Write-Output ("Restoring NuGet packages for " + $repo)
& NuGet.exe restore $slnPath

# **** Building .sln ****
write-Output ("Building " + $repo + ".sln")
& $msbuildPath -nologo $slnPath /verbosity:minimal

If(-not $?)
{
	Write-Error  "##vso[task.logissue type=error;]Failed '$repo'"
	return
}