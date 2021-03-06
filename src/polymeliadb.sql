USE [master]
GO
/****** Object:  Database [PolymeliaDeploy]    Script Date: 2013-09-25 19:16:23 ******/
CREATE DATABASE [PolymeliaDeploy] ON  PRIMARY 
( NAME = N'PolymeliaDeploy', FILENAME = N'E:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\PolymeliaDeploy.mdf' , SIZE = 5120KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'PolymeliaDeploy_log', FILENAME = N'E:\Program Files\Microsoft SQL Server\MSSQL11.MSSQLSERVER\MSSQL\DATA\PolymeliaDeploy_log.ldf' , SIZE = 1280KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [PolymeliaDeploy].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [PolymeliaDeploy] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET ARITHABORT OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [PolymeliaDeploy] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [PolymeliaDeploy] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [PolymeliaDeploy] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET  DISABLE_BROKER 
GO
ALTER DATABASE [PolymeliaDeploy] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [PolymeliaDeploy] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [PolymeliaDeploy] SET RECOVERY FULL 
GO
ALTER DATABASE [PolymeliaDeploy] SET  MULTI_USER 
GO
ALTER DATABASE [PolymeliaDeploy] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [PolymeliaDeploy] SET DB_CHAINING OFF 
GO
EXEC sys.sp_db_vardecimal_storage_format N'PolymeliaDeploy', N'ON'
GO
USE [PolymeliaDeploy]
GO
/****** Object:  Table [dbo].[ActivityReports]    Script Date: 2013-09-25 19:16:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ActivityReports](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[DeploymentId] [bigint] NOT NULL,
	[ActivityTaskId] [bigint] NULL,
	[ActivityName] [nvarchar](50) NOT NULL,
	[Message] [nvarchar](max) NOT NULL,
	[LocalCreated] [datetime] NOT NULL,
	[Created] [datetime] NOT NULL,
	[ServerRole] [nvarchar](50) NOT NULL,
	[MachineName] [nvarchar](50) NOT NULL,
	[Status] [int] NOT NULL,
	[Environment] [nvarchar](50) NULL,
 CONSTRAINT [PK_Task_Reports] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ActivityTasks]    Script Date: 2013-09-25 19:16:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ActivityTasks](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[DeploymentId] [bigint] NOT NULL,
	[ServerRole] [nvarchar](128) NOT NULL,
	[ActivityCode] [nvarchar](max) NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[Created] [datetime] NOT NULL,
	[ActivityName] [nvarchar](128) NOT NULL,
	[Status] [int] NOT NULL,
	[DeployVersion] [nvarchar](10) NOT NULL,
	[Environment] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_dbo.ActivityTasks] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Agents]    Script Date: 2013-09-25 19:16:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Agents](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Role] [nvarchar](50) NOT NULL,
	[ServerName] [nvarchar](50) NOT NULL,
	[IpAddress] [nvarchar](50) NOT NULL,
	[ConfirmedBy] [nvarchar](50) NOT NULL,
	[Confirmed] [datetime] NULL,
	[IsActive] [bit] NULL,
	[LastDeploymentId] [bigint] NULL,
	[AgentToken] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Agents] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Deployments]    Script Date: 2013-09-25 19:16:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Deployments](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ProjectId] [int] NOT NULL,
	[EnvironmentId] [int] NOT NULL,
	[Version] [nvarchar](10) NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[Created] [datetime] NOT NULL,
	[Status] [int] NOT NULL,
	[DeployActivity] [nvarchar](max) NOT NULL,
	[Environment] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_MainActivity] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DeployVariables]    Script Date: 2013-09-25 19:16:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeployVariables](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[DeploymentId] [bigint] NOT NULL,
	[VariableKey] [nvarchar](50) NOT NULL,
	[VariableValue] [nvarchar](512) NOT NULL,
	[Scope] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_DeployVariables] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Environments]    Script Date: 2013-09-25 19:16:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Environments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ProjectId] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[WorkflowContent] [nvarchar](max) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[Deleted] [bit] NOT NULL,
 CONSTRAINT [PK_Environments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Projects]    Script Date: 2013-09-25 19:16:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Projects](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[Created] [datetime] NOT NULL,
	[CreatedBy] [nvarchar](50) NOT NULL,
	[Deleted] [bit] NOT NULL,
 CONSTRAINT [PK_Projects] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Variables]    Script Date: 2013-09-25 19:16:23 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Variables](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EnvironmentId] [int] NOT NULL,
	[VariableKey] [nvarchar](50) NOT NULL,
	[VariableValue] [nvarchar](512) NOT NULL,
	[Scope] [nvarchar](50) NULL,
 CONSTRAINT [PK_Variables] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_Environments]    Script Date: 2013-09-25 19:16:23 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Environments] ON [dbo].[Environments]
(
	[ProjectId] ASC,
	[Name] ASC,
	[Deleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ActivityReports] ADD  CONSTRAINT [DF_Task_Reports_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[ActivityTasks] ADD  CONSTRAINT [DF_ActivityTasks_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[ActivityTasks] ADD  CONSTRAINT [DF_ActivityTasks_Status]  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[Deployments] ADD  CONSTRAINT [DF_MainActivity_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Deployments] ADD  CONSTRAINT [DF_MainActivity_Status]  DEFAULT ((0)) FOR [Status]
GO
ALTER TABLE [dbo].[Environments] ADD  CONSTRAINT [DF_Environments_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Environments] ADD  CONSTRAINT [DF_Environments_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[Projects] ADD  CONSTRAINT [DF_Projects_Created]  DEFAULT (getdate()) FOR [Created]
GO
ALTER TABLE [dbo].[Projects] ADD  CONSTRAINT [DF_Projects_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
ALTER TABLE [dbo].[ActivityReports]  WITH CHECK ADD  CONSTRAINT [FK_ActivityReports_ActivityTasks] FOREIGN KEY([ActivityTaskId])
REFERENCES [dbo].[ActivityTasks] ([Id])
GO
ALTER TABLE [dbo].[ActivityReports] CHECK CONSTRAINT [FK_ActivityReports_ActivityTasks]
GO
ALTER TABLE [dbo].[ActivityReports]  WITH CHECK ADD  CONSTRAINT [FK_ActivityReports_MainActivities] FOREIGN KEY([DeploymentId])
REFERENCES [dbo].[Deployments] ([Id])
GO
ALTER TABLE [dbo].[ActivityReports] CHECK CONSTRAINT [FK_ActivityReports_MainActivities]
GO
ALTER TABLE [dbo].[ActivityTasks]  WITH CHECK ADD  CONSTRAINT [FK_ActivityTasks_MainActivities] FOREIGN KEY([DeploymentId])
REFERENCES [dbo].[Deployments] ([Id])
GO
ALTER TABLE [dbo].[ActivityTasks] CHECK CONSTRAINT [FK_ActivityTasks_MainActivities]
GO
ALTER TABLE [dbo].[Deployments]  WITH CHECK ADD  CONSTRAINT [FK_MainActivities_Environments] FOREIGN KEY([EnvironmentId])
REFERENCES [dbo].[Environments] ([Id])
GO
ALTER TABLE [dbo].[Deployments] CHECK CONSTRAINT [FK_MainActivities_Environments]
GO
ALTER TABLE [dbo].[Deployments]  WITH CHECK ADD  CONSTRAINT [FK_MainActivities_Projects] FOREIGN KEY([ProjectId])
REFERENCES [dbo].[Projects] ([Id])
GO
ALTER TABLE [dbo].[Deployments] CHECK CONSTRAINT [FK_MainActivities_Projects]
GO
ALTER TABLE [dbo].[DeployVariables]  WITH CHECK ADD  CONSTRAINT [FK_DeployVariables_MainActivities] FOREIGN KEY([DeploymentId])
REFERENCES [dbo].[Deployments] ([Id])
GO
ALTER TABLE [dbo].[DeployVariables] CHECK CONSTRAINT [FK_DeployVariables_MainActivities]
GO
ALTER TABLE [dbo].[Environments]  WITH CHECK ADD  CONSTRAINT [FK_Environments_Projects] FOREIGN KEY([ProjectId])
REFERENCES [dbo].[Projects] ([Id])
GO
ALTER TABLE [dbo].[Environments] CHECK CONSTRAINT [FK_Environments_Projects]
GO
ALTER TABLE [dbo].[Variables]  WITH CHECK ADD  CONSTRAINT [FK_Variables_Environments] FOREIGN KEY([EnvironmentId])
REFERENCES [dbo].[Environments] ([Id])
GO
ALTER TABLE [dbo].[Variables] CHECK CONSTRAINT [FK_Variables_Environments]
GO
USE [master]
GO
ALTER DATABASE [PolymeliaDeploy] SET  READ_WRITE 
GO
