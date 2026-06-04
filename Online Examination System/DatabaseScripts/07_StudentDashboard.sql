USE OnlineExamDb;
GO

IF OBJECT_ID('sp_StudentGetDashboardStats', 'P') IS NOT NULL DROP PROCEDURE sp_StudentGetDashboardStats;
GO

CREATE PROCEDURE sp_StudentGetDashboardStats
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TotalExams INT;
    DECLARE @CompletedExams INT;
    DECLARE @InProgressExams INT;
    DECLARE @NotStartedExams INT;

    -- Total available active exams
    SELECT @TotalExams = COUNT(ExamId)
    FROM Exams
    WHERE IsActive = 1;

    -- Completed exams by this student
    SELECT @CompletedExams = COUNT(StudentExamId)
    FROM StudentExams
    WHERE StudentId = @StudentId AND Status = 'Completed';

    -- In Progress exams by this student
    SELECT @InProgressExams = COUNT(StudentExamId)
    FROM StudentExams
    WHERE StudentId = @StudentId AND Status = 'In Progress';

    -- Not Started
    SET @NotStartedExams = @TotalExams - (@CompletedExams + @InProgressExams);
    IF @NotStartedExams < 0 SET @NotStartedExams = 0; -- Safety check

    SELECT 
        @TotalExams AS TotalExams,
        @CompletedExams AS CompletedExams,
        @InProgressExams AS InProgressExams,
        @NotStartedExams AS NotStartedExams;
END
GO
