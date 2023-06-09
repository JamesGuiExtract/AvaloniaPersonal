USE [master]
GO
/****** Object:  Database [EngineeringLICodeDB]    Script Date: 4/4/2016 4:25:08 PM ******/
CREATE DATABASE [EngineeringLICodeDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'EngineeringLICodeDB', FILENAME = N'D:\SQLData\MSSQL11.MSSQLSERVER\MSSQL\DATA\EngineeringLICodeDB.mdf' , SIZE = 10240KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'EngineeringLICodeDB_log', FILENAME = N'D:\SQLData\MSSQL11.MSSQLSERVER\MSSQL\DATA\EngineeringLICodeDB_log.ldf' , SIZE = 102144KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [EngineeringLICodeDB] SET COMPATIBILITY_LEVEL = 110
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [EngineeringLICodeDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [EngineeringLICodeDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [EngineeringLICodeDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [EngineeringLICodeDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET  DISABLE_BROKER 
GO
ALTER DATABASE [EngineeringLICodeDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [EngineeringLICodeDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET RECOVERY FULL 
GO
ALTER DATABASE [EngineeringLICodeDB] SET  MULTI_USER 
GO
ALTER DATABASE [EngineeringLICodeDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [EngineeringLICodeDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [EngineeringLICodeDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [EngineeringLICodeDB] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
EXEC sys.sp_db_vardecimal_storage_format N'EngineeringLICodeDB', N'ON'
GO
USE [EngineeringLICodeDB]
GO
/****** Object:  User [EXTRACT\Developers]    Script Date: 4/4/2016 4:25:08 PM ******/
CREATE USER [EXTRACT\Developers] FOR LOGIN [EXTRACT\Developers]
GO
ALTER ROLE [db_datareader] ADD MEMBER [EXTRACT\Developers]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [EXTRACT\Developers]
GO
/****** Object:  Table [dbo].[ELICodes]    Script Date: 4/4/2016 4:25:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ELICodes](
	[ID] [int] IDENTITY(40000,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[MachineID] [int] NOT NULL,
	[DateAndTime] [datetime] NOT NULL,
	[LICode]  AS (('"ELI'+CONVERT([nvarchar](10),[ID],(0)))+'"'),
 CONSTRAINT [PK_ELICodes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Machines]    Script Date: 4/4/2016 4:25:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Machines](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Machine] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Machine] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MLICodes]    Script Date: 4/4/2016 4:25:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MLICodes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[MachineID] [int] NOT NULL,
	[DateAndTime] [datetime] NOT NULL,
	[LICode]  AS (('"MLI'+CONVERT([nvarchar](10),[ID],(0)))+'"'),
 CONSTRAINT [PK_MLICodes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Users]    Script Date: 4/4/2016 4:25:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[ELICodes] ADD  CONSTRAINT [DF_ELICodes_DateAndTime]  DEFAULT (getdate()) FOR [DateAndTime]
GO
ALTER TABLE [dbo].[MLICodes] ADD  CONSTRAINT [DF_MLICodes_DateAndTime]  DEFAULT (getdate()) FOR [DateAndTime]
GO
ALTER TABLE [dbo].[ELICodes]  WITH CHECK ADD  CONSTRAINT [FK_ELICodes_Machines] FOREIGN KEY([MachineID])
REFERENCES [dbo].[Machines] ([ID])
GO
ALTER TABLE [dbo].[ELICodes] CHECK CONSTRAINT [FK_ELICodes_Machines]
GO
ALTER TABLE [dbo].[ELICodes]  WITH CHECK ADD  CONSTRAINT [FK_ELICodes_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[ELICodes] CHECK CONSTRAINT [FK_ELICodes_Users]
GO
ALTER TABLE [dbo].[MLICodes]  WITH CHECK ADD  CONSTRAINT [FK_MLICodes_Machines] FOREIGN KEY([MachineID])
REFERENCES [dbo].[Machines] ([ID])
GO
ALTER TABLE [dbo].[MLICodes] CHECK CONSTRAINT [FK_MLICodes_Machines]
GO
ALTER TABLE [dbo].[MLICodes]  WITH CHECK ADD  CONSTRAINT [FK_MLICodes_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([ID])
GO
ALTER TABLE [dbo].[MLICodes] CHECK CONSTRAINT [FK_MLICodes_Users]
GO
/****** Object:  StoredProcedure [dbo].[GetEliCodes]    Script Date: 4/4/2016 4:25:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetEliCodes]
	-- Add the parameters for the stored procedure here
	@NumberOfCodes int = 1
AS
BEGIN
	DECLARE @MachineName nvarchar(50);
	DECLARE @MachineID int;
	DECLARE @UserName nvarchar(50);
	DECLARE @UserID int;

	SELECT @MachineName = HOST_NAME(), @UserName = SUSER_SNAME();

	Select @MachineID = ID FROM Machines WHERE Machine = @MachineName
	Select @UserID = ID FROM Users WHERE UserName = @UserName
	
	IF (@UserID IS NULL) 
	BEGIN 
		INSERT INTO Users (UserName)
		VALUES (@UserName)
		SELECT @UserID = ID FROM USERS WHERE UserName = @UserName
	END;

	IF (@MachineID IS NULL)
	BEGIN
		INSERT INTO Machines (Machine)
		VALUES (@MachineName)
		SELECT @MachineID = ID FROM Machines WHERE Machine = @MachineName
	END;

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @InsertSQL nvarchar(max)
	SET @InsertSQL = 'INSERT INTO ELICodes (UserID, MachineID) OUTPUT INSERTED.*  VALUES '
	
	DECLARE @First as int;
	SET @First = 1;

	WHILE (@NumberOfCodes > 0)
	BEGIN
		if (@First = 1)
		BEGIN
			SELECT @First = 0;
		END ELSE BEGIN
			SELECT @InsertSQL = @InsertSQL + ',';
		END;

		SELECT @InsertSQL = @InsertSQL + '(' + CONVERT(nvarchar(max), @UserID, 0) + ', ' + CONVERT(nvarchar(max), @MachineID, 0) + ')'
		SELECT @NumberOfCodes = @NumberOfCodes -1;
	END

	DECLARE @retryCount as int;
	DECLARE @completed as int;
	set @completed = 0;
	set @retryCount = 0;

	WHILE (@completed = 0 and @retryCount < 10)
	BEGIN
		BEGIN TRY
			BEGIN TRANSACTION
			EXECUTE(@InsertSQL);
			COMMIT TRANSACTION
			SET @completed = 1;
		END TRY
		BEGIN CATCH
			ROLLBACK TRANSACTION
			SET @retryCount = @retryCount +1
		END CATCH
	END



END


GO
/****** Object:  StoredProcedure [dbo].[GetMliCodes]    Script Date: 4/4/2016 4:25:08 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetMliCodes]
	-- Add the parameters for the stored procedure here
	@NumberOfCodes int = 1
AS
BEGIN
	DECLARE @MachineName nvarchar(50);
	DECLARE @MachineID int;
	DECLARE @UserName nvarchar(50);
	DECLARE @UserID int;

	SELECT @MachineName = HOST_NAME(), @UserName = SUSER_SNAME();

	Select @MachineID = ID FROM Machines WHERE Machine = @MachineName
	Select @UserID = ID FROM Users WHERE UserName = @UserName
	
	IF (@UserID IS NULL) 
	BEGIN 
		INSERT INTO Users (UserName)
		VALUES (@UserName)
		SELECT @UserID = ID FROM USERS WHERE UserName = @UserName
	END;

	IF (@MachineID IS NULL)
	BEGIN
		INSERT INTO Machines (Machine)
		VALUES (@MachineName)
		SELECT @MachineID = ID FROM Machines WHERE Machine = @MachineName
	END;

	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @InsertSQL nvarchar(max)
	SET @InsertSQL = 'INSERT INTO MliCodes (UserID, MachineID) OUTPUT INSERTED.*  VALUES '
	
	DECLARE @First as int;
	SET @First = 1;

	WHILE (@NumberOfCodes > 0)
	BEGIN
		if (@First = 1)
		BEGIN
			SELECT @First = 0;
		END ELSE BEGIN
			SELECT @InsertSQL = @InsertSQL + ',';
		END;

		SELECT @InsertSQL = @InsertSQL + '(' + CONVERT(nvarchar(max), @UserID, 0) + ', ' + CONVERT(nvarchar(max), @MachineID, 0) + ')'
		SELECT @NumberOfCodes = @NumberOfCodes -1;
	END

	EXECUTE(@InsertSQL);

END




GO
USE [master]
GO
ALTER DATABASE [EngineeringLICodeDB] SET  READ_WRITE 
GO
