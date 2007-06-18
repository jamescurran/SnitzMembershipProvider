SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
IF EXISTS (SELECT * FROM tempdb..sysobjects WHERE id=OBJECT_ID('tempdb..#tmpErrors')) DROP TABLE #tmpErrors
GO
CREATE TABLE #tmpErrors (Error int)
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO
BEGIN TRANSACTION
GO
PRINT N'Creating [dbo].[SnitzValidateUser]'
GO
/****** Object:  StoredProcedure [dbo].[SnitzValidateUser]    Script Date: 03/07/2006 22:18:17 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SnitzValidateUser]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SnitzValidateUser]
GO

create procedure SnitzValidateUser
	(
		@pUsername nvarchar(150),
		@pHashedPassword  nvarchar(65)
	)
AS
BEGIN
   Select count(*) from FORUM_MEMBERS 
   where M_NAME = @pUsername
   and   M_PASSWORD = @pHashedPassword
   AND   M_STATUS = 1
END

GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO

/****** Object:  StoredProcedure [dbo].[SnitzChangePassword]    Script Date: 03/07/2006 22:18:17 ******/
PRINT N'Creating [dbo].[SnitzChangePassword]'

GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SnitzChangePassword]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SnitzChangePassword]
GO

create procedure [dbo].[SnitzChangePassword]
	(
		@pUsername nvarchar(150),
		@pHashedOldPassword  nvarchar(65),
		@pHashedNewPassword  nvarchar(65)
	)
AS
BEGIN
	IF exists (select (0) from FORUM_MEMBERS where M_NAME = @pUsername and M_PASSWORD = @pHashedOldPassword)
	BEGIN
		UPDATE 	FORUM_MEMBERS 
		SET   M_PASSWORD = @pHashedNewPassword 
		WHERE M_NAME     = @pUsername
		AND   M_PASSWORD = @pHashedOldPassword
	END
	SELECT @@ROWCOUNT
END

GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[SnitzDeleteUser]'
GO

create procedure [dbo].[SnitzDeleteUser]
	(
		@pUsername nvarchar(150)
	)
AS
BEGIN
	DELETE From FORUM_MEMBERS where M_NAME = @pUsername
	SELECT @@ROWCOUNT
END

GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[SnitzGetUserByName]'
GO

create procedure [dbo].[SnitzGetUserByName]
	(
		@pName nvarchar(150),
		@pUserIsOnline bit
	)
AS
BEGIN
	SELECT M_NAME as [Name], MEMBER_ID, M_EMAIL as Email, M_DATE as CreateDate, M_LASTHEREDATE as LastLoginDate
	FROM FORUM_MEMBERS where M_NAME = @pName
	IF @@ROWCOUNT > 0 and @pUserIsOnline = 1
	BEGIN
		UPDATE FORUM_MEMBERS 
		SET M_LASTHEREDATE = replace(replace(replace(convert(varchar(23), getdate(), 20),'-', ''),':', ''), ' ', '')
		WHERE M_NAME = @pName
	END
END

GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[SnitzGetUserById]'
GO

CREATE procedure [dbo].[SnitzGetUserById]
	(
		@pId int,
		@pUserIsOnline bit
	)
AS
BEGIN
	SELECT M_NAME as [Name], MEMBER_ID, M_EMAIL as Email, M_DATE as CreateDate, M_LASTHEREDATE as LastLoginDate
	FROM FORUM_MEMBERS where MEMBER_ID = @pId AND   M_STATUS = 1

	IF @@ROWCOUNT > 0 and @pUserIsOnline = 1
	BEGIN
		UPDATE FORUM_MEMBERS 
		SET M_LASTHEREDATE = replace(replace(replace(convert(varchar(23), getdate(), 20),'-', ''),':', ''), ' ', '')
		WHERE MEMBER_ID = @pId
	END
END

GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO

PRINT N'Creating [dbo].[SnitzFindUsersByName]'
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SnitzFindUsersByName]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SnitzFindUsersByName]
GO


CREATE PROCEDURE [dbo].[SnitzFindUsersByName]
    @UserNameToMatch       NVARCHAR(150),
    @PageIndex             INT,
    @PageSize              INT
AS
BEGIN
    -- Set the page bounds
    DECLARE @PageLowerBound INT
    DECLARE @PageUpperBound INT
    DECLARE @TotalRecords   INT
    SET @PageLowerBound = @PageSize * @PageIndex
    SET @PageUpperBound = @PageSize - 1 + @PageLowerBound

    -- Create a temp table TO store the select results
    CREATE TABLE #PageIndexForUsers
    (
        IndexId int IDENTITY (0, 1) NOT NULL,
        MEMBER_ID int
    )

    -- Insert into our temp table
    INSERT INTO #PageIndexForUsers (MEMBER_ID)
        SELECT   MEMBER_ID
        FROM     FORUM_MEMBERS
        WHERE	 LOWER(M_NAME) LIKE LOWER(@UserNameToMatch)  AND   M_STATUS = 1

        ORDER BY M_NAME


    SELECT  m.M_NAME as [Name], 
			m.MEMBER_ID, 
			m.M_EMAIL as Email, 
			m.M_DATE as CreateDate, 
			m.M_LASTHEREDATE as LastLoginDate
    FROM   FORUM_MEMBERS m, #PageIndexForUsers p
    WHERE  m.MEMBER_ID = p.MEMBER_ID 
    AND    p.IndexId >= @PageLowerBound AND p.IndexId <= @PageUpperBound
    ORDER BY m.M_NAME

    SELECT  @TotalRecords = COUNT(*)
    FROM    #PageIndexForUsers
    RETURN @TotalRecords
END

PRINT N'Creating [dbo].[SnitzFindUsersByEmail]'
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SnitzFindUsersByEmail]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SnitzFindUsersByEmail]
GO


CREATE PROCEDURE [dbo].[SnitzFindUsersByEmail]
    @EmailToMatch       NVARCHAR(256),
    @PageIndex             INT,
    @PageSize              INT
AS
BEGIN
    -- Set the page bounds
    DECLARE @PageLowerBound INT
    DECLARE @PageUpperBound INT
    DECLARE @TotalRecords   INT
    SET @PageLowerBound = @PageSize * @PageIndex
    SET @PageUpperBound = @PageSize - 1 + @PageLowerBound

    -- Create a temp table TO store the select results
    CREATE TABLE #PageIndexForUsers
    (
        IndexId int IDENTITY (0, 1) NOT NULL,
        MEMBER_ID int
    )

    -- Insert into our temp table
    INSERT INTO #PageIndexForUsers (MEMBER_ID)
        SELECT   MEMBER_ID
        FROM     FORUM_MEMBERS
        WHERE	 LOWER(M_EMAIL) LIKE LOWER(@EmailToMatch)    AND   M_STATUS = 1

        ORDER BY M_NAME


    SELECT  m.M_NAME as [Name], 
			m.MEMBER_ID, 
			m.M_EMAIL as Email, 
			m.M_DATE as CreateDate, 
			m.M_LASTHEREDATE as LastLoginDate
    FROM   FORUM_MEMBERS m, #PageIndexForUsers p
    WHERE  m.MEMBER_ID = p.MEMBER_ID 
    AND    p.IndexId >= @PageLowerBound AND p.IndexId <= @PageUpperBound
    ORDER BY m.M_NAME

    SELECT  @TotalRecords = COUNT(*)
    FROM    #PageIndexForUsers
    RETURN @TotalRecords
END

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SnitzGetAllUsers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SnitzGetAllUsers]
GO

CREATE PROCEDURE [dbo].[SnitzGetAllUsers]
    @PageIndex             INT,
    @PageSize              INT
AS
BEGIN
    -- Set the page bounds
    DECLARE @PageLowerBound INT
    DECLARE @PageUpperBound INT
    DECLARE @TotalRecords   INT
    SET @PageLowerBound = @PageSize * @PageIndex
    SET @PageUpperBound = @PageSize - 1 + @PageLowerBound

    -- Create a temp table TO store the select results
    CREATE TABLE #PageIndexForUsers
    (
        IndexId int IDENTITY (0, 1) NOT NULL,
        MEMBER_ID int
    )

    -- Insert into our temp table
    INSERT INTO #PageIndexForUsers (MEMBER_ID)
        SELECT   MEMBER_ID
        FROM     FORUM_MEMBERS
		WHERE    M_STATUS = 1

        ORDER BY M_NAME


    SELECT  m.M_NAME as [Name], 
			m.MEMBER_ID, 
			m.M_EMAIL as Email, 
			m.M_DATE as CreateDate, 
			m.M_LASTHEREDATE as LastLoginDate
    FROM   FORUM_MEMBERS m, #PageIndexForUsers p
    WHERE  m.MEMBER_ID = p.MEMBER_ID 
    AND    p.IndexId >= @PageLowerBound AND p.IndexId <= @PageUpperBound
    ORDER BY m.M_NAME

    SELECT  @TotalRecords = COUNT(*)
    FROM    #PageIndexForUsers
    RETURN @TotalRecords
END


PRINT N'Creating [dbo].[SnitzGetNumberOfUsersOnline]'
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SnitzGetNumberOfUsersOnline]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SnitzGetNumberOfUsersOnline]
GO

CREATE PROCEDURE [dbo].[SnitzGetNumberOfUsersOnline]
		@pWindow nvarchar(150)
AS
BEGIN
    DECLARE @NumUsers   INT
--    SELECT  @NumUsers = count(*) 
    SELECT  count(*) 
	FROM    FORUM_MEMBERS 
	WHERE   M_LASTHEREDATE > @pWindow

--	RETURN @NumUsers
END
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[SnitzGetUserNameByEmail]'
GO

create procedure SnitzGetUserNameByEmail
	(
		@pEmail nvarchar(50)
	)
AS
BEGIN
	SELECT M_NAME From FORUM_MEMBERS where M_EMAIL = @pEmail
END

GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[SnitzFindUsersByEmail]'
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SnitzFindUsersByEmail]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SnitzFindUsersByEmail]

GO
CREATE PROCEDURE [dbo].[SnitzFindUsersByEmail]
    @EmailToMatch       NVARCHAR(256),
    @PageIndex             INT,
    @PageSize              INT
AS
BEGIN
    -- Set the page bounds
    DECLARE @PageLowerBound INT
    DECLARE @PageUpperBound INT
    DECLARE @TotalRecords   INT
    SET @PageLowerBound = @PageSize * @PageIndex
    SET @PageUpperBound = @PageSize - 1 + @PageLowerBound

    -- Create a temp table TO store the select results
    CREATE TABLE #PageIndexForUsers
    (
        IndexId int IDENTITY (0, 1) NOT NULL,
        MEMBER_ID int
    )

    -- Insert into our temp table
    INSERT INTO #PageIndexForUsers (MEMBER_ID)
        SELECT   MEMBER_ID
        FROM     FORUM_MEMBERS
        WHERE	 LOWER(M_EMAIL) LIKE LOWER(@EmailToMatch)    AND   M_STATUS = 1

        ORDER BY M_NAME


    SELECT  m.M_NAME as [Name], 
			m.MEMBER_ID, 
			m.M_EMAIL as Email, 
			m.M_DATE as CreateDate, 
			m.M_LASTHEREDATE as LastLoginDate
    FROM   FORUM_MEMBERS m, #PageIndexForUsers p
    WHERE  m.MEMBER_ID = p.MEMBER_ID 
    AND    p.IndexId >= @PageLowerBound AND p.IndexId <= @PageUpperBound
    ORDER BY m.M_NAME

    SELECT  @TotalRecords = COUNT(*)
    FROM    #PageIndexForUsers
    RETURN @TotalRecords
END

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
IF EXISTS (SELECT * FROM #tmpErrors) ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT>0 BEGIN
PRINT 'The database update succeeded'
COMMIT TRANSACTION
END
ELSE PRINT 'The database update failed'
GO
DROP TABLE #tmpErrors
GO
