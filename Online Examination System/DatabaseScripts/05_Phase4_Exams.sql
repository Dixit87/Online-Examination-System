USE OnlineExamDb;
GO

-- Table for Exams
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Exams')
BEGIN
    CREATE TABLE Exams (
        ExamId INT IDENTITY(1,1) PRIMARY KEY,
        ExamTypeId INT FOREIGN KEY REFERENCES ExamTypes(ExamTypeId),
        ExamTitle NVARCHAR(200) NOT NULL,
        NoOfQuestions INT NOT NULL,
        PerQuestionMark DECIMAL(5,2) NOT NULL,
        TotalMark DECIMAL(8,2) NOT NULL,
        PassingMark DECIMAL(8,2) NOT NULL,
        IsActive BIT DEFAULT 1,
        CreatedBy INT FOREIGN KEY REFERENCES Users(UserId),
        CreatedDate DATETIME DEFAULT GETDATE(),
        UpdatedDate DATETIME DEFAULT GETDATE()
    );
END
GO

-- Table for Exam Question Mappings
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExamQuestions')
BEGIN
    CREATE TABLE ExamQuestions (
        ExamId INT FOREIGN KEY REFERENCES Exams(ExamId) ON DELETE CASCADE,
        QuestionId INT FOREIGN KEY REFERENCES Questions(QuestionId) ON DELETE CASCADE,
        PRIMARY KEY (ExamId, QuestionId)
    );
END
GO

-- Drop SPs if they exist
IF OBJECT_ID('sp_ExamUpsert', 'P') IS NOT NULL DROP PROCEDURE sp_ExamUpsert;
GO
IF OBJECT_ID('sp_ExamGetAll', 'P') IS NOT NULL DROP PROCEDURE sp_ExamGetAll;
GO
IF OBJECT_ID('sp_ExamGetById', 'P') IS NOT NULL DROP PROCEDURE sp_ExamGetById;
GO
IF OBJECT_ID('sp_ExamQuestionsUpsert', 'P') IS NOT NULL DROP PROCEDURE sp_ExamQuestionsUpsert;
GO
IF OBJECT_ID('sp_ExamToggleStatus', 'P') IS NOT NULL DROP PROCEDURE sp_ExamToggleStatus;
GO

-- SP for Upserting Exam
CREATE PROCEDURE sp_ExamUpsert
    @ExamId INT,
    @ExamTypeId INT,
    @ExamTitle NVARCHAR(200),
    @NoOfQuestions INT,
    @PerQuestionMark DECIMAL(5,2),
    @TotalMark DECIMAL(8,2),
    @PassingMark DECIMAL(8,2),
    @IsActive BIT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF @ExamId = 0
    BEGIN
        INSERT INTO Exams (ExamTypeId, ExamTitle, NoOfQuestions, PerQuestionMark, TotalMark, PassingMark, IsActive, CreatedBy)
        VALUES (@ExamTypeId, @ExamTitle, @NoOfQuestions, @PerQuestionMark, @TotalMark, @PassingMark, @IsActive, @UserId);
        
        SELECT CAST(SCOPE_IDENTITY() AS INT);
    END
    ELSE
    BEGIN
        UPDATE Exams
        SET ExamTypeId = @ExamTypeId,
            ExamTitle = @ExamTitle,
            NoOfQuestions = @NoOfQuestions,
            PerQuestionMark = @PerQuestionMark,
            TotalMark = @TotalMark,
            PassingMark = @PassingMark,
            IsActive = @IsActive,
            UpdatedDate = GETDATE()
        WHERE ExamId = @ExamId;
        
        SELECT @ExamId;
    END
END
GO

-- SP for Upserting Exam Questions (Deletes all mapped questions and re-inserts)
CREATE PROCEDURE sp_ExamQuestionsUpsert
    @ExamId INT,
    @QuestionId INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ExamQuestions (ExamId, QuestionId)
    VALUES (@ExamId, @QuestionId);
END
GO

-- SP to Get All Exams
CREATE PROCEDURE sp_ExamGetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        e.ExamId,
        et.Title AS ExamTypeName,
        e.ExamTitle,
        e.NoOfQuestions,
        e.TotalMark,
        e.PassingMark,
        e.IsActive,
        e.CreatedDate
    FROM Exams e
    INNER JOIN ExamTypes et ON e.ExamTypeId = et.ExamTypeId
    ORDER BY e.ExamId DESC;
END
GO

-- SP to Get Exam By Id
CREATE PROCEDURE sp_ExamGetById
    @ExamId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM Exams WHERE ExamId = @ExamId;
    
    -- Return selected QuestionIds
    SELECT QuestionId FROM ExamQuestions WHERE ExamId = @ExamId;
END
GO

-- SP to Toggle Exam Status
CREATE PROCEDURE sp_ExamToggleStatus
    @ExamId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Exams 
    SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
    WHERE ExamId = @ExamId;
END
GO
