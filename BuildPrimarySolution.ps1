Param([string]$repo)

$msbuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"

# **** Building primary solution ****
$repo = $ENV:PROJECTNAME

# **** Defining Paths ****
$slnPath = "$ENV:BUILD_SOURCESDIRECTORY\$repo\$repo.sln"

# **** Building .sln ****
write-Output ("Building " + $repo + ".sln")
& $msbuildPath -nologo $slnPath /verbosity:minimal