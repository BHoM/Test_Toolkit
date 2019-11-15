Param([string]$repo)

# **** READ DEPENDENCIES ARRAY ****
$depsFilePath = "$ENV:BUILD_SOURCESDIRECTORY\$repo\dependencies.txt"
$dependencies = gc $depsFilePath


# **** Constants ****
$msbuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"

git config --global user.email "BHoMBot@burohappold.com"
git config --global user.name "BHoMBot"


# **** Iterate over all dependencies ****
write-Output ("**** ITERATE OVER ALL DEPENDENCIES ****")
ForEach ($repo in $dependencies) 
{
  & "$ENV:BUILD_SOURCESDIRECTORY\Test_Toolkit\CloneAndBuildRepoWithMerge.ps1" -repo $repo
}