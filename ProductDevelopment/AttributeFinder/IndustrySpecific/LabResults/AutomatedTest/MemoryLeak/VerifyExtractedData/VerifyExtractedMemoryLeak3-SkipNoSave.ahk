;;; This script requires AutoHotKey available at http://www.autohotkey.com/
;;; 
;;; Turn on ScrollLock to pause the script (it will finish the current image first)

SetTitleMatchMode, 2
DetectHiddenText, on

; Text to search for in LabDE window title. This is a part of the image file name. eg. "350_x.tif" where
; 'x' is a number generated by CopyNumberedFiles.exe
LabDEWindowTitle := "350_"	

; Default time in milliseconds to wait for lag between mouse clicks/text entry
SleepTime := 400

Loop,
{
	;pause script if scroll lock is on
	Gosub, PauseIfScrollLock
	
	;Resize LabDE window
		WinWait, LabDE,
		IfWinNotActive, LabDE,,
			WinActivate, LabDE,
		Winmove, LabDE, , 1, 1, 1000, 600

	;; Wait for an image to be loaded for verification
	WinWait, % LabDEWindowTitle,
	IfWinNotActive, % LabDEWindowTitle, ,
		WinActivate, % LabDEWindowTitle,
	;;WinWaitActive, % LabDEWindowTitle,
	Sleep, SleepTime

	;Press "Copy" button
	ControlFocus WindowsForms10.BUTTON.app.0.3d893c1
	Sleep, SleepTime
	Send, {SPACE}
	Sleep, SleepTime
	
	;;Copy the patient name into the EpicCare table
	/*
	;changed to use hot keys for copy/paste - AJ 1.5.2009
	MouseClick, right, 35, 280, 1
	Sleep, SleepTime
	MouseClick, left, 50, 290, 1
	Sleep, SleepTime
	MouseClick, right, 35, 350, 1
	Sleep, SleepTime
	MouseClick, left, 50, 400, 1
	Sleep, SleepTime
	*/
	
	; Select name row From Lab Result:
	MouseClick, left, 35, 280, 1
	Sleep, SleepTime
	; Copy
	Send ^c
	Sleep, SleepTime
	; Select name row From EpicCare:
	MouseClick, left, 35, 350, 1		
	Sleep, SleepTime
	; Paste
	Send ^v
	Sleep, SleepTime
	
	;;Type in MR number
	; Select MR # edit box
	ControlFocus WindowsForms10.EDIT.app.0.3d893c10
	Sleep, SleepTime
	Send, M123456
	Sleep, SleepTime

	;;Enter ordering physician code
	Send, {F3}
	Sleep, SleepTime
	Send, 3085
	Sleep, SleepTime

	;;Delete orders
	Send, {TAB 2}
	Sleep, SleepTime
	Send, ^a
	Sleep, SleepTime
	Send, {DELETE}
	Sleep, SleepTime

	;;Swipe in order
	; Set image window to show entire page
	Send !p
	Sleep, SleepTime
	; Select the rectangular swipe tool
	Send, {Alt}
	Sleep, SleepTime
	Send t
	Sleep, SleepTime
	Send s
	Sleep, SleepTime
	;MouseClick, left, 625, 237, , , d
	MouseClick, left, 620, 180, , , d	;swipe bigger box in order to get Collection Date/Time
	Sleep, SleepTime
	MouseClick, left, 916, 280, , , u
	Sleep, 10000

	;;Deal with remaining invalid data
	
	if !(IsRemainingInvalid()){
		continue
	}
	
	;Send, {F3}
	; Select Result Date edit box
	ControlFocus WindowsForms10.EDIT.app.0.3d893c8
	Sleep, SleepTime
	; input a valid date
	Send, 01/01/1901
	Sleep, SleepTime
	
	if !(IsRemainingInvalid()){
		continue
	}
	
	; Select Result Time edit box
	;Send, {F3}
	ControlFocus WindowsForms10.EDIT.app.0.3d893c7
	Sleep, SleepTime
	; input a valid time
	Send, 12:00
	Sleep, SleepTime
	
	if !(IsRemainingInvalid()){
		continue
	}
	
	; Select Order Number edit box
	;Send, {F3}
	ControlFocus WindowsForms10.EDIT.app.0.3d893c3
	Sleep, SleepTime
	; input valid data
	Send, Order123456
	Sleep, SleepTime
	
	if !(IsRemainingInvalid()){
		continue
	}
	
	; Select Lab Code edit box
	;Send, {F3}
	ControlFocus WindowsForms10.EDIT.app.0.3d893c6
	Sleep, SleepTime
	Send {Space}
	Send {Enter}

	;;Save
	SaveAndContinue()
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

;; Check for invalid data items. If F3 is pressed with no invalid data remaining, a message box is presented.
IsRemainingInvalid()
{
	Send {F3}
	Sleep, SleepTime
	; If invalid items remain do nothing.
	IfWinNotExist, LabDE, There are no invalid, ,
	{
		return true
	}
	else
	; Otherwise, hit ok on message box (press enter) and save the document.
	{
		Send {Enter}
		Sleep, SleepTime
		SaveAndContinue()
		return false
	}
}

;; Skip the current document, check whether to pause the script, then move on.
SaveAndContinue()
{
	;MsgBox About to save ;debug only
	;Send, ^s	;skip document rather than save
	; Navigate to the File menu
	Send {Alt}
	Sleep, SleepTime
	Send f
	Sleep, SleepTime
	; Arrow down to 'Skip document'
	Send {Down 3}
	Sleep, SleepTime
	Send {Enter}
	Sleep, 800
	; Tell the dialogue not to save the work
	Send n
	Sleep, SleepTime
	Sleep, 2000
	Gosub, PauseIfScrollLock
}