Verification memory leak test instructions:

These tests use autohotkey scripts. Autohotkey is available here:
http://www.autohotkey.com/download/

To run the tests, first start the appropriate .ahk script, then run the related 
batch file. Watch the test for a while to confirm that the script is timed well 
for your machine and adjust the timing in the script if necessary. Note that AHK
sometimes behaves weirdly with remote desktop; sometimes if a remote desktop window
is minimized, the script stops, for instance.

Tests 1, 2, and 3 can be run in a standard memory leak database.
Test 4 requires that Input Event Tracking be enabled in the DBInfo table.
Test 5 requires that the database have an action named "Redact".
Test 6 requires the database to have a tag "Tag".