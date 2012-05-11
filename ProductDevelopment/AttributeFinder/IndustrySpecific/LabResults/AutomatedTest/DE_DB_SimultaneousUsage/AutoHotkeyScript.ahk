SetTitleMatchMode, 2

Sleep, 15000

Loop
{
    IfWinNotActive, SQLCDBEditor -,, WinActivate, SQLCDBEditor -, 
    WinWaitActive, SQLCDBEditor -,,2
    WinMaximize
    MouseClick, left,  700,  500
    Sleep, 100
    Send, abc{CTRLDOWN}z{CTRLUP}
    Sleep, 100
    Send, {ALTDOWN}fs{ALTUP}

    Random, rand, 1, 1000
    Sleep, rand

    IfWinNotActive, - LabDE (Process),, WinActivate, - LabDE (Process),
    WinWaitActive, - LabDE (Process),,2
    WinMaximize
    Send, {CTRLDOWN}s{CTRLUP}

    WinWait, Database save,,2
    IfWinNotActive, Database save,, WinActivate, Database save, 
    WinWaitActive, Database save,,2
    Send, {ENTER}
}