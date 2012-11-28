
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 11/05/2012 05:38:07
-- Generated from EDMX file: c:\proj\Yam\YammerServer\TokenStore.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [YamAz];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Tokens]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Tokens];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Tokens'
CREATE TABLE [dbo].[Tokens] (
    [Id] uniqueidentifier  NOT NULL,
    [StoreTypeId] tinyint  NOT NULL,
    [UserId] nvarchar(max)  NOT NULL,
    [TokenContent] nvarchar(max)  NOT NULL,
    [Type] int  NOT NULL,
    [Secret] nvarchar(max)  NULL,
    [ContextContent] nvarchar(max)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Tokens'
ALTER TABLE [dbo].[Tokens]
ADD CONSTRAINT [PK_Tokens]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------