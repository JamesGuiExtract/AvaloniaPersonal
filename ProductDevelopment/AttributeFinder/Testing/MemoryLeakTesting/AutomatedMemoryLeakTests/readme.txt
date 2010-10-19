The .bat files require that ID Shield, the RDT, and both SQL and the management
studio be installed.

1. Copy the AutomatedMemoryLeakTests folder to a location on your test machine.
   Make sure the files are not read-only.
2. Confirm that the initialfiles subfolder contains schema-appropriate
   Memory_Leak database files with actions Test and Test2 and counter Test.
   There should be appropriate .mdf and .ldf files on I: in the product testing
   folder for the relevant version (8.0+).
3. Copy the desired .sdf file from initialfiles to its parent folder.
4. Run initialize.bat; configure email settings.
5. Set up the FAM service to run as administrator and start manually.
6. Open runtest.bat and fill in appropriate values for processing time and
   email recipients.
7. Add a scheduled task that will run runtest.bat on startup; configure it to
   run as administrator.
8. Reboot.