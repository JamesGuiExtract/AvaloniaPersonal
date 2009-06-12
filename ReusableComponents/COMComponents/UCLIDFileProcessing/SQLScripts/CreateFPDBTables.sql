USE [FPDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Action]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Action](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ASCName] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NULL,
 CONSTRAINT [PK_ActionType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DBInfo]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[DBInfo](
	[FPMDBSchemaVersion] [int] NOT NULL
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActionState]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ActionState](
	[Code] [nvarchar](1) NOT NULL,
	[Meaning] [nvarchar](255) NULL,
 CONSTRAINT [PK_ActionState] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FPMFile]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[FPMFile](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FileName] [nvarchar](255) NULL,
	[Priority] [int] NULL,
	[FileSize] [int] NOT NULL CONSTRAINT [DF_FPMFile_FileSize]  DEFAULT ((0)),
	[Pages] [int] NOT NULL CONSTRAINT [DF_FPMFile_Pages]  DEFAULT ((0)),
 CONSTRAINT [PK_File] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[FPMFile]') AND name = N'IX_Files_FileName')
CREATE UNIQUE NONCLUSTERED INDEX [IX_Files_FileName] ON [dbo].[FPMFile] 
(
	[FileName] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[QueueEventCode]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[QueueEventCode](
	[Code] [nvarchar](1) NOT NULL,
	[Description] [nvarchar](255) NULL,
 CONSTRAINT [PK_QueueEventCode] PRIMARY KEY CLUSTERED 
(
	[Code] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ActionStatistics]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ActionStatistics](
	[ActionID] [int] NOT NULL,
	[NumDocuments] [int] NOT NULL CONSTRAINT [DF_Statistics_TotalDocuments]  DEFAULT ((0)),
	[NumDocumentsComplete] [int] NOT NULL CONSTRAINT [DF_Statistics_ProcessedDocuments]  DEFAULT ((0)),
	[NumDocumentsFailed] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumDocumentsFailed]  DEFAULT ((0)),
	[NumPages] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumPages]  DEFAULT ((0)),
	[NumPagesComplete] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumPagesComplete]  DEFAULT ((0)),
	[NumPagesFailed] [int] NOT NULL CONSTRAINT [DF_ActionStatistics_NumPagesFailed]  DEFAULT ((0)),
	[NumBytes] [bigint] NOT NULL CONSTRAINT [DF_ActionStatistics_NumBytes]  DEFAULT ((0)),
	[NumBytesComplete] [bigint] NOT NULL CONSTRAINT [DF_ActionStatistics_NumBytesComplete]  DEFAULT ((0)),
	[NumBytesFailed] [bigint] NOT NULL CONSTRAINT [DF_ActionStatistics_NumBytesFailed]  DEFAULT ((0)),
 CONSTRAINT [PK_Statistics] PRIMARY KEY CLUSTERED 
(
	[ActionID] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FileActionStateTransition]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[FileActionStateTransition](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FileID] [int] NULL,
	[ActionID] [int] NULL,
	[ASC_From] [nvarchar](1) NULL,
	[ASC_To] [nvarchar](1) NULL,
	[TS_Transition] [datetime] NULL,
	[ExecutedOnNode] [nvarchar](50) NULL,
	[Exception] [ntext] NULL,
	[Comment] [nvarchar](50) NULL,
 CONSTRAINT [PK_FileActionStateTransition] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[QueueEvent]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[QueueEvent](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FileID] [int] NULL,
	[TimeStamp] [datetime] NULL,
	[QueueEventCode] [nvarchar](1) NULL,
	[FileSupplierDesc] [nvarchar](255) NULL,
	[FileModifyTime] [datetime] NULL,
	[FileSizeInBytes] [int] NULL,
 CONSTRAINT [PK_QueueEvent] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[QueueEvent]') AND name = N'IX_FileID')
CREATE NONCLUSTERED INDEX [IX_FileID] ON [dbo].[QueueEvent] 
(
	[FileID] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Statistics_Action]') AND parent_object_id = OBJECT_ID(N'[dbo].[ActionStatistics]'))
ALTER TABLE [dbo].[ActionStatistics]  WITH CHECK ADD  CONSTRAINT [FK_Statistics_Action] FOREIGN KEY([ActionID])
REFERENCES [dbo].[Action] ([ID])
GO
ALTER TABLE [dbo].[ActionStatistics] CHECK CONSTRAINT [FK_Statistics_Action]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FileActionStateTransition_Action]') AND parent_object_id = OBJECT_ID(N'[dbo].[FileActionStateTransition]'))
ALTER TABLE [dbo].[FileActionStateTransition]  WITH CHECK ADD  CONSTRAINT [FK_FileActionStateTransition_Action] FOREIGN KEY([ActionID])
REFERENCES [dbo].[Action] ([ID])
GO
ALTER TABLE [dbo].[FileActionStateTransition] CHECK CONSTRAINT [FK_FileActionStateTransition_Action]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_FileActionStateTransition_FPMFile]') AND parent_object_id = OBJECT_ID(N'[dbo].[FileActionStateTransition]'))
ALTER TABLE [dbo].[FileActionStateTransition]  WITH CHECK ADD  CONSTRAINT [FK_FileActionStateTransition_FPMFile] FOREIGN KEY([FileID])
REFERENCES [dbo].[FPMFile] ([ID])
GO
ALTER TABLE [dbo].[FileActionStateTransition] CHECK CONSTRAINT [FK_FileActionStateTransition_FPMFile]
GO
IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'MS_Description' , N'SCHEMA',N'dbo', N'TABLE',N'FileActionStateTransition', N'CONSTRAINT',N'FK_FileActionStateTransition_FPMFile'))
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'FileActionStateTransition and FPMFile' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'FileActionStateTransition', @level2type=N'CONSTRAINT',@level2name=N'FK_FileActionStateTransition_FPMFile'
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_QueueEvent_File]') AND parent_object_id = OBJECT_ID(N'[dbo].[QueueEvent]'))
ALTER TABLE [dbo].[QueueEvent]  WITH CHECK ADD  CONSTRAINT [FK_QueueEvent_File] FOREIGN KEY([FileID])
REFERENCES [dbo].[FPMFile] ([ID])
GO
ALTER TABLE [dbo].[QueueEvent] CHECK CONSTRAINT [FK_QueueEvent_File]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_QueueEvent_QueueEventCode]') AND parent_object_id = OBJECT_ID(N'[dbo].[QueueEvent]'))
ALTER TABLE [dbo].[QueueEvent]  WITH CHECK ADD  CONSTRAINT [FK_QueueEvent_QueueEventCode] FOREIGN KEY([QueueEventCode])
REFERENCES [dbo].[QueueEventCode] ([Code])
GO
ALTER TABLE [dbo].[QueueEvent] CHECK CONSTRAINT [FK_QueueEvent_QueueEventCode]
