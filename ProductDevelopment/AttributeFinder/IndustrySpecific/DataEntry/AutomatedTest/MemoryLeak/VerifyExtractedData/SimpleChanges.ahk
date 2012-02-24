SetTitleMatchMode, 2
Sleep, 5000
Loop
{
WinWait, - FLEX Index
IfWinNotActive, - FLEX Index, , - FLEX Index
WinWaitActive, - FLEX Index
Sleep, 1000
IfWinNotActive, - FLEX Index, , - FLEX Index
WinWaitActive, - FLEX Index
Send, {TAB}{TAB}{DOWN}
Sleep, 100
Send, {UP}
Sleep, 100
Send, {TAB}{TAB}{TAB}{SHIFTDOWN}m{SHIFTUP}adison
Sleep, 100
Send, {TAB}{SHIFTDOWN}wi{SHIFTUP}{TAB}537
Sleep, 100
Send, {TAB}{DEL}
Sleep, 100
Send, {CTRLDOWN}s{CTRLUP}
Sleep, 100
}