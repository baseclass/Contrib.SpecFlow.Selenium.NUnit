param($installPath, $toolsPath, $package, $project)

$projectFullName = $project.FullName
$directoryName = [System.IO.Path]::GetDirectoryName($projectFullName)
$targetDir = Join-Path $directoryName "Baseclass.Contrib.SpecFlow.Selenium.NUnit"

if((Test-Path $targetDir) -eq 0)
{
	mkdir $targetDir;
}

copy (Join-Path $installPath "\lib\net45\Baseclass.Contrib.SpecFlow.Selenium.NUnit.SpecFlowPlugin.dll") $targetDir