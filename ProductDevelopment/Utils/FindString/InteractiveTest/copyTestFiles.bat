echo off
echo creating test folders and files
md .\testFiles\folder1
md .\testFiles\folder2
md .\testFiles\folder1\folder1_1
md .\testFiles\folder1\folder1_2

copy .\testFiles\*.uss .\testFiles\folder2
copy .\testFiles\*.txt .\testFiles\folder2
copy .\testFiles\*.uss .\testFiles\folder1\folder1_1
copy .\testFiles\*.txt .\testFiles\folder1\folder1_1
copy .\testFiles\*.uss .\testFiles\folder1\folder1_2
copy .\testFiles\*.txt .\testFiles\folder1\folder1_2
