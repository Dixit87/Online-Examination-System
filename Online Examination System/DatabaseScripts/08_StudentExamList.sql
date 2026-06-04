USE OnlineExamDb;
GO

-- Alter StudentExams to add new tracking columns if they don't exist
IF NOT EXISTS (
    SELECT * 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('StudentExams') 
    AND name = 'TotalAttempted'
)
BEGIN
    ALTER TABLE StudentExams ADD TotalAttempted INT DEFAULT 0;
END
GO

IF NOT EXISTS (
    SELECT * 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('StudentExams') 
    AND name = 'TotalRight'
)
BEGIN
    ALTER TABLE StudentExams ADD TotalRight INT DEFAULT 0;
END
GO

IF NOT EXISTS (
    SELECT * 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('StudentExams') 
    AND name = 'TotalWrong'
)
BEGIN
    ALTER TABLE StudentExams ADD TotalWrong INT DEFAULT 0;
END
GO

IF OBJECT_ID('sp_StudentGetExamList', 'P') IS NOT NULL DROP PROCEDURE sp_StudentGetExamList;
GO

CREATE PROCEDURE sp_StudentGetExamList
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.ExamId,
        e.ExamTitle,
        e.NoOfQuestions AS TotalQuestions,
        ISNULL(se.TotalAttempted, 0) AS Attempted,
        ISNULL(se.TotalRight, 0) AS RightAnswers,
        ISNULL(se.TotalWrong, 0) AS WrongAnswers,
        ISNULL(se.Status, 'Not Started') AS Status,
        ISNULL(se.ResultPercentage, 0) AS ResultPercentage
    FROM Exams e
    LEFT JOIN StudentExams se ON e.ExamId = se.ExamId AND se.StudentId = @StudentId
    WHERE e.IsActive = 1
    ORDER BY e.CreatedDate DESC;
END
GO
