$assemblyFile = ".\Properties\AssemblyInfo.cs"
$regularExpression = [regex] 'AssemblyVersion\(\"(.+)\"\)'

$fileContent = Get-Content $assemblyFile

foreach($content in $fileContent)
{
    $match = [System.Text.RegularExpressions.Regex]::Match($content, $RegularExpression)
    if($match.Success) {
        $nugetVersion = $match.groups[1].value.TrimEnd('*').TrimEnd('.')
		$nugetVersion
    }
}

if ($env:TEAMCITY_VERSION) {
    $host.UI.RawUI.BufferSize = New-Object System.Management.Automation.Host.Size(8192,50)
	"##teamcity[setParameter name='env.nugetVersion' value='$nugetVersion']"
}