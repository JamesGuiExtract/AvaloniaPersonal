cd C:\Engineering\RC.Net\APIs\NUnit.ConsoleRunner.3.11.1\tools
.\nunit3-console.exe (ls -r C:\Engineering\Binaries\Debug\*.Test.dll | % FullName | sort-object -Unique)(ls -r *\bin\Debug\*.Tests.dll | % FullName | sort-object -Unique) --where:cat!=Interactive
Write-Host "Tests have finsihed running, you can find the output of the tests in C:\Engineering\RC.Net\APIs\NUnit.ConsoleRunner.3.11.1\tools\TestResults.xml"
Read-Host -Prompt "Press Enter to exit"