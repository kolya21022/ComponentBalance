-- Создание/инициализация БД Баланса деталей [2020.03.11]

---- Удаление базы данных, если она существует
--USE [master]
--IF EXISTS (select * from [sys].[databases] where [name]='ComponentBalanceDb')
-- DROP DATABASE [ComponentBalanceDb]
--GO

-- Создание базы данных
CREATE DATABASE [ComponentBalanceDb]
GO
ALTER DATABASE [ComponentBalanceDb] MODIFY FILE 
( NAME = N'ComponentBalanceDb' , SIZE = 10240KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
GO
ALTER DATABASE [ComponentBalanceDb] MODIFY FILE 
( NAME = N'ComponentBalanceDb_log' , SIZE = 10240KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
EXEC dbo.sp_dbcmptlevel @dbname=N'ComponentBalanceDb', @new_cmptlevel=100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ComponentBalanceDb].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [ComponentBalanceDb] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET ARITHABORT OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [ComponentBalanceDb] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [ComponentBalanceDb] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [ComponentBalanceDb] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET DISABLE_BROKER 
GO
ALTER DATABASE [ComponentBalanceDb] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [ComponentBalanceDb] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [ComponentBalanceDb] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [ComponentBalanceDb] SET MULTI_USER 
GO
ALTER DATABASE [ComponentBalanceDb] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [ComponentBalanceDb] SET DB_CHAINING OFF 
GO

-- Создание таблицы [Рабочий месяц]
USE [ComponentBalanceDb]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MonthYears] (
	[Id] bigint IDENTITY(1, 1) NOT NULL, 
	[Month] int NOT NULL,
	[Year] int NOT NULL,
	[Description] nvarchar(50) NOT NULL,
CONSTRAINT [PK_MonthYears] PRIMARY KEY CLUSTERED (
	[Id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC [sys].[sp_addextendedproperty]	@name=N'MS_Description', @level0type=N'SCHEMA',
	@level0name=N'dbo', @level1type=N'TABLE',
	@level1name=N'MonthYears',
	@value=N'Таблица с датами'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'MonthYears', 
	@level2name = N'Id', 
	@value = N'Уникальный идентификатор'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'MonthYears', 
	@level2name = N'Month', 
	@value = N'Месяц'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'MonthYears', 
	@level2name = N'Year', 
	@value = N'Год'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'MonthYears', 
	@level2name = N'Description', 
	@value = N'Описание даты'
GO

-- Создание таблицы [Рабочие цеха]
USE [ComponentBalanceDb]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkGuilds] (
	[Id] bigint IDENTITY(1, 1) NOT NULL, 
	[WorkGuildNumber] int NOT NULL,
	[Password] nvarchar(25) NOT NULL,
	[Lvl] tinyint NOT NULL,
CONSTRAINT [PK_WorkGuilds] PRIMARY KEY CLUSTERED (
	[Id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC [sys].[sp_addextendedproperty]	@name=N'MS_Description', @level0type=N'SCHEMA',
	@level0name=N'dbo', @level1type=N'TABLE',
	@level1name=N'WorkGuilds',
	@value=N'Таблица цехов'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'WorkGuilds', 
	@level2name = N'Id', 
	@value = N'Уникальный идентификатор'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'WorkGuilds', 
	@level2name = N'WorkGuildNumber', 
	@value = N'Код цеха'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'WorkGuilds', 
	@level2name = N'Password', 
	@value = N'Пароль'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'WorkGuilds', 
	@level2name = N'Lvl', 
	@value = N'Уровень доступа в программе, чем больше номер тем выше допуск'
GO
-- Ключ уникальности поля [WorkGuild]
ALTER TABLE [dbo].[WorkGuilds]
	ADD CONSTRAINT [UQ_WorkGuilds_WorkGuildNumber] UNIQUE NONCLUSTERED ([WorkGuildNumber]) ON [PRIMARY]
GO

-- Создание таблицы [Закрытые месяця]
USE [ComponentBalanceDb]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MonthCloses] (
	[Id] bigint IDENTITY(1, 1) NOT NULL, 
	[WorkGuildId] bigint NOT NULL,
	[IsClose] bit NOT NULL,
	[Month] int NOT NULL,
	[Year] int NOT NULL
CONSTRAINT [PK_MonthCloses] PRIMARY KEY CLUSTERED (
	[Id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC [sys].[sp_addextendedproperty]	@name=N'MS_Description', @level0type=N'SCHEMA',
	@level0name=N'dbo', @level1type=N'TABLE',
	@level1name=N'MonthCloses',
	@value=N'Таблица закрытых цехом месяцов'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'MonthCloses', 
	@level2name = N'Id', 
	@value = N'Уникальный идентификатор'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'MonthCloses', 
	@level2name = N'WorkGuildId', 
	@value = N'Вторичный ключ на таблицу цехов'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'MonthCloses', 
	@level2name = N'IsClose', 
	@value = N'Флаг что месяц закрыт'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'MonthCloses', 
	@level2name = N'Month', 
	@value = N'Рабочий месяц'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'MonthCloses', 
	@level2name = N'Year', 
	@value = N'Рабочий год'
GO
-- Ключ уникальности поля [WorkGuild]
ALTER TABLE [dbo].[MonthCloses]
	ADD CONSTRAINT [UQ_MonthCloses_WorkGuildId_and_Month_and_Year] UNIQUE NONCLUSTERED ([WorkGuildId], [Month], [Year]) ON [PRIMARY]
GO
-- Вторичный ключ на таблицу [Цеха] 
ALTER TABLE [dbo].[MonthCloses] WITH CHECK
ADD CONSTRAINT [FK_MonthClosesToWorkGuilds] FOREIGN KEY([WorkGuildId])
REFERENCES [dbo].[WorkGuilds] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO

-- Создание таблицы [Продукты]
USE [ComponentBalanceDb]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products] (
	[Id] bigint IDENTITY(1, 1) NOT NULL, 
	[Code] bigint NOT NULL,
	[Name] nvarchar(50) NOT NULL,
	[Designation] nvarchar(50) NOT NULL, 
CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED (
	[Id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC [sys].[sp_addextendedproperty]	@name=N'MS_Description', @level0type=N'SCHEMA',
	@level0name=N'dbo', @level1type=N'TABLE',
	@level1name=N'Products',
	@value=N'Таблица продуктов'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Products', 
	@level2name = N'Id', 
	@value = N'Уникальный идентификатор'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Products', 
	@level2name = N'Code', 
	@value = N'Код продукта'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Products', 
	@level2name = N'Name', 
	@value = N'Название'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Products', 
	@level2name = N'Designation', 
	@value = N'Обозначение'
GO
-- Ключ уникальности поля [Code]
ALTER TABLE [dbo].[Products]
	ADD CONSTRAINT [UQ_Products_Code] UNIQUE NONCLUSTERED ([Code]) ON [PRIMARY]
GO

-- Создание таблицы [Виды ТМЦ]
USE [ComponentBalanceDb]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TmcTypes] (
	[Id] bigint IDENTITY(1, 1) NOT NULL, 	
	[Name] nvarchar(50) NOT NULL,
	[ShortName] nvarchar(10) NOT NULL,
CONSTRAINT [PK_TmcTypes] PRIMARY KEY CLUSTERED (
	[Id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC [sys].[sp_addextendedproperty]	@name=N'MS_Description', @level0type=N'SCHEMA',
	@level0name=N'dbo', @level1type=N'TABLE',
	@level1name=N'TmcTypes',
	@value=N'Таблица видов ТМЦ'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'TmcTypes', 
	@level2name = N'Id', 
	@value = N'Уникальный идентификатор'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'TmcTypes', 
	@level2name = N'Name', 
	@value = N'Название'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'TmcTypes', 
	@level2name = N'ShortName', 
	@value = N'Соращенное название'
GO
-- Ключ уникальности поля [Name]
ALTER TABLE [dbo].[TmcTypes]
	ADD CONSTRAINT [UQ_TmcTypes_Name] UNIQUE NONCLUSTERED ([Name]) ON [PRIMARY]
GO

-- Создание таблицы [Единиц измерения]
USE [ComponentBalanceDb]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Measures] (
	[Id] bigint IDENTITY(1, 1) NOT NULL, 
	[OldDbCode] int NOT NULL,	
	[ShortName] nvarchar(50) NOT NULL,
CONSTRAINT [PK_Measures] PRIMARY KEY CLUSTERED (
	[Id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC [sys].[sp_addextendedproperty]	@name=N'MS_Description', @level0type=N'SCHEMA',
	@level0name=N'dbo', @level1type=N'TABLE',
	@level1name=N'Measures',
	@value=N'Таблица единиц измерения'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Measures', 
	@level2name = N'Id', 
	@value = N'Уникальный идентификатор'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Measures', 
	@level2name = N'OldDbCode', 
	@value = N'Код в старой dbf'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Measures', 
	@level2name = N'ShortName', 
	@value = N'Короткое название'
GO
-- Ключ уникальности поля [Name]
ALTER TABLE [dbo].[Measures]
	ADD CONSTRAINT [UQ_Measures_OldDbCode] UNIQUE NONCLUSTERED ([OldDbCode]) ON [PRIMARY]
GO

-- Создание таблицы [Детали]
USE [ComponentBalanceDb]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Details] (
	[Id] bigint IDENTITY(1, 1) NOT NULL, 
	[Code] bigint NOT NULL,
	[Name] nvarchar(50) NOT NULL,
	[Designation] nvarchar(50) NOT NULL,
	[Gost] nvarchar(25) NOT NULL,
	[MeasureId] bigint NOT NULL,
	[TmcTypeId] bigint NOT NULL,	
	[Warehouse] int NOT NULL,
CONSTRAINT [PK_Details] PRIMARY KEY CLUSTERED (
	[Id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC [sys].[sp_addextendedproperty]	@name=N'MS_Description', @level0type=N'SCHEMA',
	@level0name=N'dbo', @level1type=N'TABLE',
	@level1name=N'Details',
	@value=N'Таблица продуктов'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Details', 
	@level2name = N'Id', 
	@value = N'Уникальный идентификатор'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Details', 
	@level2name = N'Code', 
	@value = N'Код продукта'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Details', 
	@level2name = N'Name', 
	@value = N'Название'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Details', 
	@level2name = N'Designation', 
	@value = N'Обозначение'
	GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Details', 
	@level2name = N'Gost', 
	@value = N'Гост'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Details', 
	@level2name = N'MeasureId', 
	@value = N'Вторичный ключ на таблицу единиц измерения'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Details', 
	@level2name = N'TmcTypeId', 
	@value = N'Вторичный ключ на таблицу типов ТМЦ'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Details', 
	@level2name = N'Warehouse', 
	@value = N'Склад'
GO
-- Ключ уникальности поля [Code]
ALTER TABLE [dbo].[Details]
	ADD CONSTRAINT [UQ_Details_Code_And_MeasureId_And_Warehouse_And_TmcType] UNIQUE NONCLUSTERED ([Code], [MeasureId], [Warehouse], [TmcTypeId]) ON [PRIMARY]
GO
-- Вторичный ключ на таблицу [Единиц измерений] 
ALTER TABLE [dbo].[Details] WITH CHECK
ADD CONSTRAINT [FK_DetailsToMeasures] FOREIGN KEY([MeasureId])
REFERENCES [dbo].[Measures] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO
-- Вторичный ключ на таблицу [Типы ТМЦ] 
ALTER TABLE [dbo].[Details] WITH CHECK
ADD CONSTRAINT [FK_DetailsToTmcTypes] FOREIGN KEY([TmcTypeId])
REFERENCES [dbo].[TmcTypes] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO

-- Создание таблицы [ЗамещениеДеталей]
USE [ComponentBalanceDb]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReplacementDetails] (
	[Id] bigint IDENTITY(1, 1) NOT NULL, 
	[ShortProductCode] int NOT NULL,
	[DetailStartCode] bigint NOT NULL,
	[DetailStartName] nvarchar(50) NOT NULL,
	[DetailStartDesignation] nvarchar(50) NOT NULL,
	[DetailStartGost] nvarchar(25) NOT NULL,
	[DetailEndCode] bigint NOT NULL,
	[DetailEndName] nvarchar(50) NOT NULL,
	[DetailEndDesignation] nvarchar(50) NOT NULL,
	[DetailEndGost] nvarchar(25) NOT NULL,

	[Cause] nvarchar(150) NOT NULL,

	[SERVICE_CREATE_DATETIME] [datetime] NOT NULL DEFAULT CURRENT_TIMESTAMP, 
	[SERVICE_CREATE_USER] [nvarchar](150) NOT NULL DEFAULT SUSER_SNAME(), 
	[SERVICE_LAST_MODIFY_DATETIME] [datetime] NOT NULL DEFAULT CURRENT_TIMESTAMP, 
	[SERVICE_LAST_MODIFY_USER] [nvarchar](150) NOT NULL DEFAULT SUSER_SNAME(), 
CONSTRAINT [PK_Replacements] PRIMARY KEY CLUSTERED (
	[Id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC [sys].[sp_addextendedproperty]	@name=N'MS_Description', @level0type=N'SCHEMA',
	@level0name=N'dbo', @level1type=N'TABLE',
	@level1name=N'ReplacementDetails',
	@value=N'Таблица разрешенных замещений деталей'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReplacementDetails', 
	@level2name = N'Id', 
	@value = N'Уникальный идентификатор'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReplacementDetails', 
	@level2name = N'ShortProductCode', 
	@value = N'Короткий код продукта'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReplacementDetails', 
	@level2name = N'DetailStartCode', 
	@value = N'Код детали которую можно заменить'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReplacementDetails', 
	@level2name = N'DetailEndCode', 
	@value = N'Код детали на которую можно заменить'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReplacementDetails', 
	@level2name = N'Cause', 
	@value = N'Основание замены'
GO

-- Ключ уникальности полей
ALTER TABLE [dbo].[ReplacementDetails]
	ADD CONSTRAINT [UQ_ReplacementDetails_ShortProductCode_And_DetailStartCode_And_DetailEndCode] 
	UNIQUE NONCLUSTERED ([ShortProductCode], [DetailStartCode], [DetailEndCode]) ON [PRIMARY]
GO

-- Триггер обновления даты-времени последнего редактирования и пользователя изменившего запись последним
CREATE TRIGGER [TriggerUpdateReplacementDetails] ON [ReplacementDetails] 
FOR UPDATE 
AS
	BEGIN 
		IF @@ROWCOUNT = 0 RETURN 
		IF TRIGGER_NESTLEVEL(object_ID('TriggerUpdateReplacementDetails')) > 1 RETURN 
		SET NOCOUNT ON 

		UPDATE [ReplacementDetails] 
		SET [SERVICE_LAST_MODIFY_DATETIME] = CURRENT_TIMESTAMP, [SERVICE_LAST_MODIFY_USER] = SUSER_SNAME() 
		WHERE [id] IN (SELECT DISTINCT [id] FROM [INSERTED]) 
	END
GO

-- Создание таблицы [Баланса деталей]
USE [ComponentBalanceDb]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Balances] (
	[Id] bigint IDENTITY(1, 1) NOT NULL, 
	[WorkGuildId] bigint NOT NULL,
	[DetailId] bigint NOT NULL,
	[CountStart] decimal(9,3) NOT NULL,
	[CostStart] decimal(9,2) NOT NULL,
	[CountEnd] decimal(9,3) NOT NULL,
	[CostEnd] decimal(9,2) NOT NULL,
	[Month] int NOT NULL,
	[Year] int NOT NULL, 
CONSTRAINT [PK_Balances] PRIMARY KEY CLUSTERED (
	[Id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC [sys].[sp_addextendedproperty]	@name=N'MS_Description', @level0type=N'SCHEMA',
	@level0name=N'dbo', @level1type=N'TABLE',
	@level1name=N'Balances',
	@value=N'Таблица остатков'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Balances', 
	@level2name = N'Id', 
	@value = N'Уникальный идентификатор'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Balances', 
	@level2name = N'WorkGuildId', 
	@value = N'Вторичный ключ на таблицу цехов'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Balances', 
	@level2name = N'DetailId', 
	@value = N'Вторичный ключ на таблицу материалов'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Balances', 
	@level2name = N'CountStart', 
	@value = N'Количество на начало месяца'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Balances', 
	@level2name = N'CostStart', 
	@value = N'Цена на начало месяца'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Balances', 
	@level2name = N'CountEnd', 
	@value = N'Количество на конец месяца(с учетом всех движений)'
	GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Balances', 
	@level2name = N'CostEnd', 
	@value = N'Цена на конец месяца(с учетом всех движений)'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Balances', 
	@level2name = N'Month', 
	@value = N'Рабочий месяц'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Balances', 
	@level2name = N'Year', 
	@value = N'Рабочий год'
GO
-- Ключ уникальности поля [DetailId] и [WorkGuildId]
ALTER TABLE [dbo].[Balances]
	ADD CONSTRAINT [UQ_Balances_DetailId_And_WorkGuildId_And_Month_And_Year] 
	UNIQUE NONCLUSTERED ([DetailId], [WorkGuildId], [Month], [Year]) ON [PRIMARY]
GO
-- Вторичный ключ на таблицу [Цехов] 
ALTER TABLE [dbo].[Balances] WITH CHECK
ADD CONSTRAINT [FK_BalancesToWorkGuilds] FOREIGN KEY([WorkGuildId])
REFERENCES [dbo].[WorkGuilds] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO
-- Вторичный ключ на таблицу [Деталей] 
ALTER TABLE [dbo].[Balances] WITH CHECK
ADD CONSTRAINT [FK_BalancesToDetails] FOREIGN KEY([DetailId])
REFERENCES [dbo].[Details] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO

-- Создание таблицы [Выпущенные изделия]
USE [ComponentBalanceDb]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReleaseProducts] (
	[Id] bigint IDENTITY(1, 1) NOT NULL, 
	[WorkGuildId] bigint NOT NULL,
	[FactoryNumber] nvarchar(25) NOT NULL,
	[ProductId] bigint NOT NULL,
	[Month] int NOT NULL,
	[Year] int NOT NULL, 

	[SERVICE_CREATE_DATETIME] [datetime] NOT NULL DEFAULT CURRENT_TIMESTAMP, 
	[SERVICE_CREATE_USER] [nvarchar](150) NOT NULL DEFAULT SUSER_SNAME(), 
	[SERVICE_LAST_MODIFY_DATETIME] [datetime] NOT NULL DEFAULT CURRENT_TIMESTAMP, 
	[SERVICE_LAST_MODIFY_USER] [nvarchar](150) NOT NULL DEFAULT SUSER_SNAME(), 
CONSTRAINT [PK_ReleaseProducts] PRIMARY KEY CLUSTERED (
	[Id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC [sys].[sp_addextendedproperty]	@name=N'MS_Description', @level0type=N'SCHEMA',
	@level0name=N'dbo', @level1type=N'TABLE',
	@level1name=N'ReleaseProducts',
	@value=N'Таблица выпущенных изделий'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReleaseProducts', 
	@level2name = N'Id', 
	@value = N'Уникальный идентификатор'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReleaseProducts', 
	@level2name = N'WorkGuildId', 
	@value = N'Вторичный ключ на таблицу цехов'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReleaseProducts', 
	@level2name = N'FactoryNumber', 
	@value = N'Заводской номер'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReleaseProducts', 
	@level2name = N'ProductId', 
	@value = N'Вторичный ключ на таблицу продуктов'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReleaseProducts', 
	@level2name = N'Month', 
	@value = N'Рабочий месяц'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReleaseProducts', 
	@level2name = N'Year', 
	@value = N'Рабочий год'
GO
-- Ключ уникальности поля [FactoryNumber] и [WorkGuildId]
ALTER TABLE [dbo].[ReleaseProducts]
	ADD CONSTRAINT [UQ_ReleaseProducts_FactoryNumber_And_WorkGuildId_And_MonthYear] UNIQUE NONCLUSTERED ([FactoryNumber], [WorkGuildId], [Month], [Year]) ON [PRIMARY]
GO
-- Вторичный ключ на таблицу [Цехов] 
ALTER TABLE [dbo].[ReleaseProducts] WITH CHECK
ADD CONSTRAINT [FK_ReleaseProductsToWorkGuilds] FOREIGN KEY([WorkGuildId])
REFERENCES [dbo].[WorkGuilds] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO
-- Вторичный ключ на таблицу [Продуктов] 
ALTER TABLE [dbo].[ReleaseProducts] WITH CHECK
ADD CONSTRAINT [FK_ReleaseProductsToProducts] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO
-- Триггер обновления даты-времени последнего редактирования и пользователя изменившего запись последним
CREATE TRIGGER [TriggerUpdateReleaseProducts] ON [ReleaseProducts] 
FOR UPDATE 
AS
	BEGIN 
		IF @@ROWCOUNT = 0 RETURN 
		IF TRIGGER_NESTLEVEL(object_ID('TriggerUpdateReleaseProducts')) > 1 RETURN 
		SET NOCOUNT ON 

		UPDATE [ReleaseProducts] 
		SET [SERVICE_LAST_MODIFY_DATETIME] = CURRENT_TIMESTAMP, [SERVICE_LAST_MODIFY_USER] = SUSER_SNAME() 
		WHERE [id] IN (SELECT DISTINCT [id] FROM [INSERTED]) 
	END
GO

-- Создание таблицы [Вид движения]
USE [ComponentBalanceDb]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MoveTypes] (
	[Id] bigint IDENTITY(1, 1) NOT NULL, 
	[Name] nvarchar(25) NOT NULL,
	[IsView] bit NOT NULL,
CONSTRAINT [PK_MoveTypes] PRIMARY KEY CLUSTERED (
	[Id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC [sys].[sp_addextendedproperty]	@name=N'MS_Description', @level0type=N'SCHEMA',
	@level0name=N'dbo', @level1type=N'TABLE',
	@level1name=N'MoveTypes',
	@value=N'Таблица видов движения'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'MoveTypes', 
	@level2name = N'Id', 
	@value = N'Уникальный идентификатор'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'MoveTypes', 
	@level2name = N'Name', 
	@value = N'Название'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'MoveTypes', 
	@level2name = N'IsView', 
	@value = N'Флаг показывать ли для выбора на добавление'
GO

-- Ключ уникальности поля [Name]
ALTER TABLE [dbo].MoveTypes
	ADD CONSTRAINT [UQ_MoveTypes_Name] UNIQUE NONCLUSTERED ([Name]) ON [PRIMARY]
GO

-- Создание таблицы [Движение]
USE [ComponentBalanceDb]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Moves] (
	[Id] bigint IDENTITY(1, 1) NOT NULL,
	[Discriminator] nvarchar(max) NOT NULL, 
	[BalanceId] bigint NOT NULL,
	[Count] decimal(9,3) NOT NULL,
	[Cost] decimal(9,2) NOT NULL,
	[ReleaseProductId] bigint, -- Для выпуска
	[ProductId] bigint, -- Для прихода со склада
	[TransferBalanceId] bigint, -- Для приход из другого цеха; расхода в другой цех
	[Number] nvarchar(50), -- Для брака; расхода на склад; доработак
	[Month] int NOT NULL,
	[Year] int NOT NULL,

	[IsSupply] bit NOT NULL,
	[IsUserCanDelete] bit NOT NULL,

	[SERVICE_CREATE_DATETIME] [datetime] NOT NULL DEFAULT CURRENT_TIMESTAMP, 
	[SERVICE_CREATE_USER] [nvarchar](150) NOT NULL DEFAULT SUSER_SNAME(), 
	[SERVICE_LAST_MODIFY_DATETIME] [datetime] NOT NULL DEFAULT CURRENT_TIMESTAMP, 
	[SERVICE_LAST_MODIFY_USER] [nvarchar](150) NOT NULL DEFAULT SUSER_SNAME(), 
CONSTRAINT [PK_Moves] PRIMARY KEY CLUSTERED (
	[Id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC [sys].[sp_addextendedproperty]	@name=N'MS_Description', @level0type=N'SCHEMA',
	@level0name=N'dbo', @level1type=N'TABLE',
	@level1name=N'Moves',
	@value=N'Таблица движения деталей'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Moves', 
	@level2name = N'Id', 
	@value = N'Уникальный идентификатор'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Moves', 
	@level2name = N'BalanceId', 
	@value = N'Вторичный ключ на таблицу остатков'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Moves', 
	@level2name = N'Count', 
	@value = N'Количество двигающихся деталей'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Moves', 
	@level2name = N'Cost', 
	@value = N'Цена двигающихся деталей'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Moves', 
	@level2name = N'ReleaseProductId', 
	@value = N'Вторичный ключ на таблицу Выпуска изделий для указания на какое изделие потратилось'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Moves', 
	@level2name = N'ProductId', 
	@value = N'Вторичный ключ на таблицу [Изделий] для указания на какое изделие пришло со склада'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Moves', 
	@level2name = N'TransferBalanceId', 
	@value = N'Вторичный ключ на таблицу [Баланс] куда была перередана деталь'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Moves', 
	@level2name = N'Number', 
	@value = N'Номер - для брака, склада, доработки'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Moves', 
	@level2name = N'Month', 
	@value = N'Рабочий месяц'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Moves', 
	@level2name = N'Year', 
	@value = N'Рабочий год'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Moves', 
	@level2name = N'IsSupply', 
	@value = N'Флаг прихода'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'Moves', 
	@level2name = N'IsUserCanDelete', 
	@value = N'Флаг может ли пользователь удалить запись'
GO

-- Вторичный ключ на таблицу [Остатков] 
ALTER TABLE [dbo].[Moves] WITH CHECK
ADD CONSTRAINT [FK_MovesToBalances] FOREIGN KEY([BalanceId])
REFERENCES [dbo].[Balances] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO
-- Вторичный ключ на таблицу [Выпуск] 
ALTER TABLE [dbo].[Moves] WITH CHECK
ADD CONSTRAINT [FK_MovesToReleaseProducts] FOREIGN KEY([ReleaseProductId])
REFERENCES [dbo].[ReleaseProducts] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO
-- Вторичный ключ на таблицу [Изделий] 
ALTER TABLE [dbo].[Moves] WITH CHECK
ADD CONSTRAINT [FK_MovesToProducts] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO
-- Вторичный ключ на таблицу [Цехов] 
ALTER TABLE [dbo].[Moves] WITH CHECK
ADD CONSTRAINT [FK_MovesToTransferBalances] FOREIGN KEY([TransferBalanceId])
REFERENCES [dbo].[Balances] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO
-- Триггер обновления даты-времени последнего редактирования и пользователя изменившего запись последним
CREATE TRIGGER [TriggerUpdateMoves] ON [Moves] 
FOR UPDATE 
AS
	BEGIN 
		IF @@ROWCOUNT = 0 RETURN 
		IF TRIGGER_NESTLEVEL(object_ID('TriggerUpdateMoves')) > 1 RETURN 
		SET NOCOUNT ON 

		UPDATE [Moves] 
		SET [SERVICE_LAST_MODIFY_DATETIME] = CURRENT_TIMESTAMP, [SERVICE_LAST_MODIFY_USER] = SUSER_SNAME() 
		WHERE [id] IN (SELECT DISTINCT [id] FROM [INSERTED]) 
	END
GO

-- Создание таблицы [Состав изделия]
USE [ComponentBalanceDb]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CompositionProducts] (
	[Id] bigint IDENTITY(1, 1) NOT NULL, 
	[WorkGuildId] bigint NOT NULL,
	[ProductId] bigint NOT NULL,
	[DetailId] bigint NOT NULL,
	[Count] decimal(9,3) NOT NULL,
	[IsCanDeleteInRelease] bit NOT NULL,
	[IsCanUsePart] bit NOT NULL,
CONSTRAINT [PK_CompositionProducts] PRIMARY KEY CLUSTERED (
	[Id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC [sys].[sp_addextendedproperty]	@name=N'MS_Description', @level0type=N'SCHEMA',
	@level0name=N'dbo', @level1type=N'TABLE',
	@level1name=N'CompositionProducts',
	@value=N'Таблица состав изделия'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'CompositionProducts', 
	@level2name = N'Id', 
	@value = N'Уникальный идентификатор'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'CompositionProducts', 
	@level2name = N'WorkGuildId', 
	@value = N'Вторичный ключ на таблицу цехов'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'CompositionProducts', 
	@level2name = N'ProductId', 
	@value = N'Вторичный ключ на таблицу изделий'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'CompositionProducts', 
	@level2name = N'DetailId', 
	@value = N'Вторичный ключ на таблицу деталей'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'CompositionProducts', 
	@level2name = N'Count', 
	@value = N'Количество входящих в изделие деталей'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'CompositionProducts', 
	@level2name = N'IsCanDeleteInRelease', 
	@value = N'Флаг - необязательная деталь'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'CompositionProducts', 
	@level2name = N'IsCanUsePart', 
	@value = N'Флаг - можно списать часть'
GO
-- Вторичный ключ на таблицу [Цехов] 
ALTER TABLE [dbo].[CompositionProducts] WITH CHECK
ADD CONSTRAINT [FK_CompositionProductsToWorkGuilds] FOREIGN KEY([WorkGuildId])
REFERENCES [dbo].[WorkGuilds] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO
-- Вторичный ключ на таблицу [Изделий] 
ALTER TABLE [dbo].[CompositionProducts] WITH CHECK
ADD CONSTRAINT [FK_CompositionProductsToProducts] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO
-- Вторичный ключ на таблицу [Деталей] 
ALTER TABLE [dbo].[CompositionProducts] WITH CHECK
ADD CONSTRAINT [FK_CompositionProductsToDetails] FOREIGN KEY([DetailId])
REFERENCES [dbo].[Details] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO

-- Создание таблицы [Замены в выпуске]
USE [ComponentBalanceDb]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReleaseReplacementDetails] (
	[Id] bigint IDENTITY(1, 1) NOT NULL, 
	[ReleaseProductId] bigint NOT NULL,
	[DetailId] bigint NOT NULL,
	[BalanceId] bigint NOT NULL,
	[Count] decimal(9,3) NOT NULL,
CONSTRAINT [PK_ReleaseReplacementDetails] PRIMARY KEY CLUSTERED (
	[Id] ASC
) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, 
	ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
EXEC [sys].[sp_addextendedproperty]	@name=N'MS_Description', @level0type=N'SCHEMA',
	@level0name=N'dbo', @level1type=N'TABLE',
	@level1name=N'ReleaseReplacementDetails',
	@value=N'Таблица замены в выпуске'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReleaseReplacementDetails', 
	@level2name = N'Id', 
	@value = N'Уникальный идентификатор'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReleaseReplacementDetails', 
	@level2name = N'ReleaseProductId', 
	@value = N'Вторичный ключ на таблицу выпусков'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReleaseReplacementDetails', 
	@level2name = N'DetailId', 
	@value = N'Вторичный ключ на таблицу деталей (основная деталь)'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReleaseReplacementDetails', 
	@level2name = N'BalanceId', 
	@value = N'Вторичный ключ на таблицу балансов (замена основной детали)'
GO
EXEC [sys].[sp_addextendedproperty] @name=N'MS_Description', @level0type=N'SCHEMA', 
	@level0name = N'dbo', @level1type = N'TABLE', @level2type = N'COLUMN', 
	@level1name = N'ReleaseReplacementDetails', 
	@level2name = N'Count', 
	@value = N'Кол-во деталей на замене'
GO
-- Вторичный ключ на таблицу [Выпуск изделий] 
ALTER TABLE [dbo].[ReleaseReplacementDetails] WITH CHECK
ADD CONSTRAINT [FK_ReleaseReplacementDetailsToReleaseProducts] FOREIGN KEY([ReleaseProductId])
REFERENCES [dbo].[ReleaseProducts] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO
-- Вторичный ключ на таблицу [Детали] 
ALTER TABLE [dbo].[ReleaseReplacementDetails] WITH CHECK
ADD CONSTRAINT [FK_ReleaseReplacementDetailsToDetails] FOREIGN KEY([DetailId])
REFERENCES [dbo].[Details] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO
-- Вторичный ключ на таблицу [Балансов] 
ALTER TABLE [dbo].[ReleaseReplacementDetails] WITH CHECK
ADD CONSTRAINT [FK_ReleaseReplacementDetailsToBalances] FOREIGN KEY([BalanceId])
REFERENCES [dbo].[Balances] ([id])
	ON UPDATE NO ACTION
	ON DELETE NO ACTION
GO

INSERT INTO [MonthYears] ([Month],[Year],[Description]) VALUES (5, 2020, 'Текущий рабочий месяц')
--INSERT INTO [MonthYears] ([Month],[Year],[Description]) VALUES (01, 2018, 'Последнее сохранение в архив')
GO

INSERT INTO [WorkGuilds] ([WorkGuildNumber],[Password],[Lvl]) 
VALUES (2, '2', 0)
INSERT INTO [WorkGuilds] ([WorkGuildNumber],[Password],[Lvl]) 
VALUES (3, '3', 0)
INSERT INTO [WorkGuilds] ([WorkGuildNumber],[Password],[Lvl]) 
VALUES (4, '4', 0)
INSERT INTO [WorkGuilds] ([WorkGuildNumber],[Password],[Lvl]) 
VALUES (5, '5', 0)
INSERT INTO [WorkGuilds] ([WorkGuildNumber],[Password],[Lvl]) 
VALUES (75, '777', 1)
GO

INSERT INTO [MonthCloses] ([WorkGuildId],[IsClose],[Month],[Year]) 
VALUES (1, 0, 5, 2020)
INSERT INTO [MonthCloses] ([WorkGuildId],[IsClose],[Month],[Year]) 
VALUES (2, 0, 5, 2020)
INSERT INTO [MonthCloses] ([WorkGuildId],[IsClose],[Month],[Year]) 
VALUES (3, 0, 5, 2020)
INSERT INTO [MonthCloses] ([WorkGuildId],[IsClose],[Month],[Year]) 
VALUES (4, 0, 5, 2020)
GO

INSERT INTO [MoveTypes] ([Name], [IsView])
     VALUES ('Со склада', 1);
INSERT INTO [MoveTypes] ([Name], [IsView])
     VALUES ('Из другого цеха', 0);
INSERT INTO [MoveTypes] ([Name], [IsView])
     VALUES ('Приход доработка', 0);

INSERT INTO [MoveTypes] ([Name], [IsView])
     VALUES ('Брак', 1);
INSERT INTO [MoveTypes] ([Name], [IsView])
     VALUES ('На склад', 1);
INSERT INTO [MoveTypes] ([Name], [IsView])
     VALUES ('В другой цех', 1);
INSERT INTO [MoveTypes] ([Name], [IsView])
     VALUES ('Докомплектация', 1);
INSERT INTO [MoveTypes] ([Name], [IsView])
     VALUES ('Расход доработка', 0);
INSERT INTO [MoveTypes] ([Name], [IsView])
     VALUES ('Выпуск', 0);
GO

INSERT INTO [TmcTypes] ([Name], [ShortName]) VALUES ('Комплектующие', 'KOM');
INSERT INTO [TmcTypes] ([Name], [ShortName]) VALUES ('Вспомогательные материалы', 'VSP');
INSERT INTO [TmcTypes] ([Name], [ShortName]) VALUES ('Литьё', 'FAB');
INSERT INTO [TmcTypes] ([Name], [ShortName]) VALUES ('Пластмассовые изделия', 'PLA');
INSERT INTO [TmcTypes] ([Name], [ShortName]) VALUES ('Инструменты (58 склад)', 'INS');
GO