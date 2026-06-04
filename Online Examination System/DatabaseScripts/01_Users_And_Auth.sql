-- =======================================================
-- Online Examination System - Phase 1: Authentication
-- =======================================================

-- 1. Create Roles Table (Fixed Roles: Admin, Student)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        RoleId INT PRIMARY KEY IDENTITY(1,1),
        RoleName NVARCHAR(50) NOT NULL UNIQUE
    );
    
    -- Insert Default Roles
    INSERT INTO Roles (RoleName) VALUES ('Admin'), ('Student');
END
GO

-- 2. Create Users Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserId INT PRIMARY KEY IDENTITY(1,1),
        RoleId INT NOT NULL FOREIGN KEY REFERENCES Roles(RoleId),
        Name NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        Contact NVARCHAR(20) NULL,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        Address NVARCHAR(MAX) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME NULL,
        CreatedBy INT NULL,
        UpdatedBy INT NULL
    );
END
GO

-- =======================================================
-- STORED PROCEDURES
-- =======================================================

-- 1. Procedure to Register a new Student
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_StudentRegister')
    DROP PROCEDURE sp_StudentRegister;
GO

CREATE PROCEDURE sp_StudentRegister
    @Name NVARCHAR(100),
    @Email NVARCHAR(100),
    @Contact NVARCHAR(20),
    @Username NVARCHAR(50),
    @PasswordHash NVARCHAR(255),
    @Address NVARCHAR(MAX),
    @UserId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @StudentRoleId INT;
    SELECT @StudentRoleId = RoleId FROM Roles WHERE RoleName = 'Student';

    -- Check if username or email already exists
    IF EXISTS(SELECT 1 FROM Users WHERE Username = @Username)
    BEGIN
        RAISERROR('Username already exists.', 16, 1);
        RETURN;
    END

    IF EXISTS(SELECT 1 FROM Users WHERE Email = @Email)
    BEGIN
        RAISERROR('Email already exists.', 16, 1);
        RETURN;
    END

    INSERT INTO Users (RoleId, Name, Email, Contact, Username, PasswordHash, Address, IsActive)
    VALUES (@StudentRoleId, @Name, @Email, @Contact, @Username, @PasswordHash, @Address, 1);

    SET @UserId = SCOPE_IDENTITY();
END
GO

-- 2. Procedure for User Login
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_UserLogin')
    DROP PROCEDURE sp_UserLogin;
GO

CREATE PROCEDURE sp_UserLogin
    @UsernameOrEmail NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.Name,
        u.Email,
        u.Username,
        u.PasswordHash,
        u.IsActive,
        r.RoleName
    FROM Users u
    INNER JOIN Roles r ON u.RoleId = r.RoleId
    WHERE (u.Username = @UsernameOrEmail OR u.Email = @UsernameOrEmail);
END
GO

-- 3. Procedure to create Admin (To be run manually once)
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CreateAdmin')
    DROP PROCEDURE sp_CreateAdmin;
GO

CREATE PROCEDURE sp_CreateAdmin
    @Name NVARCHAR(100),
    @Email NVARCHAR(100),
    @Contact NVARCHAR(20),
    @Username NVARCHAR(50),
    @PasswordHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @AdminRoleId INT;
    SELECT @AdminRoleId = RoleId FROM Roles WHERE RoleName = 'Admin';

    IF NOT EXISTS(SELECT 1 FROM Users WHERE Username = @Username)
    BEGIN
        INSERT INTO Users (RoleId, Name, Email, Contact, Username, PasswordHash, IsActive)
        VALUES (@AdminRoleId, @Name, @Email, @Contact, @Username, @PasswordHash, 1);
    END
END
GO
