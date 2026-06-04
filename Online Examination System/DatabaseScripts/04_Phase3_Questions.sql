USE OnlineExamDb;
GO

-- Table for Questions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Questions')
BEGIN
    CREATE TABLE Questions (
        QuestionId INT IDENTITY(1,1) PRIMARY KEY,
        QuestionText NVARCHAR(MAX) NOT NULL,
        IsActive BIT DEFAULT 1,
        CreatedBy INT FOREIGN KEY REFERENCES Users(UserId),
        CreatedDate DATETIME DEFAULT GETDATE(),
        UpdatedDate DATETIME DEFAULT GETDATE()
    );
END
GO

-- Table for Options
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QuestionOptions')
BEGIN
    CREATE TABLE QuestionOptions (
        OptionId INT IDENTITY(1,1) PRIMARY KEY,
        QuestionId INT FOREIGN KEY REFERENCES Questions(QuestionId) ON DELETE CASCADE,
        OptionText NVARCHAR(500) NOT NULL,
        IsCorrect BIT DEFAULT 0
    );
END
GO

-- Drop SPs if they exist to support all SQL Server versions
IF OBJECT_ID('sp_QuestionUpsert', 'P') IS NOT NULL DROP PROCEDURE sp_QuestionUpsert;
GO
IF OBJECT_ID('sp_OptionsDeleteByQuestionId', 'P') IS NOT NULL DROP PROCEDURE sp_OptionsDeleteByQuestionId;
GO
IF OBJECT_ID('sp_OptionInsert', 'P') IS NOT NULL DROP PROCEDURE sp_OptionInsert;
GO
IF OBJECT_ID('sp_QuestionGetAll', 'P') IS NOT NULL DROP PROCEDURE sp_QuestionGetAll;
GO
IF OBJECT_ID('sp_QuestionGetById', 'P') IS NOT NULL DROP PROCEDURE sp_QuestionGetById;
GO
IF OBJECT_ID('sp_OptionsGetByQuestionId', 'P') IS NOT NULL DROP PROCEDURE sp_OptionsGetByQuestionId;
GO
IF OBJECT_ID('sp_QuestionToggleStatus', 'P') IS NOT NULL DROP PROCEDURE sp_QuestionToggleStatus;
GO

-- SP for Upserting Question
CREATE PROCEDURE sp_QuestionUpsert
    @QuestionId INT,
    @QuestionText NVARCHAR(MAX),
    @IsActive BIT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @QuestionId = 0
    BEGIN
        INSERT INTO Questions (QuestionText, IsActive, CreatedBy)
        VALUES (@QuestionText, @IsActive, @UserId);
        
        SELECT CAST(SCOPE_IDENTITY() AS INT);
    END
    ELSE
    BEGIN
        UPDATE Questions
        SET QuestionText = @QuestionText,
            IsActive = @IsActive,
            UpdatedDate = GETDATE()
        WHERE QuestionId = @QuestionId;
        
        SELECT @QuestionId;
    END
END
GO

-- SP to Delete Options (Before Re-inserting during an Edit)
CREATE PROCEDURE sp_OptionsDeleteByQuestionId
    @QuestionId INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM QuestionOptions WHERE QuestionId = @QuestionId;
END
GO

-- SP to Insert Option
CREATE PROCEDURE sp_OptionInsert
    @QuestionId INT,
    @OptionText NVARCHAR(500),
    @IsCorrect BIT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO QuestionOptions (QuestionId, OptionText, IsCorrect)
    VALUES (@QuestionId, @OptionText, @IsCorrect);
END
GO

-- SP to Get All Questions
CREATE PROCEDURE sp_QuestionGetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        q.QuestionId,
        q.QuestionText,
        q.IsActive,
        q.CreatedDate,
        (SELECT COUNT(*) FROM QuestionOptions o WHERE o.QuestionId = q.QuestionId) AS OptionsCount
    FROM Questions q
    ORDER BY q.QuestionId DESC;
END
GO

-- SP to Get Question By Id
CREATE PROCEDURE sp_QuestionGetById
    @QuestionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Questions WHERE QuestionId = @QuestionId;
END
GO

-- SP to Get Options By QuestionId
CREATE PROCEDURE sp_OptionsGetByQuestionId
    @QuestionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM QuestionOptions WHERE QuestionId = @QuestionId ORDER BY OptionId;
END
GO

-- SP to Toggle Status
CREATE PROCEDURE sp_QuestionToggleStatus
    @QuestionId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Questions 
    SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
    WHERE QuestionId = @QuestionId;
END
GO
