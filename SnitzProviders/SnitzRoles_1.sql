USE [SnitzProviderTest]
GO
/****** Object:  Table [dbo].[FORUM_ROLES]    Script Date: 06/12/2007 16:47:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FORUM_ROLES](
	[RoleID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nchar](25) NOT NULL,
	[Description] [nvarchar](250) NULL,
	[ModUser] [nvarchar](50) NOT NULL,
	[ModTime] [datetime] NOT NULL 
        CONSTRAINT [DF_FORUM_ROLES_ModTime]  DEFAULT (getdate()),
 CONSTRAINT [PK_FORUM_ROLES] PRIMARY KEY CLUSTERED 
(
	[RoleID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

SET XACT_ABORT, ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS, NOCOUNT ON
GO
BEGIN TRANSACTION

-- Add rows to [dbo].[FORUM_ROLES]
SET IDENTITY_INSERT [dbo].[FORUM_ROLES] ON
INSERT INTO [dbo].[FORUM_ROLES] ([RoleID], [Name], [Description], [ModUser]) VALUES (1, N'User                     ', N'Standard User', N'intrinsic')
INSERT INTO [dbo].[FORUM_ROLES] ([RoleID], [Name], [Description], [ModUser]) VALUES (2, N'Moderator                ', N'Forum Moderator', N'intrinsic')
INSERT INTO [dbo].[FORUM_ROLES] ([RoleID], [Name], [Description], [ModUser]) VALUES (3, N'Administrator            ', N'System Administrator', N'intrinsic')
SET IDENTITY_INSERT [dbo].[FORUM_ROLES] OFF

COMMIT TRANSACTION

/****** Object:  StoredProcedure [dbo].[snitz_Roles_RoleExists]    Script Date: 06/12/2007 17:04:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[snitz_Roles_RoleExists]
    @RoleName         nvarchar(256)
AS
BEGIN
	select count(*) 
	FROM dbo.FORUM_ROLES 
	WHERE LOWER(@RoleName) = LOWER(Name)
END


/****** Object:  StoredProcedure [dbo].[snitz_Roles_GetAllRoles]    Script Date: 06/12/2007 17:04:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[snitz_Roles_GetAllRoles]
AS
BEGIN
    SELECT Name from FORUM_ROLES ORDER BY Name
END

/****** Object:  StoredProcedure [dbo].[snitz_Roles_CreateRole]    Script Date: 06/12/2007 17:03:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[snitz_Roles_CreateRole]
    @RoleName         nvarchar(256),
	@ModUser		nvarchar(256)
AS
BEGIN
    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    IF (EXISTS(SELECT RoleId 
				FROM dbo.FORUM_ROLES 
				WHERE LOWER(Name) = LOWER(@RoleName)))
    BEGIN
        SET @ErrorCode = 1
        GOTO Cleanup
    END

    INSERT INTO dbo.FORUM_ROLES
                (Name, ModUser, ModTime)
         VALUES (@RoleName, @ModUser, GetDate())

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    select (0)

Cleanup:

    select @ErrorCode

END

END


CREATE PROCEDURE [dbo].[snitz_Roles_DeleteRole]
    @RoleName                   nvarchar(256),
    @DeleteOnlyIfRoleIsEmpty    bit
AS
BEGIN
    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

    IF( @@TRANCOUNT = 0 )
    BEGIN
        BEGIN TRANSACTION
        SET @TranStarted = 1
    END
    ELSE
        SET @TranStarted = 0

    DECLARE @RoleId   int
    SELECT  @RoleId = 0
    SELECT  @RoleId = RoleId FROM dbo.FORUM_ROLES 
		WHERE LOWER([Name]) = LOWER(@RoleName) 

    IF (@RoleId IS NULL)
    BEGIN
        SELECT @ErrorCode = 1
        GOTO Cleanup
    END
    IF (@DeleteOnlyIfRoleIsEmpty <> 0)
    BEGIN
        IF (EXISTS (SELECT RoleId FROM dbo.FORUM_USERSINROLES  
		            WHERE @RoleId = RoleId))
        BEGIN
            SELECT @ErrorCode = 2
            GOTO Cleanup
        END
    END


    DELETE FROM dbo.FORUM_USERSINROLES  WHERE @RoleId = RoleId

    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    DELETE FROM dbo.FORUM_ROLES WHERE @RoleId = RoleId  


    IF( @@ERROR <> 0 )
    BEGIN
        SET @ErrorCode = -1
        GOTO Cleanup
    END

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
        COMMIT TRANSACTION
    END

    select (0)

Cleanup:

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
        ROLLBACK TRANSACTION
    END

    select @ErrorCode
END

/****** Object:  Table [dbo].[FORUM_USERSINROLES]    Script Date: 06/12/2007 14:26:29 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FORUM_USERSINROLES](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[MEMBER_ID] [int] NOT NULL,
	[ROLEID] [int] NOT NULL,
	[ModUser] [nvarchar](50) NOT NULL,
	[ModTime] [datetime] NOT NULL CONSTRAINT [DF_FORUM_USERSINROLES_ModTime]  DEFAULT (getdate()),
 CONSTRAINT [PK_FORUM_USERSINROLES] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[FORUM_USERSINROLES]  WITH CHECK ADD  CONSTRAINT [FK_FORUM_USERSINROLES_FORUM_MEMBERS] FOREIGN KEY([MEMBER_ID])
REFERENCES [dbo].[FORUM_MEMBERS] ([MEMBER_ID])
GO
ALTER TABLE [dbo].[FORUM_USERSINROLES] CHECK CONSTRAINT [FK_FORUM_USERSINROLES_FORUM_MEMBERS]
GO
ALTER TABLE [dbo].[FORUM_USERSINROLES]  WITH CHECK ADD  CONSTRAINT [FK_FORUM_USERSINROLES_FORUM_ROLES] FOREIGN KEY([ROLEID])
REFERENCES [dbo].[FORUM_ROLES] ([RoleID])
GO
ALTER TABLE [dbo].[FORUM_USERSINROLES] CHECK CONSTRAINT [FK_FORUM_USERSINROLES_FORUM_ROLES]
GO
ALTER TABLE [dbo].[FORUM_USERSINROLES]  WITH CHECK ADD  CONSTRAINT [CK_FORUM_USERSINROLES] CHECK  (([ROLEID]>(3)))
GO
ALTER TABLE [dbo].[FORUM_USERSINROLES] CHECK CONSTRAINT [CK_FORUM_USERSINROLES]



/****** Object:  StoredProcedure [dbo].[snitz_UsersInRoles_AddUserToRole]    Script Date: 06/12/2007 17:09:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[snitz_UsersInRoles_AddUserToRole]
    @UserName         nvarchar(256),
    @RoleName         nvarchar(256),
	@ModUser		nvarchar(256)
AS
BEGIN
    DECLARE @UserId int
    SELECT  @UserId = NULL
    DECLARE @RoleId varchar(40)
    SELECT  @RoleId = NULL
	DECLARE @RetVal int

    SELECT  @UserId = MEMBER_ID
    FROM    FORUM_MEMBERS
    WHERE   LOWER(M_NAME) = LOWER(@UserName) 

 IF (@UserId IS NULL)
        Set @RetVal = 2
 Else
 BEGIN
    SELECT  @RoleId = RoleId
    FROM    FORUM_ROLES
    WHERE   LOWER(Name) = LOWER(@RoleName) 

    IF (@RoleId IS NULL)
        Set @RetVal = 3
    ELSE
		
      IF (EXISTS( SELECT * FROM FORUM_USERSINROLES 
                 WHERE  Member_ID = @UserId AND RoleID = @RoleId))
         SET @RetVal = (4)
      ELSE
		INSERT INTO FORUM_USERSINROLES(Member_ID, RoleID, ModUser) Values(@Userid, @RoleID, @ModUser) 
		set @RetVal = @@Rowcount
  enD
  select @RetVal
END

/****** Object:  StoredProcedure [dbo].[snitz_UsersInRoles_FindUsersInRole]    Script Date: 06/12/2007 17:09:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[snitz_UsersInRoles_FindUsersInRole]
    @RoleName         nvarchar(256),
    @UserNameToMatch  nvarchar(256)
AS
BEGIN
     DECLARE @RoleId int 
     SELECT  @RoleId = NULL

     SELECT  @RoleId = RoleId
     FROM    FORUM_ROLES
     WHERE   LOWER(@RoleName) = LOWER(name)

     IF (@RoleId IS NULL)
         RETURN(1)

	if (@RoleID > 3)
		SELECT u.M_NAME as UserName
		FROM   FORUM_MEMBERS u 
			inner join FORUM_USERSINROLES ur on u.MEMBER_ID = ur.MEMBER_ID
		WHERE  ur.RoleID = @RoleId
		AND    LOWER(M_NAME) LIKE LOWER(@UserNameToMatch)
		ORDER BY u.M_NAME
	ELSE
		SELECT M_NAME as UserName
		FROM   FORUM_MEMBERS
		WHERE  M_LEVEL >= @RoleId
		AND    LOWER(M_NAME) LIKE LOWER(@UserNameToMatch)
		ORDER BY M_NAME
    

    RETURN(0)
END

/****** Object:  StoredProcedure [dbo].[snitz_UsersInRoles_IsUserInRole]    Script Date: 06/12/2007 17:10:09 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[snitz_UsersInRoles_IsUserInRole]
    @UserName         nvarchar(256),
    @RoleName         nvarchar(256)
AS
BEGIN
    DECLARE @UserId int
    SELECT  @UserId = NULL
    DECLARE @RoleId varchar(40)
    SELECT  @RoleId = NULL
	DECLARE @RetVal int

    SELECT  @UserId = MEMBER_ID
    FROM    FORUM_MEMBERS
    WHERE   LOWER(M_NAME) = LOWER(@UserName) 

 IF (@UserId IS NULL)
		Select 2
 Else
 BEGIN
    SELECT  @RoleId = RoleId
    FROM    FORUM_ROLES
    WHERE   LOWER(Name) = LOWER(@RoleName) 

    IF (@RoleId IS NULL)
		Select 3
    ELSE
    	SELECT count(*) FROM FORUM_USERSINROLES 
        WHERE  Member_ID = @UserId AND RoleID = @RoleId
  End
END

/****** Object:  StoredProcedure [dbo].[snitz_UsersInRoles_GetUsersInRoles]    Script Date: 06/12/2007 17:10:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[snitz_UsersInRoles_GetUsersInRoles]
    @RoleName         nvarchar(256)
AS
BEGIN

DECLARE @RoleID int
     SELECT  @RoleId = NULL
	Select @RoleId = Roleid from FORUM_ROLES where name = @RoleName
     IF (@RoleId IS NULL)
         RETURN(1)

	select fm.M_NAME 
	from FORUM_MEMBERS AS fm
	WHERE FM.M_LEVEL >= @Roleid
	union 
	select fm.M_NAME 
	from FORUM_MEMBERS AS fm inner join  FORUM_USERSINROLES ur on ur.member_id = fm.member_id
	where ur.roleid = @Roleid
	order by fm.M_Name

    RETURN(0)
END
/****** Object:  StoredProcedure [dbo].[snitz_UsersInRoles_RemoveUserFromRole]    Script Date: 06/12/2007 17:10:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[snitz_UsersInRoles_RemoveUserFromRole]
	@UserName		  nvarchar(256),
	@RoleName		  nvarchar(256)
AS
BEGIN
    DECLARE @UserId int
    SELECT  @UserId = NULL

	DECLARE @RetVal int

    SELECT  @UserId = MEMBER_ID
    FROM    FORUM_MEMBERS
    WHERE   LOWER(M_NAME) = LOWER(@UserName) 

 IF (@UserId IS NULL)
        Set @RetVal = 2
 Else
 BEGIN
    DECLARE @RoleId varchar(40)
    SELECT  @RoleId = NULL

    SELECT  @RoleId = RoleId
    FROM    FORUM_ROLES
    WHERE   LOWER(Name) = LOWER(@RoleName) 

    IF (@RoleId IS NULL)
        Set @RetVal = 3
    ELSE
		DELETE FROM FORUM_USERSINROLES 
                 WHERE  Member_ID = @UserId AND RoleID = @RoleId
         SET @RetVal = @@ROWCOUNT
  enD
  select @RetVal
END
