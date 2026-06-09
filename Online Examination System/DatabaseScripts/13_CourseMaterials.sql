-- =======================================================
-- Online Examination System - Phase 5: Course Materials
-- Sessions, Chapters, Topics, TopicDetails
-- =======================================================

-- 1. Create Sessions Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Sessions')
BEGIN
    CREATE TABLE Sessions (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Status BIT NOT NULL DEFAULT 1,
        CreatedBy INT NULL,
        UpdatedBy INT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME NULL
    );
END
GO

-- 2. Create Chapters Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Chapters')
BEGIN
    CREATE TABLE Chapters (
        Id INT PRIMARY KEY IDENTITY(1,1),
        SessionId INT NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Status BIT NOT NULL DEFAULT 1,
        CreatedBy INT NULL,
        UpdatedBy INT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME NULL,
        CONSTRAINT FK_Chapters_Sessions FOREIGN KEY (SessionId) REFERENCES Sessions(Id)
    );
END
GO

-- 3. Create Topics Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Topics')
BEGIN
    CREATE TABLE Topics (
        Id INT PRIMARY KEY IDENTITY(1,1),
        SessionId INT NOT NULL,
        ChapterId INT NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        DurationMin INT NULL,
        TopicType NVARCHAR(50) NOT NULL, -- General, Accordion, Multi Slider
        TopicFileType NVARCHAR(50) NULL,
        TopicFilePath NVARCHAR(MAX) NULL,
        TopicPosition INT NOT NULL DEFAULT 1,
        Status BIT NOT NULL DEFAULT 1,
        CreatedBy INT NULL,
        UpdatedBy INT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME NULL,
        CONSTRAINT FK_Topics_Sessions FOREIGN KEY (SessionId) REFERENCES Sessions(Id),
        CONSTRAINT FK_Topics_Chapters FOREIGN KEY (ChapterId) REFERENCES Chapters(Id)
    );
END
GO

-- 4. Create TopicDetails Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TopicDetails')
BEGIN
    CREATE TABLE TopicDetails (
        Id INT PRIMARY KEY IDENTITY(1,1),
        SessionId INT NOT NULL,
        ChapterId INT NOT NULL,
        TopicId INT NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        DurationMin INT NULL,
        FileType NVARCHAR(50) NULL,
        FilePath NVARCHAR(MAX) NULL,
        Position INT NOT NULL DEFAULT 1,
        Status BIT NOT NULL DEFAULT 1,
        CreatedBy INT NULL,
        UpdatedBy INT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
        UpdatedDate DATETIME NULL,
        CONSTRAINT FK_TopicDetails_Sessions FOREIGN KEY (SessionId) REFERENCES Sessions(Id),
        CONSTRAINT FK_TopicDetails_Chapters FOREIGN KEY (ChapterId) REFERENCES Chapters(Id),
        CONSTRAINT FK_TopicDetails_Topics FOREIGN KEY (TopicId) REFERENCES Topics(Id)
    );
END
GO

-- =======================================================
-- SESSION PROCEDURES
-- =======================================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_SessionUpsert')
    DROP PROCEDURE sp_SessionUpsert;
GO
CREATE PROCEDURE sp_SessionUpsert
    @Id INT,
    @Title NVARCHAR(200),
    @Description NVARCHAR(MAX),
    @Status BIT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @Id = 0
    BEGIN
        INSERT INTO Sessions (Title, Description, Status, CreatedBy, CreatedDate)
        VALUES (@Title, @Description, @Status, @UserId, GETDATE());
    END
    ELSE
    BEGIN
        UPDATE Sessions
        SET Title = @Title,
            Description = @Description,
            Status = @Status,
            UpdatedBy = @UserId,
            UpdatedDate = GETDATE()
        WHERE Id = @Id;
    END
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_SessionsGetAll')
    DROP PROCEDURE sp_SessionsGetAll;
GO
CREATE PROCEDURE sp_SessionsGetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Sessions ORDER BY Id DESC;
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_SessionGetById')
    DROP PROCEDURE sp_SessionGetById;
GO
CREATE PROCEDURE sp_SessionGetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Sessions WHERE Id = @Id;
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_SessionToggleStatus')
    DROP PROCEDURE sp_SessionToggleStatus;
GO
CREATE PROCEDURE sp_SessionToggleStatus
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Sessions SET Status = CASE WHEN Status = 1 THEN 0 ELSE 1 END WHERE Id = @Id;
END
GO

-- =======================================================
-- CHAPTER PROCEDURES
-- =======================================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ChapterUpsert')
    DROP PROCEDURE sp_ChapterUpsert;
GO
CREATE PROCEDURE sp_ChapterUpsert
    @Id INT,
    @SessionId INT,
    @Title NVARCHAR(200),
    @Description NVARCHAR(MAX),
    @Status BIT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @Id = 0
    BEGIN
        INSERT INTO Chapters (SessionId, Title, Description, Status, CreatedBy, CreatedDate)
        VALUES (@SessionId, @Title, @Description, @Status, @UserId, GETDATE());
    END
    ELSE
    BEGIN
        UPDATE Chapters
        SET SessionId = @SessionId,
            Title = @Title,
            Description = @Description,
            Status = @Status,
            UpdatedBy = @UserId,
            UpdatedDate = GETDATE()
        WHERE Id = @Id;
    END
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ChaptersGetAll')
    DROP PROCEDURE sp_ChaptersGetAll;
GO
CREATE PROCEDURE sp_ChaptersGetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.*, s.Title as SessionTitle 
    FROM Chapters c
    INNER JOIN Sessions s ON c.SessionId = s.Id
    ORDER BY c.Id DESC;
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ChapterGetById')
    DROP PROCEDURE sp_ChapterGetById;
GO
CREATE PROCEDURE sp_ChapterGetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Chapters WHERE Id = @Id;
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ChapterToggleStatus')
    DROP PROCEDURE sp_ChapterToggleStatus;
GO
CREATE PROCEDURE sp_ChapterToggleStatus
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Chapters SET Status = CASE WHEN Status = 1 THEN 0 ELSE 1 END WHERE Id = @Id;
END
GO

-- =======================================================
-- TOPIC PROCEDURES
-- =======================================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_TopicUpsert')
    DROP PROCEDURE sp_TopicUpsert;
GO
CREATE PROCEDURE sp_TopicUpsert
    @Id INT,
    @SessionId INT,
    @ChapterId INT,
    @Title NVARCHAR(200),
    @Description NVARCHAR(MAX),
    @DurationMin INT,
    @TopicType NVARCHAR(50),
    @TopicFileType NVARCHAR(50),
    @TopicFilePath NVARCHAR(MAX),
    @TopicPosition INT,
    @Status BIT,
    @UserId INT,
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @Id = 0
    BEGIN
        INSERT INTO Topics (SessionId, ChapterId, Title, Description, DurationMin, TopicType, TopicFileType, TopicFilePath, TopicPosition, Status, CreatedBy, CreatedDate)
        VALUES (@SessionId, @ChapterId, @Title, @Description, @DurationMin, @TopicType, @TopicFileType, @TopicFilePath, @TopicPosition, @Status, @UserId, GETDATE());
        
        SET @NewId = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE Topics
        SET SessionId = @SessionId,
            ChapterId = @ChapterId,
            Title = @Title,
            Description = @Description,
            DurationMin = @DurationMin,
            TopicType = @TopicType,
            TopicFileType = @TopicFileType,
            TopicFilePath = @TopicFilePath,
            TopicPosition = @TopicPosition,
            Status = @Status,
            UpdatedBy = @UserId,
            UpdatedDate = GETDATE()
        WHERE Id = @Id;
        
        SET @NewId = @Id;
    END
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_TopicsGetAll')
    DROP PROCEDURE sp_TopicsGetAll;
GO
CREATE PROCEDURE sp_TopicsGetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT t.*, c.Title as ChapterTitle, s.Title as SessionTitle 
    FROM Topics t
    INNER JOIN Chapters c ON t.ChapterId = c.Id
    INNER JOIN Sessions s ON t.SessionId = s.Id
    ORDER BY t.Id DESC;
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_TopicGetById')
    DROP PROCEDURE sp_TopicGetById;
GO
CREATE PROCEDURE sp_TopicGetById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Topics WHERE Id = @Id;
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_TopicToggleStatus')
    DROP PROCEDURE sp_TopicToggleStatus;
GO
CREATE PROCEDURE sp_TopicToggleStatus
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Topics SET Status = CASE WHEN Status = 1 THEN 0 ELSE 1 END WHERE Id = @Id;
END
GO

-- =======================================================
-- TOPIC DETAILS PROCEDURES
-- =======================================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_TopicDetailsUpsert')
    DROP PROCEDURE sp_TopicDetailsUpsert;
GO
CREATE PROCEDURE sp_TopicDetailsUpsert
    @Id INT,
    @SessionId INT,
    @ChapterId INT,
    @TopicId INT,
    @Title NVARCHAR(200),
    @Description NVARCHAR(MAX),
    @DurationMin INT,
    @FileType NVARCHAR(50),
    @FilePath NVARCHAR(MAX),
    @Position INT,
    @Status BIT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @Id = 0
    BEGIN
        INSERT INTO TopicDetails (SessionId, ChapterId, TopicId, Title, Description, DurationMin, FileType, FilePath, Position, Status, CreatedBy, CreatedDate)
        VALUES (@SessionId, @ChapterId, @TopicId, @Title, @Description, @DurationMin, @FileType, @FilePath, @Position, @Status, @UserId, GETDATE());
    END
    ELSE
    BEGIN
        UPDATE TopicDetails
        SET SessionId = @SessionId,
            ChapterId = @ChapterId,
            TopicId = @TopicId,
            Title = @Title,
            Description = @Description,
            DurationMin = @DurationMin,
            FileType = @FileType,
            FilePath = @FilePath,
            Position = @Position,
            Status = @Status,
            UpdatedBy = @UserId,
            UpdatedDate = GETDATE()
        WHERE Id = @Id;
    END
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_TopicDetailsGetByTopicId')
    DROP PROCEDURE sp_TopicDetailsGetByTopicId;
GO
CREATE PROCEDURE sp_TopicDetailsGetByTopicId
    @TopicId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM TopicDetails WHERE TopicId = @TopicId ORDER BY Position ASC;
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_TopicDetailsDeleteByTopicId')
    DROP PROCEDURE sp_TopicDetailsDeleteByTopicId;
GO
CREATE PROCEDURE sp_TopicDetailsDeleteByTopicId
    @TopicId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM TopicDetails WHERE TopicId = @TopicId;
END
GO
