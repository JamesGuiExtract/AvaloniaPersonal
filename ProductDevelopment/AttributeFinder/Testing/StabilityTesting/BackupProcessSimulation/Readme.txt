The files here allow for testing queuing (with force-processing) and processing where files are being both rapidly added and modified.

This is a good excercise of:
- QueuedActionStatusChange logic (LegacyRCAndUtils #6242)
- FAM Processing Log grids when run as FAM instead of service. (FlexIDSCore #5185-5187, 5193, 5194)
- Database integrity with concurrent set file action operations across multiple processes. (LegacyRCAndUtils #6350)

To test:
1) Copy this directory to D:\BackupProcessSimulation.
2) Create a local FAM DB named "BackupSimulation" and create "Action1" and "Action2"
3) Start the FPS files
4) Start "CreateCopy.vbs - Run" and "Modify.vbs - Run"

NOTE: It is expected that this test will result in periodic "Error reading file size" when queuing. This is because it is possible in this test for a file to be modified, but then deleted before the modified queuing event has a chance to be processed. It is not expected that there should be any other type of error or logged exceptions during this test.