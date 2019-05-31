--DECLARE @StartTime DATETIME
--DECLARE @EndTime DATETIME

---- Get the beginning of today as EndTime
--SET @EndTime = CAST(FLOOR(CAST(GETDATE() AS DECIMAL(12, 5))) AS DATETIME)
---- Next line allows for checking a specific previous day
---- Back up a specified number of days, if desired
----SET @EndTime = DATEADD(d, -2, @EndTime)
---- Add 12 hours so that EndTime is noon on the desired day
--SET @EndTime = DATEADD(hh, 15, @EndTime)
---- Set StartTime as one day before EndTime
--SET @StartTime = DATEADD(d, -5, @EndTime)

SELECT DATEADD(HOUR, DATEDIFF(HOUR, 0, [DateTimeStamp]), 0),
	SUM(CASE [ASC_To] WHEN 'C' THEN 1 END) AS [DocsComplete]
	FROM [FileActionStateTransition]
	INNER JOIN [Machine] ON [MachineID] = [Machine].[ID]
	INNER JOIN [Action] ON [ActionID] = [Action].[ID]
	INNER JOIN [FAMFile] ON [FileID] = [FAMFile].[ID]
	WHERE [ASCName] = 'Action3'
		AND [ASC_From] = 'R'
	GROUP BY DATEADD(HOUR, DATEDIFF(HOUR, 0, [DateTimeStamp]), 0)
	ORDER BY DATEADD(HOUR, DATEDIFF(HOUR, 0, [DateTimeStamp]), 0)