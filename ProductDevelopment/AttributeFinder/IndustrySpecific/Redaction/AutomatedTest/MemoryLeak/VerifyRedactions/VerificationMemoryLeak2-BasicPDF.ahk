;;; This script requires AutoHotKey available at http://www.autohotkey.com/
;;; 
;;; Turn on ScrollLock to pause the script (it will finish the current image first)

SetTitleMatchMode, 2
DetectHiddenText, on

;resize Extract Image Viewer window
	WinWait, Extract Image Viewer, 
    	WinActivate, Extract Image Viewer, 
	WinWaitActive, Extract Image Viewer, 
	Winmove, Extract Image Viewer, , 401, 1, 400, 600

;resize ID Shield window
	WinWait, ID Shield,
	WinActivate, ID Shield,
	WinWaitActive, ID Shield,
	Winmove, ID Shield, , 0, 1, 400, 600

;;;Image 1 ;;;
;turn off first redaction and save
	WinWait, - ID Shield, Current,
    	WinActivate, - ID Shield, Current,
	Sleep, 2000

Loop,
{
;pause script if scroll lock is on
Gosub, PauseIfScrollLock

;;;Image 1 ;;;
;turn off first redaction and save
	WinWait,ID Shield, Current,
	WinActivate,ID Shield, Current,
	Sleep, 2000
	Send, {SPACE}
	WinWait, Confirm Non-Redaction, , 2 
	WinActivate, Confirm Non-Redaction, 
	Send, {LEFT}{ENTER}
	Sleep, 1000
	WinWait, - ID Shield, Current,
	WinActivate, - ID Shield, Current,
	Send, {CTRLDOWN}s{CTRLUP}
	Sleep, 9000
Gosub, PauseIfScrollLock

;;;Image 2 ;;;
;save
	WinWait, - ID Shield, Current,
	WinActivate, - ID Shield, Current,
	WinWaitActive, - ID Shield, Current,
	Send, {CTRLDOWN}s{CTRLUP}
	Sleep, 9000
Gosub, PauseIfScrollLock

;;; Image 3 ;;;
;save
	WinWait, - ID Shield, Current,
	WinActivate, - ID Shield, Current,
	WinWaitActive, - ID Shield, Current,
	Send, {CTRLDOWN}s{CTRLUP}
	Sleep, 9000
Gosub, PauseIfScrollLock

;go back and verify Image 3 again
	WinWait, - ID Shield, Current,
	WinActivate, - ID Shield, Current,
	WinWaitActive, - ID Shield, Current,
;click previous document button
	Send, {CTRLDOWN}{SHIFTDOWN}{TAB}{SHIFTUP}{CTRLUP}
	Sleep, 9000
	WinWait, - ID Shield, Current,
	WinActivate, - ID Shield, Current,
	Send, h
;make redaction
	WinWait, - Extract Image Viewer, 
	WinActivate, - Extract Image Viewer, 
	MouseClickDrag, left, 200, 200, 317, 493
	Sleep, 1000
	Send, {CTRLDOWN}s{CTRLUP}
	Sleep, 9000
Gosub, PauseIfScrollLock

;;; Image 4 ;;;
;set any-angle tool height, make 2 redactions, turn 2nd redaction off, save
	WinWait, - Extract Image Viewer, 
	WinActivate, - Extract Image Viewer, 
;click set height button
	PostMessage, 0x111, 12010
;set height
	MouseClickDrag, left, 313, 81, 313, 123
;select any-angle tool
	MouseClick, left,  275,  35 ; highlight drop down box
	Sleep, 100
	MouseClick, left,  275,  60 ; marker
	Sleep, 1000
	MouseClickDrag, left, 160, 187, 260, 187
	Sleep, 2000
;select box tool
	MouseClick, left,  275,  35 ; highlight drop down box
	Sleep, 100
	MouseClick, left,  275,  80 ; box
;make redaction
	MouseClickDrag, left, 160, 387, 310, 487
	Sleep, 2000
;turn off redaction
	Send, {SPACE}
	WinWait, Confirm Non-Redaction, 
	WinActivate, Confirm Non-Redaction, 
	Send, {LEFT}{ENTER}
	Sleep, 2000
	WinWait, - Extract Image Viewer, 
	WinActivate, - Extract Image Viewer, 
;click on first redaction, move
	Send, {ESC}
	Sleep, 400
	MouseClickDrag, left, 200, 187, 100, 400
	Sleep, 400
	Send, {CTRLDOWN}s{CTRLUP}
	Sleep, 9000
GoSub, PauseIfScrollLock

;;; Image 5 ;;;
; autosplit redaction, save
	WinWait, - Extract Image Viewer, 
	WinActivate, - Extract Image Viewer, 
	Send, h
; hold ctrl and make box
	Send, {CTRLDOWN}
	MouseClickDrag, left, 160, 387, 310, 487
	Sleep, 2000
	Send, {CTRLUP}
	Sleep, 2000
	Send, {CTRLDOWN}s{CTRLUP}
	Sleep, 9000
}

PauseIfScrollLock:
    Loop
    {
	GetKeyState, lockstate, ScrollLock, T
		If lockstate = D
           Sleep, 2000
        Else
            break
    }
return
