
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 02/24/2019 20:54:09
-- Generated from EDMX file: C:\Playground\sidenotes\SideNotes\SideNotes\Models\SideNotesModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [SideNotes];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Autosaves_Books]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Autosaves] DROP CONSTRAINT [FK_Autosaves_Books];
GO
IF OBJECT_ID(N'[dbo].[FK_Autosaves_Paragraphs]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Autosaves] DROP CONSTRAINT [FK_Autosaves_Paragraphs];
GO
IF OBJECT_ID(N'[dbo].[FK_Autosaves_Users]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Autosaves] DROP CONSTRAINT [FK_Autosaves_Users];
GO
IF OBJECT_ID(N'[dbo].[FK_AvatarLarge]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Avatars] DROP CONSTRAINT [FK_AvatarLarge];
GO
IF OBJECT_ID(N'[dbo].[FK_AvatarMedium]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Avatars] DROP CONSTRAINT [FK_AvatarMedium];
GO
IF OBJECT_ID(N'[dbo].[FK_AvatarPhoto]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Avatars] DROP CONSTRAINT [FK_AvatarPhoto];
GO
IF OBJECT_ID(N'[dbo].[FK_AvatarSmall]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Avatars] DROP CONSTRAINT [FK_AvatarSmall];
GO
IF OBJECT_ID(N'[dbo].[FK_AvatarTiny]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Avatars] DROP CONSTRAINT [FK_AvatarTiny];
GO
IF OBJECT_ID(N'[dbo].[FK_BookmarkOwner]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Bookmarks] DROP CONSTRAINT [FK_BookmarkOwner];
GO
IF OBJECT_ID(N'[dbo].[FK_Bookmarks_Paragraphs]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Bookmarks] DROP CONSTRAINT [FK_Bookmarks_Paragraphs];
GO
IF OBJECT_ID(N'[dbo].[FK_Books_Avatars]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Books] DROP CONSTRAINT [FK_Books_Avatars];
GO
IF OBJECT_ID(N'[dbo].[FK_BuiltInCommentAuthor]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[BuiltInComments] DROP CONSTRAINT [FK_BuiltInCommentAuthor];
GO
IF OBJECT_ID(N'[dbo].[FK_CatalogEntry_Books]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CatalogEntry] DROP CONSTRAINT [FK_CatalogEntry_Books];
GO
IF OBJECT_ID(N'[dbo].[FK_CatalogEntry_Category]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[CatalogEntry] DROP CONSTRAINT [FK_CatalogEntry_Category];
GO
IF OBJECT_ID(N'[dbo].[FK_Chapter_Book]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Chapters] DROP CONSTRAINT [FK_Chapter_Book];
GO
IF OBJECT_ID(N'[dbo].[FK_ChaptersTree]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Chapters] DROP CONSTRAINT [FK_ChaptersTree];
GO
IF OBJECT_ID(N'[dbo].[FK_CommentAuthor]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Comments] DROP CONSTRAINT [FK_CommentAuthor];
GO
IF OBJECT_ID(N'[dbo].[FK_Comments_HeadComments]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Comments] DROP CONSTRAINT [FK_Comments_HeadComments];
GO
IF OBJECT_ID(N'[dbo].[FK_HeadCommentAuthor]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[HeadComments] DROP CONSTRAINT [FK_HeadCommentAuthor];
GO
IF OBJECT_ID(N'[dbo].[FK_Paragraphs_Books]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Paragraphs] DROP CONSTRAINT [FK_Paragraphs_Books];
GO
IF OBJECT_ID(N'[dbo].[FK_Paragraphs_Chapters]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Paragraphs] DROP CONSTRAINT [FK_Paragraphs_Chapters];
GO
IF OBJECT_ID(N'[dbo].[FK_ParentComment]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Comments] DROP CONSTRAINT [FK_ParentComment];
GO
IF OBJECT_ID(N'[dbo].[FK_UserFriends_Friends]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserFriends] DROP CONSTRAINT [FK_UserFriends_Friends];
GO
IF OBJECT_ID(N'[dbo].[FK_UserFriends_Users]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UserFriends] DROP CONSTRAINT [FK_UserFriends_Users];
GO
IF OBJECT_ID(N'[dbo].[FK_Users_Avatars]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Users] DROP CONSTRAINT [FK_Users_Avatars];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Autosaves]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Autosaves];
GO
IF OBJECT_ID(N'[dbo].[Avatars]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Avatars];
GO
IF OBJECT_ID(N'[dbo].[Bookmarks]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Bookmarks];
GO
IF OBJECT_ID(N'[dbo].[Books]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Books];
GO
IF OBJECT_ID(N'[dbo].[BuiltInComments]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BuiltInComments];
GO
IF OBJECT_ID(N'[dbo].[CatalogEntry]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CatalogEntry];
GO
IF OBJECT_ID(N'[dbo].[Category]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Category];
GO
IF OBJECT_ID(N'[dbo].[Chapters]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Chapters];
GO
IF OBJECT_ID(N'[dbo].[Comments]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Comments];
GO
IF OBJECT_ID(N'[dbo].[DailyHits]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DailyHits];
GO
IF OBJECT_ID(N'[dbo].[DailyHitsArchive]', 'U') IS NOT NULL
    DROP TABLE [dbo].[DailyHitsArchive];
GO
IF OBJECT_ID(N'[dbo].[HeadComments]', 'U') IS NOT NULL
    DROP TABLE [dbo].[HeadComments];
GO
IF OBJECT_ID(N'[dbo].[Notifications]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Notifications];
GO
IF OBJECT_ID(N'[dbo].[Paragraphs]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Paragraphs];
GO
IF OBJECT_ID(N'[dbo].[Photos]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Photos];
GO
IF OBJECT_ID(N'[dbo].[RequestLog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RequestLog];
GO
IF OBJECT_ID(N'[dbo].[SentNotifications]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SentNotifications];
GO
IF OBJECT_ID(N'[dbo].[TempAnonymousComments]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TempAnonymousComments];
GO
IF OBJECT_ID(N'[dbo].[UserFriends]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserFriends];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Avatars'
CREATE TABLE [dbo].[Avatars] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Original_Id] int  NOT NULL,
    [Large_Id] int  NOT NULL,
    [Small_Id] int  NOT NULL,
    [Medium_Id] int  NULL,
    [Tiny_Id] int  NULL
);
GO

-- Creating table 'Bookmarks'
CREATE TABLE [dbo].[Bookmarks] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Owner_Id] int  NOT NULL,
    [Paragraph_Id] int  NOT NULL
);
GO

-- Creating table 'Books'
CREATE TABLE [dbo].[Books] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Title] nvarchar(max)  NOT NULL,
    [Annotation] nvarchar(max)  NOT NULL,
    [ISBN] nvarchar(max)  NOT NULL,
    [Author] nvarchar(max)  NOT NULL,
    [Avatar_Id] int  NULL,
    [AuthorsEmail] nvarchar(max)  NULL,
    [HashTag] nvarchar(140)  NULL,
    [Popularity] int  NOT NULL,
    [PropertyStatus] int  NOT NULL,
    [DonationMessage] nvarchar(max)  NULL,
    [DonationForm] nvarchar(max)  NULL,
    [CustomStyles] nvarchar(max)  NULL,
    [MetaKeywords] nvarchar(max)  NULL,
    [MetaDescription] nvarchar(max)  NULL,
	[UrlName] nvarchar(150)  NULL,
	[AnnotatorUrlName] nvarchar(150)  NULL
);
GO

-- Creating table 'BuiltInComments'
CREATE TABLE [dbo].[BuiltInComments] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ContentNumber] int  NOT NULL,
    [EntityId] int  NOT NULL,
    [EntityType] int  NOT NULL,
    [Author_Id] int  NOT NULL
);
GO

-- Creating table 'Chapters'
CREATE TABLE [dbo].[Chapters] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Title] nvarchar(max)  NOT NULL,
    [OrderNumber] int  NOT NULL,
    [ParentChapter_Id] int  NULL,
    [TopParagraph_Id] int  NULL,
    [Book_Id] int  NULL,
    [HeadCommentsCount] int  NOT NULL
);
GO

-- Creating table 'Paragraphs'
CREATE TABLE [dbo].[Paragraphs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [OrderNumber] int  NOT NULL,
    [Content] nvarchar(max)  NULL,
    [Book_Id] int  NOT NULL,
    [ChapterId] int  NOT NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Login] nvarchar(200)  NOT NULL,
    [PasswordHash] varbinary(max)  NULL,
    [PasswordSeed] varbinary(max)  NULL,
    [Email] nvarchar(max)  NULL,
    [FacebookProfileUrl] nvarchar(max)  NULL,
    [Avatar_Id] int  NULL,
    [AccountSourceInt] int  NOT NULL,
    [IsAdmin] bit  NOT NULL,
    [IsFamous] bit  NOT NULL,
    [NotifyAuthorCommentReplied] bit  NOT NULL,
    [FacebookId] nvarchar(50)  NULL,
    [TwitterId] nvarchar(50)  NULL,
    [AccessTokenTwitter] varchar(1024)  NULL,
    [AccessTokenFacebook] varchar(1024)  NULL,
    [FacebookUsername] nvarchar(255)  NULL,
    [TwitterProfileUrl] nvarchar(max)  NULL,
    [TwitterUsername] nvarchar(255)  NULL,
    [LastLoginSourceInt] int  NOT NULL,
	[UrlName] nvarchar(150)  NULL
);
GO

-- Creating table 'Photos'
CREATE TABLE [dbo].[Photos] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Location] nvarchar(max)  NULL,
    [Url] nvarchar(max)  NULL,
    [width] int  NOT NULL,
    [height] int  NOT NULL
);
GO

-- Creating table 'Autosaves'
CREATE TABLE [dbo].[Autosaves] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Book_Id] int  NOT NULL,
    [Owner_Id] int  NOT NULL,
    [Paragraph_Id] int  NOT NULL,
    [DateUpdated] datetime  NOT NULL
);
GO

-- Creating table 'Comments'
CREATE TABLE [dbo].[Comments] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Text] nvarchar(max)  NOT NULL,
    [DateCreated] datetime  NOT NULL,
    [Author_Id] int  NULL,
    [ParentCommentId] int  NULL,
    [HeadCommentId] int  NOT NULL
);
GO

-- Creating table 'HeadComments'
CREATE TABLE [dbo].[HeadComments] (
    [Id] int  IDENTITY(1,1) NOT NULL,
    [Text] nvarchar(max)  NULL,
    [DateCreated] datetime  NOT NULL,
    [Author_Id] int  NULL,
    [IsPrivate] bit  NOT NULL,
    [EntityId] int  NOT NULL,
    [EntityType] int  NOT NULL
);
GO

-- Creating table 'Notifications'
CREATE TABLE [dbo].[Notifications] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Subject] nvarchar(1024)  NULL,
    [Body] nvarchar(4000)  NULL,
    [Email] nvarchar(255)  NULL
);
GO

-- Creating table 'SentNotifications'
CREATE TABLE [dbo].[SentNotifications] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Subject] nvarchar(1024)  NULL,
    [Body] nvarchar(4000)  NULL,
    [Email] nvarchar(255)  NULL,
    [Date] datetime  NULL
);
GO

-- Creating table 'Categories'
CREATE TABLE [dbo].[Categories] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(200)  NULL,
    [ParentId] int  NULL
);
GO

-- Creating table 'RequestLogs'
CREATE TABLE [dbo].[RequestLogs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RequestDate] datetime  NOT NULL,
    [Referer] nvarchar(1024)  NULL,
    [Url] nvarchar(1024)  NULL,
    [HttpFrom] nvarchar(1024)  NULL,
    [Remote_addr] varchar(20)  NULL,
    [CurrentUser] nvarchar(1024)  NULL
);
GO

-- Creating table 'DailyHits'
CREATE TABLE [dbo].[DailyHits] (
    [CampaignId] nvarchar(100)  NOT NULL,
    [hits] int  NOT NULL
);
GO

-- Creating table 'DailyHitsArchives'
CREATE TABLE [dbo].[DailyHitsArchives] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [CampaignId] nvarchar(100)  NOT NULL,
    [hits] int  NOT NULL,
    [date] datetime  NOT NULL
);
GO

-- Creating table 'TempAnonymousComments'
CREATE TABLE [dbo].[TempAnonymousComments] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Text] nvarchar(max)  NULL,
    [EntityId] int  NOT NULL,
    [EntityType] int  NOT NULL,
    [isPrivate] bit  NOT NULL
);
GO

-- Creating table 'Paragraphs_ImageParagraph'
CREATE TABLE [dbo].[Paragraphs_ImageParagraph] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'Paragraphs_EmptyLine'
CREATE TABLE [dbo].[Paragraphs_EmptyLine] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'Paragraphs_Title'
CREATE TABLE [dbo].[Paragraphs_Title] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'Paragraphs_Epigraph'
CREATE TABLE [dbo].[Paragraphs_Epigraph] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'Paragraphs_EpigraphAuthor'
CREATE TABLE [dbo].[Paragraphs_EpigraphAuthor] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'Paragraphs_Cite'
CREATE TABLE [dbo].[Paragraphs_Cite] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'Paragraphs_CiteAuthor'
CREATE TABLE [dbo].[Paragraphs_CiteAuthor] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'Paragraphs_Poem'
CREATE TABLE [dbo].[Paragraphs_Poem] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'Paragraphs_PoemAuthor'
CREATE TABLE [dbo].[Paragraphs_PoemAuthor] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'Paragraphs_FootNote'
CREATE TABLE [dbo].[Paragraphs_FootNote] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'UserFriends'
CREATE TABLE [dbo].[UserFriends] (
    [UserFriends_User1_Id] int  NOT NULL,
    [Friends_Id] int  NOT NULL
);
GO

-- Creating table 'CatalogEntry'
CREATE TABLE [dbo].[CatalogEntry] (
    [Books_Id] int  NOT NULL,
    [Categories_Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Avatars'
ALTER TABLE [dbo].[Avatars]
ADD CONSTRAINT [PK_Avatars]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Bookmarks'
ALTER TABLE [dbo].[Bookmarks]
ADD CONSTRAINT [PK_Bookmarks]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Books'
ALTER TABLE [dbo].[Books]
ADD CONSTRAINT [PK_Books]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'BuiltInComments'
ALTER TABLE [dbo].[BuiltInComments]
ADD CONSTRAINT [PK_BuiltInComments]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Chapters'
ALTER TABLE [dbo].[Chapters]
ADD CONSTRAINT [PK_Chapters]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Paragraphs'
ALTER TABLE [dbo].[Paragraphs]
ADD CONSTRAINT [PK_Paragraphs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Photos'
ALTER TABLE [dbo].[Photos]
ADD CONSTRAINT [PK_Photos]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Autosaves'
ALTER TABLE [dbo].[Autosaves]
ADD CONSTRAINT [PK_Autosaves]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Comments'
ALTER TABLE [dbo].[Comments]
ADD CONSTRAINT [PK_Comments]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'HeadComments'
ALTER TABLE [dbo].[HeadComments]
ADD CONSTRAINT [PK_HeadComments]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Notifications'
ALTER TABLE [dbo].[Notifications]
ADD CONSTRAINT [PK_Notifications]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'SentNotifications'
ALTER TABLE [dbo].[SentNotifications]
ADD CONSTRAINT [PK_SentNotifications]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Categories'
ALTER TABLE [dbo].[Categories]
ADD CONSTRAINT [PK_Categories]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'RequestLogs'
ALTER TABLE [dbo].[RequestLogs]
ADD CONSTRAINT [PK_RequestLogs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [CampaignId] in table 'DailyHits'
ALTER TABLE [dbo].[DailyHits]
ADD CONSTRAINT [PK_DailyHits]
    PRIMARY KEY CLUSTERED ([CampaignId] ASC);
GO

-- Creating primary key on [Id] in table 'DailyHitsArchives'
ALTER TABLE [dbo].[DailyHitsArchives]
ADD CONSTRAINT [PK_DailyHitsArchives]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TempAnonymousComments'
ALTER TABLE [dbo].[TempAnonymousComments]
ADD CONSTRAINT [PK_TempAnonymousComments]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Paragraphs_ImageParagraph'
ALTER TABLE [dbo].[Paragraphs_ImageParagraph]
ADD CONSTRAINT [PK_Paragraphs_ImageParagraph]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Paragraphs_EmptyLine'
ALTER TABLE [dbo].[Paragraphs_EmptyLine]
ADD CONSTRAINT [PK_Paragraphs_EmptyLine]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Paragraphs_Title'
ALTER TABLE [dbo].[Paragraphs_Title]
ADD CONSTRAINT [PK_Paragraphs_Title]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Paragraphs_Epigraph'
ALTER TABLE [dbo].[Paragraphs_Epigraph]
ADD CONSTRAINT [PK_Paragraphs_Epigraph]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Paragraphs_EpigraphAuthor'
ALTER TABLE [dbo].[Paragraphs_EpigraphAuthor]
ADD CONSTRAINT [PK_Paragraphs_EpigraphAuthor]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Paragraphs_Cite'
ALTER TABLE [dbo].[Paragraphs_Cite]
ADD CONSTRAINT [PK_Paragraphs_Cite]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Paragraphs_CiteAuthor'
ALTER TABLE [dbo].[Paragraphs_CiteAuthor]
ADD CONSTRAINT [PK_Paragraphs_CiteAuthor]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Paragraphs_Poem'
ALTER TABLE [dbo].[Paragraphs_Poem]
ADD CONSTRAINT [PK_Paragraphs_Poem]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Paragraphs_PoemAuthor'
ALTER TABLE [dbo].[Paragraphs_PoemAuthor]
ADD CONSTRAINT [PK_Paragraphs_PoemAuthor]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Paragraphs_FootNote'
ALTER TABLE [dbo].[Paragraphs_FootNote]
ADD CONSTRAINT [PK_Paragraphs_FootNote]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [UserFriends_User1_Id], [Friends_Id] in table 'UserFriends'
ALTER TABLE [dbo].[UserFriends]
ADD CONSTRAINT [PK_UserFriends]
    PRIMARY KEY CLUSTERED ([UserFriends_User1_Id], [Friends_Id] ASC);
GO

-- Creating primary key on [Books_Id], [Categories_Id] in table 'CatalogEntry'
ALTER TABLE [dbo].[CatalogEntry]
ADD CONSTRAINT [PK_CatalogEntry]
    PRIMARY KEY CLUSTERED ([Books_Id], [Categories_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Avatar_Id] in table 'Books'
ALTER TABLE [dbo].[Books]
ADD CONSTRAINT [FK_Books_Avatars]
    FOREIGN KEY ([Avatar_Id])
    REFERENCES [dbo].[Avatars]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Books_Avatars'
CREATE INDEX [IX_FK_Books_Avatars]
ON [dbo].[Books]
    ([Avatar_Id]);
GO

-- Creating foreign key on [Paragraph_Id] in table 'Bookmarks'
ALTER TABLE [dbo].[Bookmarks]
ADD CONSTRAINT [FK_Bookmarks_Paragraphs]
    FOREIGN KEY ([Paragraph_Id])
    REFERENCES [dbo].[Paragraphs]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Bookmarks_Paragraphs'
CREATE INDEX [IX_FK_Bookmarks_Paragraphs]
ON [dbo].[Bookmarks]
    ([Paragraph_Id]);
GO

-- Creating foreign key on [Book_Id] in table 'Paragraphs'
ALTER TABLE [dbo].[Paragraphs]
ADD CONSTRAINT [FK_Paragraphs_Books]
    FOREIGN KEY ([Book_Id])
    REFERENCES [dbo].[Books]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Paragraphs_Books'
CREATE INDEX [IX_FK_Paragraphs_Books]
ON [dbo].[Paragraphs]
    ([Book_Id]);
GO

-- Creating foreign key on [ParentChapter_Id] in table 'Chapters'
ALTER TABLE [dbo].[Chapters]
ADD CONSTRAINT [FK_ChaptersTree]
    FOREIGN KEY ([ParentChapter_Id])
    REFERENCES [dbo].[Chapters]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ChaptersTree'
CREATE INDEX [IX_FK_ChaptersTree]
ON [dbo].[Chapters]
    ([ParentChapter_Id]);
GO

-- Creating foreign key on [TopParagraph_Id] in table 'Chapters'
ALTER TABLE [dbo].[Chapters]
ADD CONSTRAINT [FK_TopParagraph]
    FOREIGN KEY ([TopParagraph_Id])
    REFERENCES [dbo].[Paragraphs]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TopParagraph'
CREATE INDEX [IX_FK_TopParagraph]
ON [dbo].[Chapters]
    ([TopParagraph_Id]);
GO

-- Creating foreign key on [Avatar_Id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [FK_Users_Avatars]
    FOREIGN KEY ([Avatar_Id])
    REFERENCES [dbo].[Avatars]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Users_Avatars'
CREATE INDEX [IX_FK_Users_Avatars]
ON [dbo].[Users]
    ([Avatar_Id]);
GO

-- Creating foreign key on [Owner_Id] in table 'Bookmarks'
ALTER TABLE [dbo].[Bookmarks]
ADD CONSTRAINT [FK_BookmarkOwner]
    FOREIGN KEY ([Owner_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BookmarkOwner'
CREATE INDEX [IX_FK_BookmarkOwner]
ON [dbo].[Bookmarks]
    ([Owner_Id]);
GO

-- Creating foreign key on [UserFriends_User1_Id] in table 'UserFriends'
ALTER TABLE [dbo].[UserFriends]
ADD CONSTRAINT [FK_UserFriends_User]
    FOREIGN KEY ([UserFriends_User1_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Friends_Id] in table 'UserFriends'
ALTER TABLE [dbo].[UserFriends]
ADD CONSTRAINT [FK_UserFriends_User1]
    FOREIGN KEY ([Friends_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserFriends_User1'
CREATE INDEX [IX_FK_UserFriends_User1]
ON [dbo].[UserFriends]
    ([Friends_Id]);
GO

-- Creating foreign key on [Large_Id] in table 'Avatars'
ALTER TABLE [dbo].[Avatars]
ADD CONSTRAINT [FK_AvatarLarge]
    FOREIGN KEY ([Large_Id])
    REFERENCES [dbo].[Photos]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AvatarLarge'
CREATE INDEX [IX_FK_AvatarLarge]
ON [dbo].[Avatars]
    ([Large_Id]);
GO

-- Creating foreign key on [Original_Id] in table 'Avatars'
ALTER TABLE [dbo].[Avatars]
ADD CONSTRAINT [FK_AvatarPhoto]
    FOREIGN KEY ([Original_Id])
    REFERENCES [dbo].[Photos]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AvatarPhoto'
CREATE INDEX [IX_FK_AvatarPhoto]
ON [dbo].[Avatars]
    ([Original_Id]);
GO

-- Creating foreign key on [Small_Id] in table 'Avatars'
ALTER TABLE [dbo].[Avatars]
ADD CONSTRAINT [FK_AvatarSmall]
    FOREIGN KEY ([Small_Id])
    REFERENCES [dbo].[Photos]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AvatarSmall'
CREATE INDEX [IX_FK_AvatarSmall]
ON [dbo].[Avatars]
    ([Small_Id]);
GO

-- Creating foreign key on [Book_Id] in table 'Chapters'
ALTER TABLE [dbo].[Chapters]
ADD CONSTRAINT [FK_Chapter_Book]
    FOREIGN KEY ([Book_Id])
    REFERENCES [dbo].[Books]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Chapter_Book'
CREATE INDEX [IX_FK_Chapter_Book]
ON [dbo].[Chapters]
    ([Book_Id]);
GO

-- Creating foreign key on [Book_Id] in table 'Autosaves'
ALTER TABLE [dbo].[Autosaves]
ADD CONSTRAINT [FK_Autosaves_Books]
    FOREIGN KEY ([Book_Id])
    REFERENCES [dbo].[Books]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Autosaves_Books'
CREATE INDEX [IX_FK_Autosaves_Books]
ON [dbo].[Autosaves]
    ([Book_Id]);
GO

-- Creating foreign key on [Paragraph_Id] in table 'Autosaves'
ALTER TABLE [dbo].[Autosaves]
ADD CONSTRAINT [FK_Autosaves_Paragraphs]
    FOREIGN KEY ([Paragraph_Id])
    REFERENCES [dbo].[Paragraphs]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Autosaves_Paragraphs'
CREATE INDEX [IX_FK_Autosaves_Paragraphs]
ON [dbo].[Autosaves]
    ([Paragraph_Id]);
GO

-- Creating foreign key on [Owner_Id] in table 'Autosaves'
ALTER TABLE [dbo].[Autosaves]
ADD CONSTRAINT [FK_Autosaves_Users]
    FOREIGN KEY ([Owner_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Autosaves_Users'
CREATE INDEX [IX_FK_Autosaves_Users]
ON [dbo].[Autosaves]
    ([Owner_Id]);
GO

-- Creating foreign key on [Author_Id] in table 'BuiltInComments'
ALTER TABLE [dbo].[BuiltInComments]
ADD CONSTRAINT [FK_BuiltInCommentAuthor]
    FOREIGN KEY ([Author_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_BuiltInCommentAuthor'
CREATE INDEX [IX_FK_BuiltInCommentAuthor]
ON [dbo].[BuiltInComments]
    ([Author_Id]);
GO

-- Creating foreign key on [Author_Id] in table 'Comments'
ALTER TABLE [dbo].[Comments]
ADD CONSTRAINT [FK_CommentAuthor]
    FOREIGN KEY ([Author_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CommentAuthor'
CREATE INDEX [IX_FK_CommentAuthor]
ON [dbo].[Comments]
    ([Author_Id]);
GO

-- Creating foreign key on [HeadCommentId] in table 'Comments'
ALTER TABLE [dbo].[Comments]
ADD CONSTRAINT [FK_Comments_HeadComments]
    FOREIGN KEY ([HeadCommentId])
    REFERENCES [dbo].[HeadComments]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Comments_HeadComments'
CREATE INDEX [IX_FK_Comments_HeadComments]
ON [dbo].[Comments]
    ([HeadCommentId]);
GO

-- Creating foreign key on [ParentCommentId] in table 'Comments'
ALTER TABLE [dbo].[Comments]
ADD CONSTRAINT [FK_ParentComment]
    FOREIGN KEY ([ParentCommentId])
    REFERENCES [dbo].[Comments]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ParentComment'
CREATE INDEX [IX_FK_ParentComment]
ON [dbo].[Comments]
    ([ParentCommentId]);
GO

-- Creating foreign key on [Author_Id] in table 'HeadComments'
ALTER TABLE [dbo].[HeadComments]
ADD CONSTRAINT [FK_HeadCommentAuthor]
    FOREIGN KEY ([Author_Id])
    REFERENCES [dbo].[Users]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_HeadCommentAuthor'
CREATE INDEX [IX_FK_HeadCommentAuthor]
ON [dbo].[HeadComments]
    ([Author_Id]);
GO

-- Creating foreign key on [Medium_Id] in table 'Avatars'
ALTER TABLE [dbo].[Avatars]
ADD CONSTRAINT [FK_AvatarMedium]
    FOREIGN KEY ([Medium_Id])
    REFERENCES [dbo].[Photos]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AvatarMedium'
CREATE INDEX [IX_FK_AvatarMedium]
ON [dbo].[Avatars]
    ([Medium_Id]);
GO

-- Creating foreign key on [Tiny_Id] in table 'Avatars'
ALTER TABLE [dbo].[Avatars]
ADD CONSTRAINT [FK_AvatarTiny]
    FOREIGN KEY ([Tiny_Id])
    REFERENCES [dbo].[Photos]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_AvatarTiny'
CREATE INDEX [IX_FK_AvatarTiny]
ON [dbo].[Avatars]
    ([Tiny_Id]);
GO

-- Creating foreign key on [ChapterId] in table 'Paragraphs'
ALTER TABLE [dbo].[Paragraphs]
ADD CONSTRAINT [FK_Paragraphs_Chapters]
    FOREIGN KEY ([ChapterId])
    REFERENCES [dbo].[Chapters]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Paragraphs_Chapters'
CREATE INDEX [IX_FK_Paragraphs_Chapters]
ON [dbo].[Paragraphs]
    ([ChapterId]);
GO

-- Creating foreign key on [Books_Id] in table 'CatalogEntry'
ALTER TABLE [dbo].[CatalogEntry]
ADD CONSTRAINT [FK_CatalogEntry_Book]
    FOREIGN KEY ([Books_Id])
    REFERENCES [dbo].[Books]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Categories_Id] in table 'CatalogEntry'
ALTER TABLE [dbo].[CatalogEntry]
ADD CONSTRAINT [FK_CatalogEntry_Category]
    FOREIGN KEY ([Categories_Id])
    REFERENCES [dbo].[Categories]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CatalogEntry_Category'
CREATE INDEX [IX_FK_CatalogEntry_Category]
ON [dbo].[CatalogEntry]
    ([Categories_Id]);
GO

-- Creating foreign key on [Id] in table 'Paragraphs_ImageParagraph'
ALTER TABLE [dbo].[Paragraphs_ImageParagraph]
ADD CONSTRAINT [FK_ImageParagraph_inherits_Paragraph]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Paragraphs]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'Paragraphs_EmptyLine'
ALTER TABLE [dbo].[Paragraphs_EmptyLine]
ADD CONSTRAINT [FK_EmptyLine_inherits_Paragraph]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Paragraphs]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'Paragraphs_Title'
ALTER TABLE [dbo].[Paragraphs_Title]
ADD CONSTRAINT [FK_Title_inherits_Paragraph]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Paragraphs]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'Paragraphs_Epigraph'
ALTER TABLE [dbo].[Paragraphs_Epigraph]
ADD CONSTRAINT [FK_Epigraph_inherits_Paragraph]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Paragraphs]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'Paragraphs_EpigraphAuthor'
ALTER TABLE [dbo].[Paragraphs_EpigraphAuthor]
ADD CONSTRAINT [FK_EpigraphAuthor_inherits_Paragraph]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Paragraphs]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'Paragraphs_Cite'
ALTER TABLE [dbo].[Paragraphs_Cite]
ADD CONSTRAINT [FK_Cite_inherits_Paragraph]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Paragraphs]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'Paragraphs_CiteAuthor'
ALTER TABLE [dbo].[Paragraphs_CiteAuthor]
ADD CONSTRAINT [FK_CiteAuthor_inherits_Paragraph]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Paragraphs]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'Paragraphs_Poem'
ALTER TABLE [dbo].[Paragraphs_Poem]
ADD CONSTRAINT [FK_Poem_inherits_Paragraph]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Paragraphs]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'Paragraphs_PoemAuthor'
ALTER TABLE [dbo].[Paragraphs_PoemAuthor]
ADD CONSTRAINT [FK_PoemAuthor_inherits_Paragraph]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Paragraphs]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'Paragraphs_FootNote'
ALTER TABLE [dbo].[Paragraphs_FootNote]
ADD CONSTRAINT [FK_FootNote_inherits_Paragraph]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Paragraphs]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------