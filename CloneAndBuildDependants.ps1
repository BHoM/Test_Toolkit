Param([string]$repo)

# **** READ DEPENDANTS ARRAY ****
$dependantFilePath = "$ENV:BUILD_SOURCESDIRECTORY\$repo\dependants.txt"
$dependants = gc $dependantFilePath


# **** Constants ****
$msbuildPath = "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe"

git config --global user.email "BHoMBot@burohappold.com"
git config --global user.name "BHoMBot"

# **** Iterate over all Toolkits to Include ****
write-Output ("**** ITERATE OVER ALL DEPENDANTS  ****")
ForEach ($repo in $dependants) {
  & "$ENV:BUILD_SOURCESDIRECTORY\Test_Toolkit\CloneAndBuildRepoWithMerge.ps1" -repo $repo
}