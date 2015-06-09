param($installPath, $toolsPath, $package, $project)

$lmdb32 = $project.ProjectItems.Item("lmdb32.dll")
$lmdb64 = $project.ProjectItems.Item("lmdb64.dll")

# set 'Copy To Output Directory' to 'Copy if newer'
$copyToOutput32 = $lmdb32.Properties.Item("CopyToOutputDirectory")
$copyToOutput32.Value = 2

$copyToOutput64 = $lmdb64.Properties.Item("CopyToOutputDirectory")
$copyToOutput64.Value = 2