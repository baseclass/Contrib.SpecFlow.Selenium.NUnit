param($installPath, $toolsPath, $package, $project)

$projectFullName = $project.FullName
$directoryName = [System.IO.Path]::GetDirectoryName($projectFullName)
$targetDir = Join-Path $directoryName "Baseclass.Contrib.SpecFlow.Selenium.NUnit"

del (Join-Path $targetDir "Baseclass.Contrib.SpecFlow.Selenium.NUnit.SpecFlowPlugin.dll")

if((gci $targetDir).Count -eq 0)
{
	del $targetDir
}