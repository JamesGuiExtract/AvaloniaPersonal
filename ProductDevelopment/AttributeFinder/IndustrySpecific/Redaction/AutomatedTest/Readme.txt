Automated/Memory Leak tests for IIDSheildDataFileContentsCondition

As of this time (11/10/07) there is not an automated way to check results.  The current setup is for memory leak testing only, though results can be verified in an interactive fashion by veryifying files that get copied.

Current test framework:
- MemoryLeak_VOACondition1.bat launches the test
- "images" folder contains a bunch of "fake" image sets as a source for test data
- CopyNumberedSets.bat copies the complete set of files to "TestArea" once every ten seconds.
- VOAConditionSkipUnclassified.fps uses an add-triggered tif file provider on the TestArea folder, with an IIDSheildDataFileContentsCondition that uses the voa file contents as a basis.


Details:

* Images folder: Images folder contains .tif files and their assosiated .uss and .voa files. The tif and uss files have been gutted to prevent the test from consuming too much disk space, but the voa files are functional. (For reference, Original & valid tif and uss files are contained in images/RealImagegs).

* CopyNumberedSets.bat: Takes 4 parameters:
- source name = the folder name containing the source data set (I don't think quotes can be used)
- dest name = the folder name containing the source data set (I don't think quotes can be used)
- interval = the frequency in which the file set is copied to the destination directory
- run time = the approximate number of hours the process will run (the actual run time will be the supplied time + the time required to copy the files)

* VOAConditionSkipUnclassified.fps: Assumes "C:\MemoryLeak.dbcfg" and action "Test".  It will watch for tif files added to the TestArea folder, with a skip condition that skips unclassified documents.  For files that pass the skip condition, the voa file will be moved to sub-directory "NoUnclassified" while the assosiated tif and uss files will be deleted.

ToDo:

- Create more skip condition test cases.  I think testing a set of voa files with a good variety and distribution of attributes provides a scenario that is good enough to serve a sanity-check that there are no obvious memory leaks.  But ideally, I think more fps files should be set up to test the skip condition under different configurations (HDData, Clues, etc).

- Create automated test code to run within test harness.

- While an initial test I ran seemed to easily keep up with the set being supplied once every 5 seconds, for some reason my current tests seem to be queing the added files very slowly. For now I have reduced the supply interval to 10 seconds and will save further investigation for another time.