:: Build all JS files
for /D %%f in (*) do (
  cd "%%f"
  BuildFinalScript.bat
  cd ..
)  
pause
