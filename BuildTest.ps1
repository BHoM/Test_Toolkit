Param([string]$repo)

$msbuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"

# **** Building BHoM Test_Toolkit ****
$repo = "XML_Toolkit"

Write-Output ("Cloning " + $repo + " to " + $ENV:BUILD_SOURCESDIRECTORY + "\" + $repo)
git clone -q --branch=master https://github.com/BHoM/$repo.git $ENV:BUILD_SOURCESDIRECTORY\$repo

If(-not $?)
{
	Write-Error  "##vso[task.logissue type=error;]Failed '$repo'"
	return
}

# **** Defining Paths ****
$slnPath = "$ENV:BUILD_SOURCESDIRECTORY\$repo\$repo.sln"

# **** Building .sln ****
write-Output ("Building " + $repo + ".sln")
& $msbuildPath -nologo $slnPath /verbosity:minimal

# **** Building BHoM Test_Toolkit ****
$repo = "Test_Toolkit"

# **** Defining Paths ****
$slnPath = "$ENV:BUILD_SOURCESDIRECTORY\$repo\$repo.sln"

# **** Building .sln ****
write-Output ("Building " + $repo + ".sln")
& $msbuildPath -nologo $slnPath /verbosity:minimal /p:Configuration="ComplianceTestBuild"