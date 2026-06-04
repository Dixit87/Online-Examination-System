USE OnlineExamDb;
GO

IF OBJECT_ID('sp_GetDashboardStats', 'P') IS NOT NULL DROP PROCEDURE sp_GetDashboardStats;
GO

CREATE PROCEDURE sp_GetDashboardStats
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalUsers INT;
    DECLARE @TotalStudents INT;
    DECLARE @TotalExamTypes INT;
    DECLARE @TotalQuestions INT;
    DECLARE @TotalExams INT;
    DECLARE @CompletedExams INT;
    DECLARE @PassedStudents INT;
    DECLARE @FailedStudents INT;

    SELECT @TotalUsers = COUNT(*) FROM Users;
    
    SELECT @TotalStudents = COUNT(*) 
    FROM Users u 
    INNER JOIN Roles r ON u.RoleId = r.RoleId 
    WHERE r.RoleName = 'Student';

    SELECT @TotalExamTypes = COUNT(*) FROM ExamTypes;

    SELECT @TotalQuestions = COUNT(*) FROM Questions;

    SELECT @TotalExams = COUNT(*) FROM Exams;

    SELECT @CompletedExams = COUNT(*) FROM StudentExams WHERE Status = 'Completed';

    SELECT @PassedStudents = COUNT(*) 
    FROM StudentExams se
    INNER JOIN Exams e ON se.ExamId = e.ExamId
    WHERE se.Status = 'Completed' AND se.Score >= e.PassingMark;

    SELECT @FailedStudents = COUNT(*) 
    FROM StudentExams se
    INNER JOIN Exams e ON se.ExamId = e.ExamId
    WHERE se.Status = 'Completed' AND se.Score < e.PassingMark;

    -- Return as a single row
    SELECT 
        @TotalUsers AS TotalUsers,
        @TotalStudents AS TotalStudents,
        @TotalExamTypes AS TotalExamTypes,
        @TotalQuestions AS TotalQuestions,
        @TotalExams AS TotalExams,
        @CompletedExams AS CompletedExams,
        @PassedStudents AS PassedStudents,
        @FailedStudents AS FailedStudents;
END
GO
