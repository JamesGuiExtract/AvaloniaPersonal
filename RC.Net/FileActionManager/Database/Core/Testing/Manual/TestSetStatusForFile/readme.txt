Performance test scripts for setFileActionState:
  - runSetStatusForFile_MultipleFileVersionTest.rsd measures the performance of the overload that is used by the DB Admin to set action status
  - runSetStatusForFile_SingleFileVersionTest.rsd measures performance of the overload that is used for FAM queuing/processing
  - To run a test, open the RSD file with RuleTester.exe and press play
  - The test time result(s) will be output in the value of attributes shown in the RuleTester window
  - An RDT license is required to run rules in the RuleTester but an SDK license isn't necessary
