DB Requires:
	- Input tracking turned on.
	- "Verify" and "SecondAction" actions
	- At least one tag (Suggestion = "MemoryLeak" or one that is at least as many characters).

Setup:
	1) Delete VerificationForm.xml in C:\Users\[User]\AppData\Local\Extract Systems\ID Shield (or equivalent folder
	2) Open and run MemoryLeak.fps
	3) Maximize the verification window.
	4) In verification options, set the slideshow interval to 2 seconds.
	5) In verification options, make sure background OCR is enabled.
	6) Open the find & redact window; make sure it is at its minimum size.
	7) Close the find and redact and verification windows.

This tests:
	- Deleting/adding redactions
	- Use of exising OCR and Background OCR for word highlight tool
	- Use of Find and Redact
	- Use of and showing/hiding tumbnail and magnifier panes.
	- Use of slideshow
	- Changing to/from full screen mode
	- Navigating back and forth in history.
	- Applying exception codes
	- Applying tags
	- Applying an new action status from the verification window.
	- Input tracking.