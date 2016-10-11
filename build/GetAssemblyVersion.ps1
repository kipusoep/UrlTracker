$assemblyFile = ".\Properties\AssemblyInfo.cs"
$regularExpression = [regex] 'AssemblyVersion\(\"(.+)\"\)'

$fileContent = Get-Content $assemblyFile

foreach($content in $fileContent)
{
    $match = [System.Text.RegularExpressions.Regex]::Match($content, $RegularExpression)
    if($match.Success) {
        $match.groups[1].value.TrimEnd('*').TrimEnd('.')
    }
}