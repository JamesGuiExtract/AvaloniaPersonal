DECLARE @StartTime DATETIME
DECLARE @EndTime DATETIME

-- Get the beginning of today as EndTime
SET @EndTime = CAST(FLOOR(CAST(GETDATE() AS DECIMAL(12, 5))) AS DATETIME)
-- Next line allows for checking a specific previous day
-- Back up a specified number of days, if desired
--SET @EndTime = DATEADD(d, -2, @EndTime)
-- Add 12 hours so that EndTime is noon on the desired day
SET @EndTime = DATEADD(hh, 12, @EndTime)
-- Set StartTime as one day before EndTime
SET @StartTime = DATEADD(d, -1, @EndTime)

SELECT [MachineName],
	@EndTime AS [For 24 hours up to],
	SUM(CASE [ASC_To] WHEN 'C' THEN 1 END) AS [DocsComplete],
	SUM(CASE [ASC_To] WHEN 'C' THEN [Pages] END) AS [PagesComplete],
	SUM(CASE [ASC_To] WHEN 'F' THEN 1 END) AS [DocsFailed]
	FROM [FileActionStateTransition]
	INNER JOIN [Machine] ON [MachineID] = [Machine].[ID]
	INNER JOIN [Action] ON [ActionID] = [Action].[ID]
	INNER JOIN [FAMFile] ON [FileID] = [FAMFile].[ID]
	WHERE [ASCName] = 'Process'
		AND [ASC_From] = 'R'
		AND [DateTimeStamp] >= @StartTime
		AND [DateTimeStamp] < @EndTime
	GROUP BY [MachineName]
	ORDER BY [MachineName]