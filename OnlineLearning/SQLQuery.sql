BEGIN
CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                   NVARCHAR (128)   NOT NULL,
    [Name]                 NVARCHAR (MAX)   NULL,
    [Code]                 NVARCHAR (MAX)   NULL,
    [Gender]               NVARCHAR (MAX)   NULL,
    [IntakeYear]           INT              NULL,
    [ProfilePicName]       NVARCHAR (MAX)   NULL,
    [ProfilePicPath]       NVARCHAR (MAX)   NULL,
    [ApproveAdminId]       NVARCHAR (128)  NULL,
    [UpdateAdminId]        NVARCHAR (128)  NULL,
    [UpdateDetail]         NVARCHAR (MAX)   NULL,
    [UserStatus]           NVARCHAR (MAX)   NULL,
    [RegisterOn]           DATETIME         NULL,
    [ApprovedOn]           DATETIME         NULL,
    [UpdateOn]             DATETIME         NULL,
    [Email]                NVARCHAR (256)   NULL,
    [EmailConfirmed]       BIT              NOT NULL,
    [PasswordHash]         NVARCHAR (MAX)   NULL,
    [SecurityStamp]        NVARCHAR (MAX)   NULL,
    [PhoneNumber]          NVARCHAR (MAX)   NULL,
    [PhoneNumberConfirmed] BIT              NOT NULL,
    [TwoFactorEnabled]     BIT              NOT NULL,
    [LockoutEndDateUtc]    DATETIME         NULL,
    [LockoutEnabled]       BIT              NOT NULL,
    [AccessFailedCount]    INT              NOT NULL,
    [UserName]             NVARCHAR (256)   NOT NULL,
    [Biography]            NVARCHAR (max)   NULL
    CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);
END

GO
BEGIN
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers]([UserName] ASC);
	END

BEGIN	
CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [UserId]     NVARCHAR (128) NOT NULL,
    [ClaimType]  NVARCHAR (MAX) NULL,
    [ClaimValue] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);
END

GO
BEGIN
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[AspNetUserClaims]([UserId] ASC);
    END

BEGIN
CREATE TABLE [dbo].[AspNetUserLogins] (
    [LoginProvider] NVARCHAR (128) NOT NULL,
    [ProviderKey]   NVARCHAR (128) NOT NULL,
    [UserId]        NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC, [UserId] ASC),
    CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);
END

GO
BEGIN
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[AspNetUserLogins]([UserId] ASC);
    END

BEGIN
CREATE TABLE [dbo].[AspNetRoles] (
    [Id]   NVARCHAR (128) NOT NULL,
    [Name] NVARCHAR (256) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
);
END

GO
BEGIN
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex]
    ON [dbo].[AspNetRoles]([Name] ASC);
    END

BEGIN
CREATE TABLE [dbo].[AspNetUserRoles] (
    [UserId] NVARCHAR (128) NOT NULL,
    [RoleId] NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);
END

GO
BEGIN
CREATE NONCLUSTERED INDEX [IX_UserId]
    ON [dbo].[AspNetUserRoles]([UserId] ASC);
    END


GO
BEGIN
CREATE NONCLUSTERED INDEX [IX_RoleId]
    ON [dbo].[AspNetUserRoles]([RoleId] ASC);
END


BEGIN
insert into AspNetRoles (Id , Name) values (newid(),'Admin');
insert into AspNetRoles (Id , Name) values (newid(),'Teacher');
insert into AspNetRoles (Id , Name) values (newid(),'Student');
END

BEGIN
CREATE TABLE [dbo].[Setting] (
	[Id]	uniqueidentifier NOT NULL,	
	[UserId]	NVARCHAR(128)  NULL,
	[Background]	NVARCHAR(max)  NULL
    CONSTRAINT [PK_dbo.Setting] PRIMARY KEY CLUSTERED ([Id] ASC)
);
CREATE TABLE [dbo].[CourseChannel] (
	[Id]	uniqueidentifier NOT NULL,	
	[CreatorUserId]	NVARCHAR(max)  NULL,
	[CreatedOn]	datetime  NULL,
	[Name]	NVARCHAR (max)  NULL,
	[Description]	NVARCHAR (max)  NULL,
	[ThemeId]	uniqueidentifier  NULL
    CONSTRAINT [PK_dbo.CourseChannel] PRIMARY KEY CLUSTERED ([Id] ASC)
);
CREATE TABLE [dbo].[Theme] (
	[Id]	uniqueidentifier NOT NULL,	
	[CategoryId]	uniqueidentifier  NULL,
	[ImageSource]	NVARCHAR (max)  NULL,	
	[ImageName]	NVARCHAR (max)  NULL
    CONSTRAINT [PK_dbo.Theme] PRIMARY KEY CLUSTERED ([Id] ASC)
);
CREATE TABLE [dbo].[ThemeCategory] (
	[CateogyId]	uniqueidentifier NOT NULL,	
	[CategoryName]	NVARCHAR (max)  NULL,
	[Code]	NVARCHAR (max)  NULL
    CONSTRAINT [PK_dbo.ThemeCategory] PRIMARY KEY CLUSTERED ([CateogyId] ASC)
);
CREATE TABLE [dbo].[StudentCourseChannel] (
    [StudentId] NVARCHAR (128) NOT NULL,
    [CourseChannelId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_dbo.StudentCourseChannel] PRIMARY KEY CLUSTERED ([StudentId] ASC, [CourseChannelId] ASC),
    CONSTRAINT [FK_dbo.StudentCourseChannel_dbo.CourseChannel_CourseChannelId] FOREIGN KEY ([CourseChannelId]) REFERENCES [dbo].[CourseChannel] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.StudentCourseChannel_dbo.AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);
END

BEGIN
CREATE TABLE [dbo].[Chapter] (
	[Id]	uniqueidentifier NOT NULL,	
	[CourseChannelId]	uniqueidentifier  NULL,
	[Title]	NVARCHAR (max)  NULL,	
	[CreatorId]	NVARCHAR (128)  NULL,
	[CreatedOn]	datetime  NULL,
	[LastUpdate]	datetime  NULL
    CONSTRAINT [PK_dbo.Chapter] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.Chapter_dbo.CourseChannel_CourseChannelId] FOREIGN KEY ([CourseChannelId]) REFERENCES [dbo].[CourseChannel] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [dbo].[FileMaterial] (
	[Id]	uniqueidentifier NOT NULL,	
	[ChapterId]	uniqueidentifier  NULL,
	[FileName]	NVARCHAR (max)  NULL,	
	[CreatorId]	NVARCHAR (128)  NULL,
	[LastUpdate]	datetime  NULL
    CONSTRAINT [PK_dbo.FileMaterial] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.FileMaterial_dbo.Chapter_ChapterId] FOREIGN KEY ([ChapterId]) REFERENCES [dbo].[Chapter] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [dbo].[TextMaterial] (
	[Id]	uniqueidentifier NOT NULL,	
	[ChapterId]	uniqueidentifier  NULL,
	[Detail]	NVARCHAR (max)  NULL,
	[CreatorId]	NVARCHAR (128)  NULL,
	[LastUpdate]	datetime  NULL
    CONSTRAINT [PK_dbo.TextMaterial] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.TextMaterial_dbo.Chapter_ChapterId] FOREIGN KEY ([ChapterId]) REFERENCES [dbo].[Chapter] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [dbo].[Announcement] (
	[Id]	uniqueidentifier NOT NULL,	
	[CourseChannelId]	uniqueidentifier  NULL,
	[Title]	NVARCHAR (max)  NULL,	
	[Detail]	NVARCHAR (max)  NULL,
	[Type]	NVARCHAR (max)  NULL,	
	[CreatorId]	NVARCHAR (128)  NULL,
	[LastUpdate]	datetime  NULL,
	[CreatedOn]	datetime  NULL,
	[UpdatedBy]	NVARCHAR (128)  NULL
    CONSTRAINT [PK_dbo.Announcement] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.Announcement_dbo.CourseChannel_CourseChannelId] FOREIGN KEY ([CourseChannelId]) REFERENCES [dbo].[CourseChannel] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [dbo].[Assignment] (
	[Id]	uniqueidentifier NOT NULL,	
	[CourseChannelId]	uniqueidentifier  NULL,
	[FileName]	NVARCHAR (max)  NULL,	
	[Title]	NVARCHAR (max)  NULL,
	[Instruction]	NVARCHAR (max)  NULL,	
	[Type]	NVARCHAR (max)  NULL,
	[MemberMaxCount]	int  NULL,
	[DueDate]	datetime  NULL,
	[TeacherId]	NVARCHAR (128)  NULL,
	[LastUpdate]	datetime  NULL
    CONSTRAINT [PK_dbo.Assignment] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.Assignment_dbo.CourseChannel_CourseChannelId] FOREIGN KEY ([CourseChannelId]) REFERENCES [dbo].[CourseChannel] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [dbo].[AssignmentGroup] (
	[Id]	uniqueidentifier  NOT NULL,	
	[AssignmentId]	uniqueidentifier  NULL,
	[GroupName] NVARCHAR (max)  NULL
    CONSTRAINT [PK_dbo.AssignmentGroup] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.AssignmentGroup_dbo.Assignment_AssignmentId] FOREIGN KEY ([AssignmentId]) REFERENCES [dbo].[Assignment] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [dbo].[AssignmentSubmission] (
	[Id]	uniqueidentifier NOT NULL,	
	[AssignmentId]	uniqueidentifier  NULL,
	[AssignmentGroupId]	uniqueidentifier  NULL,
	[StudentId]	NVARCHAR (128)  NULL,
	[TeacherId]	NVARCHAR (128)  NULL,
	[Grade]	int  NULL,		
	[SubmissionDate]	datetime  NULL,
	[LastUpdate]	datetime  NULL,
	[SubmissionFile]	NVARCHAR (max)  NULL,
	[Comment]	NVARCHAR (max)  NULL
    CONSTRAINT [PK_dbo.AssignmentSubmission] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.AssignmentSubmission_dbo.Assignment_AssignmentId] FOREIGN KEY ([AssignmentId]) REFERENCES [dbo].[Assignment] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_dbo.AssignmentSubmission_dbo.AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [dbo].[StudentAssignmentGroup] (
	[StudentId]	NVARCHAR (128) NOT  NULL,
	[GroupId]	uniqueidentifier NOT NULL
	CONSTRAINT [PK_dbo.StudentAssignmentGroup] PRIMARY KEY CLUSTERED ([StudentId] ASC, [GroupId] ASC),
    CONSTRAINT [FK_dbo.StudentAssignmentGroup_dbo.AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo.StudentAssignmentGroup_dbo.AssignmentGroup_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [dbo].[AssignmentGroup] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [dbo].[Quiz] (
	[Id]	uniqueidentifier NOT NULL,	
	[CourseChannelId]	uniqueidentifier  NULL,
	[QuizTitle]	NVARCHAR(max)  NULL,
	[Instruction]	NVARCHAR (max) NULL,
	[RandomizeQuestion]	bit  NULL,
	[DueDateTime]	datetime  NULL,
	[CreatorId]	NVARCHAR (128)  NULL,
	[LastUpdate]	datetime  NULL,
	[IsEnabled]	bit  NULL
    CONSTRAINT [PK_dbo.Quiz] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.Quiz_dbo.CourseChannel_CourseChannelId] FOREIGN KEY ([CourseChannelId]) REFERENCES [dbo].[CourseChannel] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [dbo].[QuizQuestion] (
	[Id]	uniqueidentifier NOT NULL,	
	[QuizId]	uniqueidentifier  NULL,
	[Question]	NVARCHAR(max)  NULL,
	[AttachedImage]	NVARCHAR (max) NULL,
	[LastUpdate]	datetime  NULL
    CONSTRAINT [PK_dbo.QuizQuestion] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.QuizQuestion_dbo.Quiz_QuizId] FOREIGN KEY ([QuizId]) REFERENCES [dbo].[Quiz] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [dbo].[QuizAnswer] (
	[Id]	uniqueidentifier NOT NULL,	
	[QuestionId]	uniqueidentifier  NULL,
	[AnswerText]	NVARCHAR(max)  NULL,
	[IsCorrect]	bit  NULL,
	[LastUpdate]	datetime  NULL
    CONSTRAINT [PK_dbo.QuizAnswer] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.QuizAnswer_dbo.QuizQuestion_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [dbo].[QuizQuestion] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [dbo].[StudentQuizAnswer] (
	[Id]	uniqueidentifier NOT NULL,	
	[StudentId]	NVARCHAR (128)  NULL,
	[QuestionId]	uniqueidentifier  NULL,
	[AnswerId]	uniqueidentifier  NULL,
	[QuizId]	uniqueidentifier  NULL,
	[LastUpdate]	datetime  NULL
    CONSTRAINT [PK_dbo.StudentQuizAnswer] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.StudentQuizAnswer_dbo.AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_dbo.StudentQuizAnswer_dbo.QuizQuestion_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [dbo].[QuizQuestion] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [dbo].[StudentQuiz] (
	[Id]	uniqueidentifier NOT NULL,	
	[StudentId]	NVARCHAR (128)  NULL,
	[QuizId]	uniqueidentifier  NULL,	
	[QuizStartedOn]	datetime  NULL
    CONSTRAINT [PK_dbo.StudentQuiz] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.StudentQuiz_dbo.AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_dbo.StudentQuiz_dbo.Quiz_QuizId] FOREIGN KEY ([QuizId]) REFERENCES [dbo].[Quiz] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [dbo].[Thread] (
	[Id]	uniqueidentifier NOT NULL,	
	[CourseChannelId]	uniqueidentifier NULL,	
	[UserId]	NVARCHAR (128)  NULL,
	[Title]	NVARCHAR (max)  NULL,
	[Content]	NVARCHAR (max)  NULL,
	[LastUpdate]	datetime  NULL
    CONSTRAINT [PK_dbo.Thread] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.Thread_dbo.AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_dbo.Thread_dbo.CourseChannel_CourseChannelId] FOREIGN KEY ([CourseChannelId]) REFERENCES [dbo].[CourseChannel] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [dbo].[Post] (
	[Id]	uniqueidentifier NOT NULL,	
	[ThreadId]	uniqueidentifier NULL,	
	[UserId]	NVARCHAR (128)  NULL,
	[Title]	NVARCHAR (max)  NULL,
	[Content]	NVARCHAR (max)  NULL,
	[LastUpdate]	datetime  NULL,
	[PostId]	uniqueidentifier NULL	
    CONSTRAINT [PK_dbo.Post] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.Post_dbo.Thread_ThreadId] FOREIGN KEY ([ThreadId]) REFERENCES [dbo].[Thread] ([Id]) ON DELETE CASCADE
);
CREATE TABLE [dbo].[ChatChannel] (
	[Id]	uniqueidentifier NOT NULL,	
	[ChatUserOne]	NVARCHAR (128)  NULL,
	[ChatUserTwo]	NVARCHAR (128)  NULL,
	[UserOneClearedHistory] bit NOT NULL default(0), --added on 8/13 to solve clear chat history and delete chat bug
	[UserTwoClearedHistory] bit NOT NULL default(0), --added on 8/13 to solve clear chat history and delete chat bug
	[UserOneDeletedChat] bit NOT NULL default(0), --added on 8/13 to solve clear chat history and delete chat bug
	[UserTwoDeletedChat] bit NOT NULL default(0), --added on 8/13 to solve clear chat history and delete chat bug
	[UserOneClearedHistoryTime] datetime  NULL, --added on 8/13 to solve clear chat history and delete chat bug
	[UserTwoClearedHistoryTime] datetime  NULL, --added on 8/13 to solve clear chat history and delete chat bug
	[UserOneDeletedChatTime] datetime  NULL, --added on 8/13 to solve clear chat history and delete chat bug
	[UserTwoDeletedChatTime] datetime  NULL --added on 8/13 to solve clear chat history and delete chat bug
    CONSTRAINT [PK_dbo.ChatChannel] PRIMARY KEY CLUSTERED ([Id] ASC)
);
CREATE TABLE [dbo].[Chat] (
	[Id]	uniqueidentifier NOT NULL,	
	[ChannelId]	uniqueidentifier  NULL,
	[SenderId]	NVARCHAR (128)  NULL,
	[Message]	NVARCHAR (max)  NULL,
	[SentTime]	datetime  NULL
    CONSTRAINT [PK_dbo.Chat] PRIMARY KEY CLUSTERED ([Id] ASC),
	CONSTRAINT [FK_dbo.Chat_dbo.AspNetUsers_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
	CONSTRAINT [FK_dbo.Chat_dbo.ChatChannel_ChannelId] FOREIGN KEY ([ChannelId]) REFERENCES [dbo].[ChatChannel] ([Id]) ON DELETE CASCADE
);
END


BEGIN
INSERT INTO ThemeCategory (CateogyId, CategoryName, Code) values ('A1506F8A-DA59-4A07-8F65-887F498199B2','Computer Science & Information Technology','CS')
INSERT INTO ThemeCategory (CateogyId, CategoryName, Code) values ('C44AC010-C25D-493F-980D-531A6758921F','Business & Mathematics','Bus')
INSERT INTO ThemeCategory (CateogyId, CategoryName, Code) values ('43015E78-7803-4B92-8E92-CF02FD3CA1C9','Arts','Arts')
INSERT INTO ThemeCategory (CateogyId, CategoryName, Code) values ('ED3334B1-8CDD-4810-B125-B2598CA3B917','Sports','Sports')
INSERT INTO ThemeCategory (CateogyId, CategoryName, Code) values ('2B61FF48-53F3-4BC7-B2DE-5EC7781CD2C0','Other','Other')
INSERT INTO ThemeCategory (CateogyId, CategoryName, Code) values ('258CCB56-8B7A-4ACA-9767-5DD9DF37DEC5','Default','Default')
END

BEGIN
-- -------------------- Default
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'258CCB56-8B7A-4ACA-9767-5DD9DF37DEC5',
'/CourseChannelTheme/Default/1800-default-min.png', '1800-default-min.png')
-- -------------------- Art
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'43015E78-7803-4B92-8E92-CF02FD3CA1C9',
'/CourseChannelTheme/Art/1-min.png', '1-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'43015E78-7803-4B92-8E92-CF02FD3CA1C9',
'/CourseChannelTheme/Art/2-min.png', '2-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'43015E78-7803-4B92-8E92-CF02FD3CA1C9',
'/CourseChannelTheme/Art/3-min.png', '3-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'43015E78-7803-4B92-8E92-CF02FD3CA1C9',
'/CourseChannelTheme/Art/4-min.png', '4-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'43015E78-7803-4B92-8E92-CF02FD3CA1C9',
'/CourseChannelTheme/Art/5-min.png', '5-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'43015E78-7803-4B92-8E92-CF02FD3CA1C9',
'/CourseChannelTheme/Art/6-min.png', '6-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'43015E78-7803-4B92-8E92-CF02FD3CA1C9',
'/CourseChannelTheme/Art/7-min.png', '7-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'43015E78-7803-4B92-8E92-CF02FD3CA1C9',
'/CourseChannelTheme/Art/8-min.png', '8-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'43015E78-7803-4B92-8E92-CF02FD3CA1C9',
'/CourseChannelTheme/Art/9-min.png', '9-min.png')
-- -------------------- Computer Science
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'A1506F8A-DA59-4A07-8F65-887F498199B2',
'/CourseChannelTheme/CS/1-min.png', '1-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'A1506F8A-DA59-4A07-8F65-887F498199B2',
'/CourseChannelTheme/CS/2-min.png', '2-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'A1506F8A-DA59-4A07-8F65-887F498199B2',
'/CourseChannelTheme/CS/3-min.png', '3-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'A1506F8A-DA59-4A07-8F65-887F498199B2',
'/CourseChannelTheme/CS/4-min.png', '4-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'A1506F8A-DA59-4A07-8F65-887F498199B2',
'/CourseChannelTheme/CS/5-min.png', '5-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'A1506F8A-DA59-4A07-8F65-887F498199B2',
'/CourseChannelTheme/CS/6-min.png', '6-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'A1506F8A-DA59-4A07-8F65-887F498199B2',
'/CourseChannelTheme/CS/7-min.png', '7-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'A1506F8A-DA59-4A07-8F65-887F498199B2',
'/CourseChannelTheme/CS/8-min.png', '8-min.png')
-- -------------------- Sports
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'ED3334B1-8CDD-4810-B125-B2598CA3B917',
'/CourseChannelTheme/Sport/1-min.png', '1-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'ED3334B1-8CDD-4810-B125-B2598CA3B917',
'/CourseChannelTheme/Sport/2-min.png', '2-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'ED3334B1-8CDD-4810-B125-B2598CA3B917',
'/CourseChannelTheme/Sport/3-min.png', '3-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'ED3334B1-8CDD-4810-B125-B2598CA3B917',
'/CourseChannelTheme/Sport/4-min.png', '4-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'ED3334B1-8CDD-4810-B125-B2598CA3B917',
'/CourseChannelTheme/Sport/5-min.png', '5-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'ED3334B1-8CDD-4810-B125-B2598CA3B917',
'/CourseChannelTheme/Sport/6-min.png', '6-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'ED3334B1-8CDD-4810-B125-B2598CA3B917',
'/CourseChannelTheme/Sport/7-min.png', '7-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'ED3334B1-8CDD-4810-B125-B2598CA3B917',
'/CourseChannelTheme/Sport/8-min.png', '8-min.png')
-- -------------------- Business
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'C44AC010-C25D-493F-980D-531A6758921F',
'/CourseChannelTheme/Business/1-min.png', '1-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'C44AC010-C25D-493F-980D-531A6758921F',
'/CourseChannelTheme/Business/2-min.png', '2-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'C44AC010-C25D-493F-980D-531A6758921F',
'/CourseChannelTheme/Business/3-min.png', '3-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'C44AC010-C25D-493F-980D-531A6758921F',
'/CourseChannelTheme/Business/4-min.png', '4-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'C44AC010-C25D-493F-980D-531A6758921F',
'/CourseChannelTheme/Business/5-min.png', '5-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'C44AC010-C25D-493F-980D-531A6758921F',
'/CourseChannelTheme/Business/6-min.png', '6-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'C44AC010-C25D-493F-980D-531A6758921F',
'/CourseChannelTheme/Business/7-min.png', '7-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'C44AC010-C25D-493F-980D-531A6758921F',
'/CourseChannelTheme/Business/8-min.png', '8-min.png')
-- -------------------- Other
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'2B61FF48-53F3-4BC7-B2DE-5EC7781CD2C0',
'/CourseChannelTheme/Other/1-min.png', '1-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'2B61FF48-53F3-4BC7-B2DE-5EC7781CD2C0',
'/CourseChannelTheme/Other/2-min.png', '2-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'2B61FF48-53F3-4BC7-B2DE-5EC7781CD2C0',
'/CourseChannelTheme/Other/3-min.png', '3-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'2B61FF48-53F3-4BC7-B2DE-5EC7781CD2C0',
'/CourseChannelTheme/Other/4-min.png', '4-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'2B61FF48-53F3-4BC7-B2DE-5EC7781CD2C0',
'/CourseChannelTheme/Other/5-min.png', '5-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'2B61FF48-53F3-4BC7-B2DE-5EC7781CD2C0',
'/CourseChannelTheme/Other/6-min.png', '6-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'2B61FF48-53F3-4BC7-B2DE-5EC7781CD2C0',
'/CourseChannelTheme/Other/7-min.png', '7-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'2B61FF48-53F3-4BC7-B2DE-5EC7781CD2C0',
'/CourseChannelTheme/Other/8-min.png', '8-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'2B61FF48-53F3-4BC7-B2DE-5EC7781CD2C0',
'/CourseChannelTheme/Other/9-min.png', '9-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'2B61FF48-53F3-4BC7-B2DE-5EC7781CD2C0',
'/CourseChannelTheme/Other/10-min.png', '10-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'2B61FF48-53F3-4BC7-B2DE-5EC7781CD2C0',
'/CourseChannelTheme/Other/11-min.png', '11-min.png')
INSERT INTO Theme (Id, CategoryId, ImageSource, ImageName) values
(NEWID(),'2B61FF48-53F3-4BC7-B2DE-5EC7781CD2C0',
'/CourseChannelTheme/Other/12-min.png', '12-min.png')
END
