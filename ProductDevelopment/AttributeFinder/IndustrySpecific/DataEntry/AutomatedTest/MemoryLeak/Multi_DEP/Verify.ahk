SetTitleMatchMode, 2

Sleep, 10000

Loop
{
WinWait,- LabDE,,1
IfWinNotActive,- LabDE, , WinActivate,- LabDE, 
WinWaitActive,- LabDE,,1

Random, nextInput, 0, 60

If nextInput between 0 and 24
	SendInput {Tab}
If (nextInput = 25)
	SendInput +{Tab}
If (nextInput = 26)
	SendInput ^d
If (nextInput = 27)
	SendInput ^t
If (nextInput = 28)
	SendInput {Delete}
If (nextInput = 29)
	SendInput ^c
If (nextInput = 30)
	SendInput ^v
If (nextInput = 31)
	SendInput ^r
If (nextInput = 32)
	SendInput ^+r
If (nextInput = 33)
	SendInput ^z
If (nextInput = 34)
	SendInput ^y
; F3 and F4 not included since I was having difficulty making sure the script could dismiss the pop-ups that occurred if there were no missing/invalid items.
If (nextInput = 37)
	SendInput {F7}
If (nextInput = 38)
	SendInput {F8}
If (nextInput = 39)
	SendInput {F10}
If (nextInput = 40)
{
	SendInput {F11}
	Sleep, 500
	SendInput {F11}
}
If (nextInput = 41)
	SendInput {F12}
If (nextInput = 42)
	SendInput {PgDn}
If (nextInput = 43)
	SendInput {PgUp}
If (nextInput = 44)
	SendInput !p
If (nextInput = 45)
	SendInput !W
If (nextInput = 46)
	SendInput !r
If (nextInput = 47)
	SendInput !s
If (nextInput = 48)
	SendInput {Esc}
If (nextInput = 49)
	SendInput {Space}{Down}{Enter}
If nextInput between 50 and 51
	SendInput Extract Systems
If nextInput between 52 and 53
	SendInput 42
If nextInput between 54 and 55
	SendInput Coffee
If nextInput between 56 and 57
	SendInput {Space}
If (nextInput = 58)
{
	SendInput ^s
	Sleep, 3000
}

Sleep, 1000
}