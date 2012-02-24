SetTitleMatchMode, 2
Sleep, 5000
Loop
{
WinWait, - FLEX Index
IfWinNotActive, - FLEX Index, , - FLEX Index
WinWaitActive, - FLEX Index
Send, {CTRLDOWN}s{CTRLUP}
Sleep, 100
}