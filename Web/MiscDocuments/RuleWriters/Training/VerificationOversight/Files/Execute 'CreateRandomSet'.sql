--Create Random Set for verification oversight. Will set specified percentage of files to pending
--for each verifier who worked on the specified action during the range of dates.
--Need to edit almost all parameters to suit the current project.
--	@ActionID = ID of ORIGINAL verification action in database (look in Action table)
--	@startDate = verification start date
--	@endDate = verification end date
--	@ActionNameToUpdate = Review action in database (this action must exist in database!)

DECLARE	@return_value int

EXEC	@return_value = [dbo].[sp_CreateRandomSet]
		@ActionID = 2,
		@startDate = N'8/19/2008',
		@endDate = N'9/16/2008',
		@ActionNameToUpdate = N'Review',
		@PercentOfFiles = 5

SELECT	'Return Value' = @return_value

GO
