This auto-hotkey memory leak test script advances through documents in a configuration with multiple DEPs and excercises the DEPs with random shortcut keys; Every second, a key or key combination will be pressed. About 50% of the time, the key will be tab, the rest of the time it will be one of most of the other shortcut keys or key combos that are meaningful for DE including Ctrl + S to advance to the next document.

This setup should not be dependent on specific versions of the software or sample DEPs.

Setup:
DB server: (local), DB Name: MEMORY_LEAK, DB Action: Multi_DEP_AutoHotkey
FKB Versions: 12.1.0.191 and 12.1.0.223

Run ExecuteTest.bat to start. You will need to enter the admin password for the DB when prompted.
The test will run indefinetly.