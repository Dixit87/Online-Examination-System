CREATE PROCEDURE sp_GetDashboardStats
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalUsers INT;
    DECLARE @TotalStudents INT;
    DECLARE @TotalExamTypes INT;
    DECLARE @TotalQuestions INT = 0; -- Placeholder for future Phase
    DECLARE @TotalExams INT = 0; -- Placeholder
    DECLARE @CompletedExams INT = 0; -- Placeholder
    DECLARE @PassedStudents INT = 0; -- Placeholder
    DECLARE @FailedStudents INT = 0; -- Placeholder

    SELECT @TotalUsers = COUNT(*) FROM Users;
    
    SELECT @TotalStudents = COUNT(*) 
    FROM Users u 
    INNER JOIN Roles r ON u.RoleId = r.RoleId 
    WHERE r.RoleName = 'Student';

    SELECT @TotalExamTypes = COUNT(*) FROM ExamTypes;

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
