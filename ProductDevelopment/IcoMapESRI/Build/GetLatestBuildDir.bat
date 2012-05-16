@ECHO OFF

CALL InitUserEnv.Bat
CALL InitBuildEnv.Bat

ss get $/Engineering/ProductDevelopment/IcoMapESRI/Build/*.* -I- -W