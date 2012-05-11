The purpose of this test is to ensure a LabDE OrderMappingDB can be edited by SQLCDBEditor while being simulateously used by the DataEntry verification UI and by the OrderMapper output handler.

Setup:
1) To help guarantee as much simultaneous access of the master database as possible, a quadcore or greater should be used.
2) The folder must be placed on a the test machine such that the path is: "C:\DE_DB_SimultaneousUsage"
3) Install AutoHotkey (http://www.autohotkey.com, just the regular version, not AutoHotkey_L).
4) Run ExecuteTest.bat.

NOTE: When the test starts, after starting Process.fps and opening OrderMappingDB.sdf, it will wait 15 seconds for the rules to finish processing the first documents. Autohotkey will then begin to rapidly alternate between saving the DB and saving the next document presented in verification.

There is a small random wait built into both Process.fps and OrderMappingDB.sdf.vbs which gets executed when the database is saved to try to shake up the timing of when the rules, verification, and SQLCDBEditor access the master datbase file in relation to each other to ensure at some points multiple applications are accessing the file simultaneously.

If a restart of the test is necessary, be sure to exit the AutoHotkey script from the first test (via the system try) first.
