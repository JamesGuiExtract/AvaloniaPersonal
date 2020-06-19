$consoleRunner = $args[0]
$dllDir = $args[1]
$otherParams = $args[2..($args.length-1)]

$testDLLs = @(Get-ChildItem "$dllDir\*.Test.dll" | %{ '"' + $_.FullName + '"' })
$params = $testDLLs + $otherParams

& $consoleRunner $params
