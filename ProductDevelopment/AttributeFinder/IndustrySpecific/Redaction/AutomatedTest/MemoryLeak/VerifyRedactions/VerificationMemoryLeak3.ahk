;;; This script requires AutoHotKey available at http://www.autohotkey.com/
;;; 
;;; Turn on ScrollLock to pause the script (it will finish the current image first)

SetTitleMatchMode, 2
DetectHiddenText, on

;;;Image 1 ;;;
;turn off first redaction and save
	WinWaitActive, ID Shield, Current,
	Sleep, 400
	Send, {SPACE}
	Sleep, 400
	IfWinExist, state?, sure,
	{
		WinWaitActive, state?, sure,
		Send,{SPACE}
	}
	WinWaitActive, - ID Shield, Current,
;apply an exemption code to all redactions
	Send, ^a
	Sleep, 400
	Send, e
	Sleep, 400
	Send, {TAB}
	Sleep, 400
	Send, {SPACE}
	Sleep, 400
	Send, {ENTER}
	Sleep, 400
;save
	Gosub, TabUntilSave
	Send, {ENTER}
	Sleep, 400
Gosub, PauseIfScrollLock
Gosub, CheckForError

Loop,
{
;pause script if scroll lock is on
Gosub, PauseIfScrollLock
Gosub, CheckForError

;;;Image 1 ;;;
;turn off first redaction and save
	WinWaitActive, ID Shield, Current,
	Sleep, 400
	Send, {SPACE}
	Sleep, 400
	IfWinExist, state?, sure,
	{
		WinWaitActive, state?, sure,
		Send,{SPACE}
	}
	WinWaitActive, - ID Shield, Current,
	Gosub, ApplyExemptionCodes
	Gosub, TabUntilSave
	Send, {ENTER}
	Sleep, 400
Gosub, PauseIfScrollLock
Gosub, CheckForError

;;;Image 2 ;;;
;save
	WinWaitActive, - ID Shield, Current,
	Gosub, ApplyExemptionCodes
	Gosub, TabUntilSave
	Send, {ENTER}
	Sleep, 400
Gosub, PauseIfScrollLock
Gosub, CheckForError

; Image 3 ;;;
; save
	WinWaitActive, - ID Shield, Current,
	Gosub, ApplyExemptionCodes
	Gosub, TabUntilSave
	Send, {ENTER}
	Sleep, 400
Gosub, PauseIfScrollLock
Gosub, CheckForError

; go back and verify Image 3 again
	WinWaitActive, - ID Shield, Current,
; click previous document button
	Send, ^+{TAB}
	Sleep, 400
	WinWaitActive, - ID Shield, Current,
	Send, h
	Sleep, 400
	Send, {PgDn}
	Sleep, 400
; make redaction
	MouseClick, left,  650,  350, 1, ,D
	Sleep, 400
	WinWaitActive, - ID Shield, Current, 
	MouseClick, left,  950,  550, 1, ,U
	Sleep, 400
	Send, ^s
	Sleep, 400
Gosub, PauseIfScrollLock
Gosub, CheckForError

; Image 4 ;;;
; draw 2 redactions, turn second redaction off, save
	WinWaitActive, - ID Shield, Current,
; make redaction
	Send, h
	Sleep, 400
	MouseClick, left,  650,  350, 1, ,D
	Sleep, 400
	WinWaitActive, - ID Shield, Current, 
	MouseClick, left,  950,  550, 1, ,U
	Sleep, 400
; make second redaction
	Send, h
	Sleep, 400
	MouseClick, left,  650,  600, 1, ,D
	Sleep, 400
	WinWaitActive, - ID Shield, Current, 
	MouseClick, left,  950,  700, 1, ,U
	Sleep, 400
; select second redaction, turn it off
	Send, {ESC}
	Sleep, 400
	MouseClick, left, 750, 650, , , 
	Sleep, 400
	Send, {SPACE}
	Sleep, 400
	IfWinExist, state?, sure,
	{
		WinWaitActive, state?, sure,
		Send,{SPACE}
	}
	Gosub, ApplyExemptionCodes
; save
	Gosub, TabUntilSave
	Send, {ENTER}
	Sleep, 400
Gosub, PauseIfScrollLock
Gosub, CheckForError


;;; Image 5 ;;;
; autosplit redaction, save
	WinWaitActive, - ID Shield, Current, 
	Send, h
; hold shift and make box
	Send, {SHIFTDOWN}
	MouseClick, left, 650,  350, 1, ,D ; mouse button down
	Sleep, 400
	MouseClick, left, 950,  550, 1, ,U         ; mouse button up
	Sleep, 400
	Send, {SHIFTUP}
	Sleep, 400
	Gosub, ApplyExemptionCodes
; save
	Gosub, TabUntilSave
	Send, {ENTER}
	Sleep, 400
Gosub, PauseIfScrollLock
Gosub, CheckForError
}

PauseIfScrollLock:
    Loop
    {
	GetKeyState, lockstate, ScrollLock, T
		If lockstate = D
           Sleep, 1000
        Else
            break
    }
return

TabUntilSave:
	Loop
	{
		Send, {Tab}
		Sleep, 400
		IfWinExist, Save document?
			break
		IfWinExist, Save changes
			break
	}
return

CheckForError:
	IfWinExist, Must, all pages
	{
		Send, {SPACE}
		Gosub, TabUntilSave
	}
	IfWinExist, Error, Details...
	{
		Send, {SPACE}
		Sleep, 400
	}
return

ApplyExemptionCodes:
	Send, ^a
	Sleep, 400
	Send, ^e
return