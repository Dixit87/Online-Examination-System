USE OnlineExamDb;
GO

-- Table for Student Exam Results
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StudentExams')
BEGIN
    CREATE TABLE StudentExams (
        StudentExamId INT IDENTITY(1,1) PRIMARY KEY,
        ExamId INT FOREIGN KEY REFERENCES Exams(ExamId),
        StudentId INT FOREIGN KEY REFERENCES Users(UserId),
        Status NVARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'In Progress', 'Completed'
        Score DECIMAL(8,2) DEFAULT 0,
        TotalMark DECIMAL(8,2) DEFAULT 0,
        ResultPercentage DECIMAL(5,2) DEFAULT 0,
        AttemptDate DATETIME DEFAULT GETDATE()
    );
END
GO

-- Drop SPs if they exist
IF OBJECT_ID('sp_StudentGetAll', 'P') IS NOT NULL DROP PROCEDURE sp_StudentGetAll;
GO
IF OBJECT_ID('sp_StudentGetDetails', 'P') IS NOT NULL DROP PROCEDURE sp_StudentGetDetails;
GO
IF OBJECT_ID('sp_StudentExamsGetByStudentId', 'P') IS NOT NULL DROP PROCEDURE sp_StudentExamsGetByStudentId;
GO

-- SP to Get All Students (RoleId = 2)
CREATE PROCEDURE sp_StudentGetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        UserId,
        Name,
        Username,
        Email,
        Contact,
        IsActive,
        CreatedDate
    FROM Users
    WHERE RoleId = 2 -- 2 is Student
    ORDER BY UserId DESC;
END
GO

-- SP to Get Student Details By Id
CREATE PROCEDURE sp_StudentGetDetails
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        UserId,
        Name,
        Username,
        Email,
        Contact,
        Address,
        IsActive,
        CreatedDate
    FROM Users
    WHERE UserId = @UserId AND RoleId = 2;
END
GO

-- SP to Get Exam History for a Student
CREATE PROCEDURE sp_StudentExamsGetByStudentId
    @StudentId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        se.StudentExamId,
        se.ExamId,
        e.ExamTitle,
        se.Status,
        se.Score,
        se.TotalMark,
        se.ResultPercentage,
        se.AttemptDate
    FROM StudentExams se
    INNER JOIN Exams e ON se.ExamId = e.ExamId
    WHERE se.StudentId = @StudentId
    ORDER BY se.AttemptDate DESC;
END
GO
