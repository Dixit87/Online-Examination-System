USE OnlineExamDb;
GO

-- 1. Create StudentExamAnswers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StudentExamAnswers')
BEGIN
    CREATE TABLE StudentExamAnswers (
        StudentExamAnswerId INT IDENTITY(1,1) PRIMARY KEY,
        StudentExamId INT FOREIGN KEY REFERENCES StudentExams(StudentExamId),
        QuestionId INT FOREIGN KEY REFERENCES Questions(QuestionId),
        SelectedOptionId INT FOREIGN KEY REFERENCES QuestionOptions(OptionId),
        IsCorrect BIT DEFAULT 0, -- Evaluated on submit
        UNIQUE(StudentExamId, QuestionId) -- Ensure one answer per question per exam attempt
    );
END
GO

-- Drop existing SPs
IF OBJECT_ID('sp_StudentExamInitialize', 'P') IS NOT NULL DROP PROCEDURE sp_StudentExamInitialize;
GO
IF OBJECT_ID('sp_StudentGetExamData', 'P') IS NOT NULL DROP PROCEDURE sp_StudentGetExamData;
GO
IF OBJECT_ID('sp_StudentSaveAnswer', 'P') IS NOT NULL DROP PROCEDURE sp_StudentSaveAnswer;
GO
IF OBJECT_ID('sp_StudentSubmitExam', 'P') IS NOT NULL DROP PROCEDURE sp_StudentSubmitExam;
GO

-- 2. SP to Initialize or Resume Exam
CREATE PROCEDURE sp_StudentExamInitialize
    @ExamId INT,
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @StudentExamId INT;
    
    SELECT @StudentExamId = StudentExamId 
    FROM StudentExams 
    WHERE ExamId = @ExamId AND StudentId = @StudentId;
    
    IF @StudentExamId IS NULL
    BEGIN
        INSERT INTO StudentExams (ExamId, StudentId, Status, AttemptDate)
        VALUES (@ExamId, @StudentId, 'In Progress', GETDATE());
        
        SET @StudentExamId = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        -- If it was 'Not Started' or anything else, ensure it is 'In Progress'
        -- Do not change if it's already 'Completed' (handled in controller)
        IF (SELECT Status FROM StudentExams WHERE StudentExamId = @StudentExamId) <> 'Completed'
        BEGIN
            UPDATE StudentExams SET Status = 'In Progress' WHERE StudentExamId = @StudentExamId;
        END
    END
    
    SELECT @StudentExamId AS StudentExamId;
END
GO

-- 3. SP to Get Exam Questions, Options, and Existing Answers
CREATE PROCEDURE sp_StudentGetExamData
    @StudentExamId INT,
    @ExamId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Result Set 1: Questions
    SELECT 
        q.QuestionId, 
        q.QuestionText,
        ISNULL(sea.SelectedOptionId, 0) AS SavedOptionId
    FROM ExamQuestions eq
    INNER JOIN Questions q ON eq.QuestionId = q.QuestionId
    LEFT JOIN StudentExamAnswers sea ON sea.QuestionId = q.QuestionId AND sea.StudentExamId = @StudentExamId
    WHERE eq.ExamId = @ExamId;
    
    -- Result Set 2: Options
    SELECT 
        o.OptionId,
        o.QuestionId,
        o.OptionText
    FROM QuestionOptions o
    INNER JOIN ExamQuestions eq ON o.QuestionId = eq.QuestionId
    WHERE eq.ExamId = @ExamId;
END
GO

-- 4. SP to Save Single Answer
CREATE PROCEDURE sp_StudentSaveAnswer
    @StudentExamId INT,
    @QuestionId INT,
    @SelectedOptionId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if exam is already completed
    IF (SELECT Status FROM StudentExams WHERE StudentExamId = @StudentExamId) = 'Completed'
    BEGIN
        RETURN;
    END

    -- Upsert Answer
    IF EXISTS (SELECT 1 FROM StudentExamAnswers WHERE StudentExamId = @StudentExamId AND QuestionId = @QuestionId)
    BEGIN
        UPDATE StudentExamAnswers
        SET SelectedOptionId = @SelectedOptionId
        WHERE StudentExamId = @StudentExamId AND QuestionId = @QuestionId;
    END
    ELSE
    BEGIN
        INSERT INTO StudentExamAnswers (StudentExamId, QuestionId, SelectedOptionId)
        VALUES (@StudentExamId, @QuestionId, @SelectedOptionId);
    END
END
GO

-- 5. SP to Submit and Evaluate Exam
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
        TotalMark = @TotalMark
    WHERE StudentExamId = @StudentExamId;
END
GO
