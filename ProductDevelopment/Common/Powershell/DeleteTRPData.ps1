& 'C:\Program Files (x86)\Extract Systems\CommonComponents\ExtractTRP2.exe' /exit

Remove-Item (Join-Path $env:AppData 'Windows\{EFF9AEFC-3046-48BC-84D1-E9862F9D1E22}\estrpmfc.dll') -Force

Remove-Item -Path 'HKCU:\Identities\{7FEF3749-A8CC-4CD0-9CEB-E6D267FA524E}'