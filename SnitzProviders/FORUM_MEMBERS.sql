USE [SnitzProviderTest]
GO
/****** Object:  Table [dbo].[FORUM_MEMBERS]    Script Date: 06/14/2007 18:44:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FORUM_MEMBERS](
	[MEMBER_ID] [int] IDENTITY(1,1) NOT NULL,
	[M_STATUS] [smallint] NULL,
	[M_NAME] [nvarchar](75) NULL,
	[M_USERNAME] [nvarchar](150) NULL,
	[M_FIRSTNAME] [nvarchar](100) NULL,
	[M_LASTNAME] [nvarchar](100) NULL,
	[M_PASSWORD] [nvarchar](65) NULL,
	[M_EMAIL] [nvarchar](50) NULL,
	[M_COUNTRY] [nvarchar](50) NULL,
	[M_HOMEPAGE] [nvarchar](255) NULL,
	[M_SIG] [ntext] NULL,
	[M_DEFAULT_VIEW] [int] NULL CONSTRAINT [DF_FORUM_MEMBERS_M_DEFAULT_VIEW]  DEFAULT ((1)),
	[M_LEVEL] [smallint] NULL CONSTRAINT [DF_FORUM_MEMBERS_M_LEVEL]  DEFAULT ((1)),
	[M_POSTS] [int] NULL,
	[M_DATE] [nvarchar](50) NULL,
	[M_LASTHEREDATE] [nvarchar](50) NULL,
	[M_LASTPOSTDATE] [nvarchar](50) NULL,
	[M_TITLE] [nvarchar](50) NULL,
	[M_SUBSCRIPTION] [smallint] NULL CONSTRAINT [DF_FORUM_MEMBERS_M_SUBSCRIPTION]  DEFAULT ((0)),
	[M_HIDE_EMAIL] [smallint] NULL CONSTRAINT [DF_FORUM_MEMBERS_M_HIDE_EMAIL]  DEFAULT ((0)),
	[M_RECEIVE_EMAIL] [smallint] NULL,
	[M_LAST_IP] [nvarchar](50) NULL,
	[M_IP] [nvarchar](50) NULL,
	[M_OCCUPATION] [nvarchar](255) NULL,
	[M_SEX] [nvarchar](50) NULL,
	[M_AGE] [nvarchar](10) NULL,
	[M_HOBBIES] [ntext] NULL,
	[M_LNEWS] [ntext] NULL,
	[M_QUOTE] [ntext] NULL,
	[M_AIM] [nvarchar](150) NULL,
	[M_YAHOO] [nvarchar](150) NULL,
	[M_ICQ] [nvarchar](150) NULL,
	[M_BIO] [ntext] NULL,
	[M_MARSTATUS] [nvarchar](100) NULL,
	[M_LINK1] [nvarchar](255) NULL,
	[M_LINK2] [nvarchar](255) NULL,
	[M_CITY] [nvarchar](100) NULL,
	[M_STATE] [nvarchar](100) NULL,
	[M_PHOTO_URL] [nvarchar](255) NULL,
	[rowguid] [uniqueidentifier] ROWGUIDCOL  NOT NULL CONSTRAINT [DF_FORUM_MEMBERS_rowguid]  DEFAULT (newid()),
	[M_MSN] [nvarchar](150) NULL,
	[M_KEY] [nvarchar](32) NULL,
	[M_NEWEMAIL] [nvarchar](50) NULL,
	[M_SHA256] [smallint] NULL,
	[M_PWKEY] [nvarchar](32) NULL,
	[M_VIEW_SIG] [smallint] NULL,
	[M_DOB] [nvarchar](8) NULL,
	[M_SIG_DEFAULT] [smallint] NULL,
	[M_RECTID] [int] NULL,
 CONSTRAINT [PK_FORUM_MEMBERS] PRIMARY KEY CLUSTERED 
(
	[MEMBER_ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO


INSERT INTO [dbo].[FORUM_MEMBERS] ([M_STATUS], [M_NAME], [M_USERNAME], [M_FIRSTNAME], [M_LASTNAME], [M_PASSWORD], [M_EMAIL], [M_COUNTRY], [M_HOMEPAGE], [M_SIG], [M_DEFAULT_VIEW], [M_LEVEL], [M_POSTS], [M_DATE], [M_LASTHEREDATE], [M_LASTPOSTDATE], [M_TITLE], [M_SUBSCRIPTION], [M_HIDE_EMAIL], [M_RECEIVE_EMAIL], [M_LAST_IP], [M_IP], [M_OCCUPATION], [M_SEX], [M_AGE], [M_HOBBIES], [M_LNEWS], [M_QUOTE], [M_AIM], [M_YAHOO], [M_ICQ], [M_BIO], [M_MARSTATUS], [M_LINK1], [M_LINK2], [M_CITY], [M_STATE], [M_PHOTO_URL], [rowguid], [M_MSN], [M_KEY], [M_NEWEMAIL], [M_SHA256], [M_PWKEY], [M_VIEW_SIG], [M_DOB], [M_SIG_DEFAULT], [M_RECTID]) VALUES (1, N'Admin', N'Admin', N' ', N' ', N'9192338454288eea0198dbb1dc2bc161e2705a7569b816a24320eabea5d8ad82', N'forums@example.com', N' ', N'http://www.example.com', N'-- Admin --', 1, 3, 6, N'20001119000000', N'20051122231648', N'20010321142428', N'Forum Admin', 0, 0, 1, N'211.30.100.25', N'000.000.000.000', N' ', N' ', N'', N' ', N' ', N' ', N' ', N' ', N' ', N' ', N'', N'http://', N'http://', N' ', N' ', N'', N'c5ae06c1-128f-11d5-a733-00a0c96fc5f4', NULL, NULL, NULL, 1, N' ', 1, NULL, 1, NULL)
INSERT INTO [dbo].[FORUM_MEMBERS] ([M_STATUS], [M_NAME], [M_USERNAME], [M_FIRSTNAME], [M_LASTNAME], [M_PASSWORD], [M_EMAIL], [M_COUNTRY], [M_HOMEPAGE], [M_SIG], [M_DEFAULT_VIEW], [M_LEVEL], [M_POSTS], [M_DATE], [M_LASTHEREDATE], [M_LASTPOSTDATE], [M_TITLE], [M_SUBSCRIPTION], [M_HIDE_EMAIL], [M_RECEIVE_EMAIL], [M_LAST_IP], [M_IP], [M_OCCUPATION], [M_SEX], [M_AGE], [M_HOBBIES], [M_LNEWS], [M_QUOTE], [M_AIM], [M_YAHOO], [M_ICQ], [M_BIO], [M_MARSTATUS], [M_LINK1], [M_LINK2], [M_CITY], [M_STATE], [M_PHOTO_URL], [rowguid], [M_MSN], [M_KEY], [M_NEWEMAIL], [M_SHA256], [M_PWKEY], [M_VIEW_SIG], [M_DOB], [M_SIG_DEFAULT], [M_RECTID]) VALUES (1, N'James', NULL, N'James', N'Curran', N'5fd9f6a0a5c92a99b98f58aa916b397a54c8b193d9733413bfb8f9a55be82347', N'James@example.com', N' ', N'http://www.example.com', N'', 1, 3, 529, N'20010225051815', N'20060629091152', N'20060606025832', N' ', 0, 0, 1, N'64.253.39.153', N'000.000.000.000', N'Webmaster', N'Male', N'38', N' ', N'', N' ', N' ', N' ', N' ', N' ', N'', N' ', N' ', N'Bloomfield', N'NJ', N'', N'c5ae06c2-128f-11d5-a733-00a0c96fc5f4', N' ', N' ', N'James@example.com', 1, N' ', 1, NULL, 1, 3)
INSERT INTO [dbo].[FORUM_MEMBERS] ([M_STATUS], [M_NAME], [M_USERNAME], [M_FIRSTNAME], [M_LASTNAME], [M_PASSWORD], [M_EMAIL], [M_COUNTRY], [M_HOMEPAGE], [M_SIG], [M_DEFAULT_VIEW], [M_LEVEL], [M_POSTS], [M_DATE], [M_LASTHEREDATE], [M_LASTPOSTDATE], [M_TITLE], [M_SUBSCRIPTION], [M_HIDE_EMAIL], [M_RECEIVE_EMAIL], [M_LAST_IP], [M_IP], [M_OCCUPATION], [M_SEX], [M_AGE], [M_HOBBIES], [M_LNEWS], [M_QUOTE], [M_AIM], [M_YAHOO], [M_ICQ], [M_BIO], [M_MARSTATUS], [M_LINK1], [M_LINK2], [M_CITY], [M_STATE], [M_PHOTO_URL], [rowguid], [M_MSN], [M_KEY], [M_NEWEMAIL], [M_SHA256], [M_PWKEY], [M_VIEW_SIG], [M_DOB], [M_SIG_DEFAULT], [M_RECTID]) VALUES (1, N'kaletm', NULL, N'Mike', N'Kalet', N'afd5c5ae5738e27549126ac0e07607015f279a2011abe7a1ccbb73317ecfdf5f', N'Kalet@example.net', N' ', N'http://example.com', N'', 1, 1, 132, N'20010227090358', N'20030404094117', N'20030226121345', NULL, 0, 0, 1, N'208.247.209.15', N'000.000.000.000', N'Technical Support Specialist', N'Male', N'31', N'', N' ', N' ', N' ', N' ', N' ', N'', N'http://', N'http://', N'NYC', N'NY', N'', N'c5ae06c6-128f-11d5-a733-00a0c96fc5f4', NULL, NULL, NULL, 1, NULL, 1, NULL, 1, NULL)