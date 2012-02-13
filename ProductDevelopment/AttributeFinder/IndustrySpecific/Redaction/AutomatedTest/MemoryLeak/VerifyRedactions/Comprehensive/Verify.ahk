SetTitleMatchMode, 2
SetKeyDelay, 300
SetMouseDelay, 50
Sleep, 5000
Loop
{
	; Verify next 5 documents
	Loop, 5
	{
	WinWait, - ID Shield Verification
	IfWinNotActive, - ID Shield Verification, , - ID Shield Verification
	WinWaitActive, - ID Shield Verification

	; Delete existing redactions
	Sleep, 2000
	MouseClick, left,  825,  54
	Send, {DEL}{ENTER}
	MouseClick, left,  1000,  300
	Send, {TAB}{DEL}{ENTER}

	WinWait, - ID Shield Verification
	IfWinNotActive, - ID Shield Verification, , - ID Shield Verification
	WinWaitActive, - ID Shield Verification
	Sleep, 1000

	; Add new redactions with the word highlight tool.
	MouseClick, left,  747,  299
	MouseClick, left,  811,  303
	MouseClick, left,  644,  56
	MouseClick, left,  596,  175
	Send, ts{ENTER}{SHIFTDOWN}{TAB}{SHIFTUP}tt{ENTER}

	; Go to full screen mode and show/hide thumbnail and magnifier panes
	Send, {F11}
	Sleep, 2000
	Send, {F12}{F10}{F12}{F10}{F11}
	Sleep, 3000

	; Add exemption codes
	Send, e
	IfWinNotActive, Exemption codes, , WinActivate, Exemption codes, 
	WinWaitActive, Exemption codes, 
	MouseClick, left,  28,  129
	Sleep, 100
	MouseClick, left,  31,  147
	Sleep, 100
	MouseClick, left,  310,  468
	Sleep, 100

	; Add a tag
	MouseClick, left,  319,  98
	MouseClick, left,  319,  98
	Sleep, 500
	MouseClick, left,  345,  131
	Sleep, 500
	Send, {ESC}

	Send, {CTRLDOWN}f{CTRLUP}
	WinWait, Find or redact - Find or redact text, 
	IfWinNotActive, Find or redact - Find or redact text, , WinActivate, Find or redact - Find or redact text, 
	WinWaitActive, Find or redact - Find or redact text, 
	Send, {CTRLDOWN}{SHIFTDOWN}{Left}{SHIFTUP}{CTRLUP}{DEL}deed
	MouseClick, left,  319,  376
	Sleep, 500
	WinWait, Find or redact - Find or redact text, 
	IfWinNotActive, Find or redact - Find or redact text, , WinActivate, Find or redact - Find or redact text, 
	WinWaitActive, Find or redact - Find or redact text, 
	MouseClick, left,  400,  377
	Sleep, 100

	; Save document
	Send, {CTRLDOWN}s{CTRLUP}
	Sleep, 300
	}

	; Run the slideshow for a couple documents
	Send, {F5}
	Sleep, 10000

	; Back up 5 documents
	Send, {PGUP}{PGUP}{PGUP}{PGUP}{PGUP}

	; Delete a redaction on the next two documents
	Loop, 2
	{
	WinWait, - ID Shield Verification
	IfWinNotActive, - ID Shield Verification, , - ID Shield Verification
	WinWaitActive, - ID Shield Verification
	MouseClick, left,  447,  18
	MouseClick, left,  594,  152
	Send, {DEL}{ENTER}{TAB}{TAB}{LEFT}{ENTER}
	}

	; Advance back to the current document
	Send, {PGDN}{PGDN}{PGDN}
}