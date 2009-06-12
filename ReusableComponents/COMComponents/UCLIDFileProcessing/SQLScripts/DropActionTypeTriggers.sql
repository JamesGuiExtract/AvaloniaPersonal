USE [FPDB]
GO
/****** Object:  Trigger [AddFPMFile_ASCColumn]    Script Date: 09/07/2006 10:23:43 ******/
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[AddFPMFile_ASCColumn]'))
DROP TRIGGER [dbo].[AddFPMFile_ASCColumn]

GO
/****** Object:  Trigger [Remove_FPMFile_ASCColumn]    Script Date: 09/07/2006 10:24:16 ******/
IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[Remove_FPMFile_ASCColumn]'))
DROP TRIGGER [dbo].[Remove_FPMFile_ASCColumn]