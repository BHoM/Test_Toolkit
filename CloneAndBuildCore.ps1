Param([string]$repo)

# **** READ DEPENDENCIES ARRAY ****
$coreFilePath = "$ENV:BUILD_SOURCESDIRECTORY\$repo\core.txt"
$cores = gc $coreFilePath


# **** Constants ****
$msbuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"

git config --global user.email "BHoMBot@burohappold.com"
git config --global user.name "BHoMBot"


# **** Iterate over all dependencies ****
write-Output ("**** ITERATE OVER ALL DEPENDENCIES ****")
ForEach ($repo in $cores) 
{
  & "$ENV:BUILD_SOURCESDIRECTORY\Test_Toolkit\CloneAndBuildRepo.ps1" -repo $repo
}