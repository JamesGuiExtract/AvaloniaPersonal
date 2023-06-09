INSERT INTO [FPDB].[dbo].[QueueEventCode]
           ([Code]
           ,[Description])
     VALUES
           ('A', 'File added to queue')

GO

INSERT INTO [FPDB].[dbo].[QueueEventCode]
           ([Code]
           ,[Description])
     VALUES
           ('D', 'File deleted from queue')

GO

INSERT INTO [FPDB].[dbo].[QueueEventCode]
           ([Code]
           ,[Description])
     VALUES
           ('F', 'Folder was deleted')

GO

INSERT INTO [FPDB].[dbo].[QueueEventCode]
           ([Code]
           ,[Description])
     VALUES
           ('M', 'File was modified')

GO

INSERT INTO [FPDB].[dbo].[QueueEventCode]
           ([Code]
           ,[Description])
     VALUES
           ('R', 'File was renamed')