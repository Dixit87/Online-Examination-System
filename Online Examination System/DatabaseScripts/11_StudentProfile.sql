USE OnlineExamDb;
GO

IF OBJECT_ID('sp_StudentProfileUpdate', 'P') IS NOT NULL DROP PROCEDURE sp_StudentProfileUpdate;
GO

CREATE PROCEDURE sp_StudentProfileUpdate
    @UserId INT,
    @Name NVARCHAR(100),
    @Email NVARCHAR(100),
    @Contact NVARCHAR(15),
    @Address NVARCHAR(MAX),
    @PasswordHash NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @PasswordHash IS NOT NULL AND @PasswordHash <> ''
    BEGIN
        UPDATE Users
        SET 
            Name = @Name,
            Email = @Email,
            Contact = @Contact,
            Address = @Address,
            PasswordHash = @PasswordHash,
            UpdatedDate = GETDATE()
        WHERE UserId = @UserId;
    END
    ELSE
    BEGIN
        UPDATE Users
        SET 
            Name = @Name,
            Email = @Email,
            Contact = @Contact,
            Address = @Address,
            UpdatedDate = GETDATE()
        WHERE UserId = @UserId;
    END
END
GO
