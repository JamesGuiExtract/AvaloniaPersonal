;;; This script requires AutoHotKey available at http://www.autohotkey.com/
;;; 
;;; Turn on ScrollLock to pause the script (it will finish the current image first)

SetTitleMatchMode, 2
DetectHiddenText, on

Loop,
{
;pause script if scroll lock is on
Gosub, PauseIfScrollLock

;Resize LabDE window
	WinWait, LabDE,
	IfWinNotActive, LabDE,,
        WinActivate, LabDE,
	Winmove, LabDE, , 1, 1, 1000, 600
;;;Image 1 ;;;
;verify first image and save
	WinWait, - 350,
	IfWinNotActive, - 350, ,
        WinActivate, - 350,
	;;WinWaitActive, - 350,
	Sleep, 400
	Send, {TAB}
	Sleep, 400
	Send, {SPACE} ;;activate "Copy" button
	Sleep, 400
	
	;;Copy the patient name into the EpicCare table
	MouseClick, right, 35, 280, 1
	Sleep, 400
	MouseClick, left, 50, 290, 1
	Sleep, 400
	MouseClick, right, 35, 350, 1
	Sleep, 400
	MouseClick, left, 50, 400, 1
	Sleep, 400

	;;Type in MR number
	Send, {TAB 4}
	Sleep, 400
	Send, Monkeypants
	Sleep, 400

	;;Enter ordering physician code
	Send, {F3}
	Sleep, 400
	Send, 3085
	Sleep, 400

	;;Delete orders
	Send, {TAB 2}
	Sleep, 400
	Send, ^a
	Sleep, 400
	Send, {DELETE}
	Sleep, 400

	;;Swipe in order
	Send, !s
	MouseClick, left, 625, 237, , , d
	Sleep, 400
	MouseClick, left, 916, 280, , , u
	Sleep, 10000

	;;Deal with remaining invalid data
	Send, {F3}
	Sleep, 400
	Send, 01/01/1901
	Sleep, 400
	Send, {F3}
	Sleep, 400
	Send, 12:00
	Sleep, 400
	Send, {F3}
	Sleep, 400
	Send, Turtles
	Sleep, 400
	Send, {F3}
	Sleep, 400
	Send, Q

	;;Save
	Send, ^s
	Sleep, 400
Gosub, PauseIfScrollLock

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
