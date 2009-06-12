@echo off
del /S .\testFiles\*.out
del /S /Q .\testFiles\folder1\*.*
del /S /Q .\testFiles\folder2\*.*
rd .\testFiles\folder1\folder1_1
rd .\testFiles\folder1\folder1_2
rd .\testFiles\folder1
rd .\testFiles\folder2
