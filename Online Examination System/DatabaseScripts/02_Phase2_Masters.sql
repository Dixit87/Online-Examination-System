-- =======================================================
-- Online Examination System - Phase 2: Admin Masters
-- =======================================================

-- 1. Create ExamTypes Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExamTypes')
BEGIN
    CREATE TABLE ExamTypes (
        ExamTypeId INT PRIMARY KEY IDENTITY(1,1),
        Title NVARCHAR(200) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME NULL,
        CreatedBy INT NULL,
        UpdatedBy INT NULL
    );
END
GO

-- =======================================================
-- EXAM TYPES PROCEDURES
-- =======================================================

-- Insert or Update Exam Type
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ExamTypeUpsert')
    DROP PROCEDURE sp_ExamTypeUpsert;
GO

CREATE PROCEDURE sp_ExamTypeUpsert
    @ExamTypeId INT,
    @Title NVARCHAR(200),
    @IsActive BIT,
    @UserId INT -- CreatedBy / UpdatedBy
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @ExamTypeId = 0
    BEGIN
        INSERT INTO ExamTypes (Title, IsActive, CreatedDate, CreatedBy)
        VALUES (@Title, @IsActive, GETDATE(), @UserId);
    END
    ELSE
    BEGIN
        UPDATE ExamTypes
        SET Title = @Title,
            IsActive = @IsActive,
            UpdatedDate = GETDATE(),
            UpdatedBy = @UserId
        WHERE ExamTypeId = @ExamTypeId;
    END
END
GO

-- Get All Exam Types
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ExamTypesGetAll')
    DROP PROCEDURE sp_ExamTypesGetAll;
GO

CREATE PROCEDURE sp_ExamTypesGetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ExamTypeId, Title, IsActive, CreatedDate
    FROM ExamTypes
    ORDER BY Title ASC;
END
GO

-- Get Exam Type By Id
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ExamTypeGetById')
    DROP PROCEDURE sp_ExamTypeGetById;
GO

CREATE PROCEDURE sp_ExamTypeGetById
    @ExamTypeId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ExamTypeId, Title, IsActive
    FROM ExamTypes
    WHERE ExamTypeId = @ExamTypeId;
END
GO

-- =======================================================
-- USER MANAGEMENT PROCEDURES
-- =======================================================

-- Get All Users (Admin view)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_UsersGetAll')
    DROP PROCEDURE sp_UsersGetAll;
GO

CREATE PROCEDURE sp_UsersGetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        u.UserId,
        u.Name,
        u.Email,
        u.Contact,
        u.Username,
        u.IsActive,
        r.RoleName
    FROM Users u
    INNER JOIN Roles r ON u.RoleId = r.RoleId
    ORDER BY u.UserId DESC;
END
GO

-- Get User By Id
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_UserGetById')
    DROP PROCEDURE sp_UserGetById;
GO

CREATE PROCEDURE sp_UserGetById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        UserId,
        RoleId,
        Name,
        Email,
        Contact,
        Username,
        Address,
        IsActive
    FROM Users
    WHERE UserId = @UserId;
END
GO

-- Update User (Admin Side)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_UserUpdate')
    DROP PROCEDURE sp_UserUpdate;
GO

CREATE PROCEDURE sp_UserUpdate
    @UserId INT,
    @Name NVARCHAR(100),
    @Email NVARCHAR(100),
    @Contact NVARCHAR(20),
    @Username NVARCHAR(50),
    @Address NVARCHAR(MAX),
    @IsActive BIT,
    @UpdatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if email or username belongs to someone else
    IF EXISTS(SELECT 1 FROM Users WHERE Username = @Username AND UserId <> @UserId)
    BEGIN
        RAISERROR('Username already exists.', 16, 1);
        RETURN;
    END

    IF EXISTS(SELECT 1 FROM Users WHERE Email = @Email AND UserId <> @UserId)
    BEGIN
        RAISERROR('Email already exists.', 16, 1);
        RETURN;
    END

    UPDATE Users
    SET Name = @Name,
        Email = @Email,
        Contact = @Contact,
        Username = @Username,
        Address = @Address,
        IsActive = @IsActive,
        UpdatedDate = GETDATE(),
        UpdatedBy = @UpdatedBy
    WHERE UserId = @UserId;
END
GO

-- Toggle User Status
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_UserToggleStatus')
    DROP PROCEDURE sp_UserToggleStatus;
GO

CREATE PROCEDURE sp_UserToggleStatus
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users
    SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
    WHERE UserId = @UserId;
END
GO
