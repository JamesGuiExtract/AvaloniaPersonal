INSERT INTO [FPDB].[dbo].[ActionState]
           ([Code]
           ,[Meaning])
     VALUES
           ('C', 'Complete')

GO

INSERT INTO [FPDB].[dbo].[ActionState]
           ([Code]
           ,[Meaning])
     VALUES
		('F', 'Failed')

GO

INSERT INTO [FPDB].[dbo].[ActionState]
           ([Code]
           ,[Meaning])
     VALUES
		('P', 'Pending')

GO

INSERT INTO [FPDB].[dbo].[ActionState]
           ([Code]
           ,[Meaning])
     VALUES
		('R', 'Processing')

GO

INSERT INTO [FPDB].[dbo].[ActionState]
           ([Code]
           ,[Meaning])
     VALUES
		('U', 'Unattempted')