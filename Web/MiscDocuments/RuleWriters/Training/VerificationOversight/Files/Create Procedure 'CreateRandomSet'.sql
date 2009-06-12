/****** Object:  StoredProcedure [dbo].[sp_CreateRandomSet]    Script Date: 09/17/2008 11:06:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Object:  StoredProcedure [dbo].[sp_CreateRandomSet]    Script Date: 09/17/2008 11:04:03 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_CreateRandomSet]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_CreateRandomSet]
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_CreateRandomSet]
	@ActionID as Int,
	@startDate as datetime,
	@endDate as datetime,
	@ActionNameToUpdate as nvarchar(50),
	@PercentOfFiles as Int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @FileID as int

	DECLARE @Random as real
	DECLARE @ActionColumnName as nvarchar(50)
	DECLARE @NewValue as nvarchar(1)
	DECLARE @SetPendingCount as int
	
	SELECT @SetPendingCount = 0

	SELECT @ActionColumnName = 'ASC_' + @ActionNameToUpdate

	DECLARE FilesProcessedCursor CURSOR FOR
		SELECT FileID FROM FileActionStateTransition 
			WHERE ActionID = @ActionID AND ASC_To = 'C' AND
			DateTimeStamp >= @startDate AND DateTimeStamp <= @endDate
	

	OPEN FilesProcessedCursor
	FETCH NEXT FROM FilesProcessedCursor INTO @FileID

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SELECT @Random = RAND() * 100
		SELECT @NewValue = CASE WHEN @Random <= @PercentOfFiles THEN 'P' ELSE 'U' END
		Select @SetPendingCount = CASE WHEN @NewValue = 'P' THEN @SetPendingCount +1 ELSE @SetPendingCount END
		
		EXEC(' UPDATE FAMFile SET ' + @ActionColumnName + '= CASE  WHEN ' + @Random + 
				' <= ' + @PercentOfFiles + ' THEN ''P'' ELSE ''U'' END WHERE ID = ' +  @FileID )
		FETCH NEXT FROM FilesProcessedCursor INTO @FileID
	END

	CLOSE FilesProcessedCursor
	DEALLOCATE FilesProcessedCursor

	EXEC('SELECT     ' + @ActionColumnName + ', COUNT(' + @ActionColumnName + ') AS NumberOfFiles, ' +
		'				FAMUser.UserName, Machine.MachineName ' +
		' FROM         FAMFile INNER JOIN ' +
		'			  FileActionStateTransition ON FAMFile.ID = FileActionStateTransition.FileID INNER JOIN ' +
        '			  FAMUser ON FileActionStateTransition.FAMUserID = FAMUser.ID INNER JOIN ' +
		'			  Machine ON FileActionStateTransition.MachineID = Machine.ID ' + 
		' WHERE      (FileActionStateTransition.ASC_To = ''C'') AND (FileActionStateTransition.ActionID = ' + @ActionID +') ' + 
		'			AND (FileActionStateTransition.DateTimeStamp >= ''' + @startDate + ''') ' +
		'			AND (FileActionStateTransition.DateTimeStamp <= ''' + @endDate  +''') ' +
		' GROUP BY FAMUser.UserName, Machine.MachineName, ' + @ActionColumnName)
	
	RETURN @SetPendingCount
END
