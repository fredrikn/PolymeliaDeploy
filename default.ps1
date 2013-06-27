properties {
	$Version = "0.1"
	$Configuration = "Release"
}

task default -depends Clean,Compile,Test

task Clean {
	Invoke-MSBuild "Clean"
}

task Compile {
	Invoke-MSBuild "Build"
}

task Test -depends Install-NUnitRunners {
	Invoke-NUnit ".\src\PolymeliaDeploy.Tests\bin\$Configuration\PolymeliaDeploy.Tests.dll"
}

task Install-NUnitRunners {
	Exec { .\src\.nuget\NuGet.exe install "NUnit.Runners" -version "2.6.2" -o ".\src\packages" }
}

function Invoke-MSBuild($Target) {
	Exec { msbuild /p:Configuration=$Configuration /t:$Target ".\src\PolymeliaDeploy.sln" }
}

function Invoke-NUnit
{
param(
	[string]$InputFile,
	[string[]]$Include,
	[string[]]$Exclude,
	[switch]$NoResult
)

	$options = @()

	$options += $InputFile

	if ($NoResult) {
		$options += "/noresult"
	} else {
		$fileName = [System.IO.Path]::GetFileNameWithoutExtension($InputFile)
		$options += "/result=.\TestResults\${fileName}.xml"
	}
	if ($Include) {
		$options += ("/include=" + [string]::Join(",", $Include))
	}
	if ($Exclude) {
		$options += ("/exclude=" + [string]::Join(",", $Exclude))
	}

	Exec { & ".\src\packages\NUnit.Runners.2.6.2\tools\nunit-console.exe" $options }
}
