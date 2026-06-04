USE OnlineExamDb;
GO

-- 1. Add CompletionDate to StudentExams if it doesn't exist
IF NOT EXISTS (
    SELECT * 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID('StudentExams') 
    AND name = 'CompletionDate'
)
BEGIN
    ALTER TABLE StudentExams ADD CompletionDate DATETIME NULL;
END
GO

-- 2. Modify sp_StudentSubmitExam to update CompletionDate
IF OBJECT_ID('sp_StudentSubmitExam', 'P') IS NOT NULL DROP PROCEDURE sp_StudentSubmitExam;
GO

CREATE PROCEDURE sp_StudentSubmitExam
    @StudentExamId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Only evaluate if not already completed
    IF (SELECT Status FROM StudentExams WHERE StudentExamId = @StudentExamId) = 'Completed'
    BEGIN
        RETURN;
    END

    -- Evaluate Answers
    UPDATE sea
    SET sea.IsCorrect = o.IsCorrect
    FROM StudentExamAnswers sea
    INNER JOIN QuestionOptions o ON sea.SelectedOptionId = o.OptionId
    WHERE sea.StudentExamId = @StudentExamId;
    
    -- Calculate Totals
    DECLARE @TotalAttempted INT;
    DECLARE @TotalRight INT;
    DECLARE @TotalWrong INT;
    DECLARE @Score DECIMAL(8,2);
    DECLARE @TotalMark DECIMAL(8,2);
    DECLARE @ResultPercentage DECIMAL(5,2);
    DECLARE @PerQuestionMark DECIMAL(8,2);
    
    SELECT 
        @TotalAttempted = COUNT(StudentExamAnswerId),
        @TotalRight = SUM(CASE WHEN IsCorrect = 1 THEN 1 ELSE 0 END),
        @TotalWrong = SUM(CASE WHEN IsCorrect = 0 THEN 1 ELSE 0 END)
    FROM StudentExamAnswers
    WHERE StudentExamId = @StudentExamId;

    -- Handle nulls if 0 attempted
    SET @TotalAttempted = ISNULL(@TotalAttempted, 0);
    SET @TotalRight = ISNULL(@TotalRight, 0);
    SET @TotalWrong = ISNULL(@TotalWrong, 0);
    
    -- Get Exam Details for Scoring
    SELECT 
        @PerQuestionMark = e.PerQuestionMark,
        @TotalMark = e.TotalMark
    FROM StudentExams se
    INNER JOIN Exams e ON se.ExamId = e.ExamId
    WHERE se.StudentExamId = @StudentExamId;
    
    SET @Score = @TotalRight * @PerQuestionMark;
    IF @TotalMark > 0
        SET @ResultPercentage = (@Score / @TotalMark) * 100;
    ELSE
        SET @ResultPercentage = 0;
        
    -- Update StudentExam
    UPDATE StudentExams
    SET 
        Status = 'Completed',
        TotalAttempted = @TotalAttempted,
        TotalRight = @TotalRight,
        TotalWrong = @TotalWrong,
        Score = @Score,
        ResultPercentage = @ResultPercentage,
        TotalMark = @TotalMark,
        CompletionDate = GETDATE()
    WHERE StudentExamId = @StudentExamId;
END
GO


-- 3. Modify sp_StudentGetExamList to return StudentExamId
IF OBJECT_ID('sp_StudentGetExamList', 'P') IS NOT NULL DROP PROCEDURE sp_StudentGetExamList;
GO

CREATE PROCEDURE sp_StudentGetExamList
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        e.ExamId,
        se.StudentExamId,
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

-- 4. Create sp_StudentGetExamResult
IF OBJECT_ID('sp_StudentGetExamResult', 'P') IS NOT NULL DROP PROCEDURE sp_StudentGetExamResult;
GO

CREATE PROCEDURE sp_StudentGetExamResult
    @StudentExamId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        et.Title AS ExamTypeName,
        e.ExamTitle AS ExamName,
        e.NoOfQuestions AS TotalQuestions,
        e.PerQuestionMark,
        e.TotalMark AS TotalMarks,
        e.PassingMark AS PassingMarks,
        se.TotalAttempted AS TotalAttemptedQuestions,
        se.TotalRight AS TotalCorrectAnswers,
        se.TotalWrong AS TotalWrongAnswers,
        se.ResultPercentage,
        se.Score AS Score,
        se.AttemptDate AS ExamStartDate,
        se.CompletionDate AS ExamFinishDate
    FROM StudentExams se
    INNER JOIN Exams e ON se.ExamId = e.ExamId
    INNER JOIN ExamTypes et ON e.ExamTypeId = et.ExamTypeId
    WHERE se.StudentExamId = @StudentExamId;
END
GO
