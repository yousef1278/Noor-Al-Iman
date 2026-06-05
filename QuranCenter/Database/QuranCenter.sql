-- ============================================================
-- Quran Memorization Center Database
-- Course: Database Programming
-- University: Amman Arab University
-- ============================================================

USE master;
GO

-- Close all connections and drop if exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'QuranCenter')
BEGIN
    DECLARE @kill VARCHAR(8000) = '';
    SELECT @kill = @kill + 'KILL ' + CONVERT(VARCHAR(5), session_id) + ';'
    FROM   sys.dm_exec_sessions
    WHERE  database_id = DB_ID('QuranCenter')
      AND  session_id  <> @@SPID;   -- don't kill our own session
    EXEC(@kill);

    DROP DATABASE QuranCenter;
END
GO

CREATE DATABASE QuranCenter;
GO

USE QuranCenter;
GO

-- ============================================================
-- TABLE: ZipCodes  (resolve transitive dependency - 3NF)
-- ============================================================
CREATE TABLE ZipCodes (
    ZipCode   VARCHAR(10)  NOT NULL PRIMARY KEY,
    City      NVARCHAR(50) NOT NULL,
    State     NVARCHAR(50) NOT NULL
);
GO

-- ============================================================
-- TABLE: Person  (superclass)
-- ============================================================
CREATE TABLE Person (
    PersonID    INT           NOT NULL PRIMARY KEY IDENTITY(1,1),
    FirstName   NVARCHAR(50)  NOT NULL,
    LastName    NVARCHAR(50)  NOT NULL,
    Gender      CHAR(1)       NOT NULL CHECK (Gender IN ('M','F')),
    DateOfBirth DATE          NOT NULL,
    Address     NVARCHAR(100) NULL,
    ZipCode     VARCHAR(10)   NULL REFERENCES ZipCodes(ZipCode),
    Email       VARCHAR(100)  NULL UNIQUE
);
GO

-- ============================================================
-- TABLE: PhoneNumbers  (multi-valued attribute)
-- ============================================================
CREATE TABLE PhoneNumbers (
    PhoneID     INT         NOT NULL PRIMARY KEY IDENTITY(1,1),
    PersonID    INT         NOT NULL REFERENCES Person(PersonID) ON DELETE CASCADE,
    PhoneNumber VARCHAR(20) NOT NULL
);
GO

-- ============================================================
-- TABLE: Student
-- ============================================================
CREATE TABLE Student (
    StudentID       INT NOT NULL PRIMARY KEY REFERENCES Person(PersonID) ON DELETE CASCADE,
    EnrollmentDate  DATE NOT NULL DEFAULT GETDATE(),
    Level           NVARCHAR(50) NOT NULL DEFAULT N'Beginner'
);
GO

-- ============================================================
-- TABLE: Teacher
-- ============================================================
CREATE TABLE Teacher (
    TeacherID       INT NOT NULL PRIMARY KEY REFERENCES Person(PersonID) ON DELETE CASCADE,
    Specialization  NVARCHAR(100) NOT NULL,
    HireDate        DATE NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================
-- TABLE: Supervisor
-- ============================================================
CREATE TABLE Supervisor (
    SupervisorID    INT NOT NULL PRIMARY KEY REFERENCES Person(PersonID) ON DELETE CASCADE,
    Department      NVARCHAR(100) NOT NULL
);
GO

-- ============================================================
-- TABLE: Curriculum
-- ============================================================
CREATE TABLE Curriculum (
    CurriculumID    INT           NOT NULL PRIMARY KEY IDENTITY(1,1),
    CurriculumName  NVARCHAR(100) NOT NULL UNIQUE,
    Description     NVARCHAR(500) NULL,
    Category        NVARCHAR(50)  NOT NULL
        CHECK (Category IN (N'Holy Quran', N'Tajweed', N'Aqeedah', N'Hadith', N'Tafsir'))
);
GO

-- ============================================================
-- TABLE: Classroom
-- ============================================================
CREATE TABLE Classroom (
    ClassroomID     INT           NOT NULL PRIMARY KEY IDENTITY(1,1),
    ClassroomName   NVARCHAR(100) NOT NULL,
    MaxSize         INT           NOT NULL CHECK (MaxSize > 0),
    CurriculumID    INT           NOT NULL REFERENCES Curriculum(CurriculumID),
    TeacherID       INT           NOT NULL REFERENCES Teacher(TeacherID),
    SupervisorID    INT           NULL     REFERENCES Supervisor(SupervisorID),
    RequiredLevel   NVARCHAR(20)  NOT NULL DEFAULT N'Beginner'
);
GO

-- ============================================================
-- TABLE: Enrollment  (Student ↔ Classroom)
-- ============================================================
CREATE TABLE Enrollment (
    EnrollmentID    INT  NOT NULL PRIMARY KEY IDENTITY(1,1),
    StudentID       INT  NOT NULL REFERENCES Student(StudentID),
    ClassroomID     INT  NOT NULL REFERENCES Classroom(ClassroomID),
    EnrollmentDate  DATE NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_Enrollment UNIQUE (StudentID, ClassroomID)
);
GO

-- ============================================================
-- TABLE: Attendance
-- ============================================================
CREATE TABLE Attendance (
    AttendanceID    INT           NOT NULL PRIMARY KEY IDENTITY(1,1),
    StudentID       INT           NOT NULL REFERENCES Student(StudentID),
    ClassroomID     INT           NOT NULL REFERENCES Classroom(ClassroomID),
    AttendanceDate  DATE          NOT NULL DEFAULT GETDATE(),
    Status          CHAR(1)       NOT NULL CHECK (Status IN ('P','A','L')), -- Present/Absent/Late
    Notes           NVARCHAR(300) NULL
);
GO

-- ============================================================
-- TABLE: Memorization
-- ============================================================
CREATE TABLE Memorization (
    MemorizationID  INT           NOT NULL PRIMARY KEY IDENTITY(1,1),
    StudentID       INT           NOT NULL REFERENCES Student(StudentID),
    SurahName       NVARCHAR(100) NOT NULL,
    FromAyah        INT           NOT NULL CHECK (FromAyah >= 1),
    ToAyah          INT           NOT NULL CHECK (ToAyah >= 1),
    DateCompleted   DATE          NOT NULL DEFAULT GETDATE(),
    Rating          TINYINT       NULL CHECK (Rating BETWEEN 1 AND 5)
);
GO

-- ============================================================
-- TABLE: Gift
-- ============================================================
CREATE TABLE Gift (
    GiftID      INT           NOT NULL PRIMARY KEY IDENTITY(1,1),
    GiftName    NVARCHAR(100) NOT NULL,
    GiftType    NVARCHAR(50)  NOT NULL,
    Quantity    INT           NOT NULL DEFAULT 0 CHECK (Quantity >= 0)
);
GO

-- ============================================================
-- TABLE: GiftDistribution
-- ============================================================
CREATE TABLE GiftDistribution (
    DistributionID  INT  NOT NULL PRIMARY KEY IDENTITY(1,1),
    GiftID          INT  NOT NULL REFERENCES Gift(GiftID),
    StudentID       INT  NOT NULL REFERENCES Student(StudentID),
    DateReceived    DATE NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================
-- TRIGGER: Prevent exceeding classroom capacity
-- ============================================================
CREATE TRIGGER trg_CheckClassroomCapacity
ON Enrollment
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1
        FROM inserted i
        JOIN Classroom c ON c.ClassroomID = i.ClassroomID
        WHERE (
            SELECT COUNT(*) FROM Enrollment e
            WHERE e.ClassroomID = i.ClassroomID
        ) > c.MaxSize
    )
    BEGIN
        RAISERROR(N'Cannot enroll: classroom has reached maximum capacity.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

-- ============================================================
-- TRIGGER: Reduce gift quantity on distribution
-- ============================================================
CREATE TRIGGER trg_ReduceGiftQuantity
ON GiftDistribution
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE g
    SET    g.Quantity = g.Quantity - 1
    FROM   Gift g
    JOIN   inserted i ON i.GiftID = g.GiftID;

    IF EXISTS (SELECT 1 FROM Gift WHERE Quantity < 0)
    BEGIN
        RAISERROR(N'Cannot distribute gift: quantity is zero.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;
GO

-- ============================================================
-- VIEWS
-- ============================================================
CREATE VIEW vw_StudentDetails AS
SELECT
    p.PersonID,
    p.FirstName + ' ' + p.LastName AS FullName,
    p.Gender,
    p.DateOfBirth,
    p.Email,
    s.Level,
    s.EnrollmentDate
FROM Person p
JOIN Student s ON s.StudentID = p.PersonID;
GO

CREATE VIEW vw_ClassroomInfo AS
SELECT
    c.ClassroomID,
    c.ClassroomName,
    c.MaxSize,
    (SELECT COUNT(*) FROM Enrollment e WHERE e.ClassroomID = c.ClassroomID) AS CurrentSize,
    cu.CurriculumName,
    cu.Category,
    tp.FirstName + ' ' + tp.LastName AS TeacherName
FROM Classroom c
JOIN Curriculum cu ON cu.CurriculumID = c.CurriculumID
JOIN Teacher   t  ON t.TeacherID = c.TeacherID
JOIN Person    tp ON tp.PersonID  = t.TeacherID;
GO

CREATE VIEW vw_AttendanceSummary AS
SELECT
    s.StudentID,
    p.FirstName + ' ' + p.LastName AS StudentName,
    COUNT(CASE WHEN a.Status = 'P' THEN 1 END) AS PresentCount,
    COUNT(CASE WHEN a.Status = 'A' THEN 1 END) AS AbsentCount,
    COUNT(CASE WHEN a.Status = 'L' THEN 1 END) AS LateCount,
    COUNT(*) AS TotalSessions
FROM Student s
JOIN Person     p ON p.PersonID  = s.StudentID
LEFT JOIN Attendance a ON a.StudentID = s.StudentID
GROUP BY s.StudentID, p.FirstName, p.LastName;
GO

-- ============================================================
-- STORED PROCEDURES
-- ============================================================

-- Get all students never received a gift
CREATE PROCEDURE sp_StudentsWithNoGifts
AS
BEGIN
    SELECT p.PersonID, p.FirstName + ' ' + p.LastName AS FullName, p.Email
    FROM Person p
    JOIN Student s ON s.StudentID = p.PersonID
    WHERE s.StudentID NOT IN (SELECT DISTINCT StudentID FROM GiftDistribution);
END;
GO

-- Get memorization progress for a student
CREATE PROCEDURE sp_StudentMemorizationProgress
    @StudentID INT
AS
BEGIN
    SELECT
        m.SurahName,
        m.FromAyah,
        m.ToAyah,
        m.DateCompleted,
        m.Rating
    FROM Memorization m
    WHERE m.StudentID = @StudentID
    ORDER BY m.DateCompleted;
END;
GO

-- Enroll a student in a classroom
CREATE PROCEDURE sp_EnrollStudent
    @StudentID  INT,
    @ClassroomID INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Enrollment WHERE StudentID = @StudentID AND ClassroomID = @ClassroomID)
    BEGIN
        RAISERROR(N'Student is already enrolled in this classroom.', 16, 1);
        RETURN;
    END
    INSERT INTO Enrollment (StudentID, ClassroomID) VALUES (@StudentID, @ClassroomID);
    PRINT N'Student enrolled successfully.';
END;
GO

-- Record attendance
CREATE PROCEDURE sp_RecordAttendance
    @StudentID    INT,
    @ClassroomID  INT,
    @Date         DATE,
    @Status       CHAR(1),
    @Notes        NVARCHAR(300) = NULL
AS
BEGIN
    INSERT INTO Attendance (StudentID, ClassroomID, AttendanceDate, Status, Notes)
    VALUES (@StudentID, @ClassroomID, @Date, @Status, @Notes);
END;
GO

-- ============================================================
-- SEED DATA
-- ============================================================
INSERT INTO ZipCodes VALUES ('11110', N'عمان', N'الأردن');
INSERT INTO ZipCodes VALUES ('11120', N'إربد', N'الأردن');
INSERT INTO ZipCodes VALUES ('11130', N'الزرقاء', N'الأردن');
GO

INSERT INTO Person (FirstName, LastName, Gender, DateOfBirth, Address, ZipCode, Email) VALUES
(N'أحمد',   N'الخالد',  'M', '2000-03-15', N'شارع الملك عبدالله', '11110', 'ahmed@quran.jo'),
(N'سارة',   N'محمد',    'F', '2001-07-22', N'شارع الأردن',         '11110', 'sara@quran.jo'),
(N'محمد',   N'العلي',   'M', '1999-01-10', N'شارع الحمراء',        '11120', 'mohamad@quran.jo'),
(N'فاطمة',  N'الزيد',   'F', '2002-05-30', N'شارع النخيل',         '11110', 'fatima@quran.jo'),
(N'عمر',    N'السالم',  'M', '2000-11-08', N'شارع الجامعة',        '11130', 'omar@quran.jo'),
(N'نورة',   N'الرشيد',  'F', '1985-04-18', N'شارع القدس',          '11110', 'noura@quran.jo'),
(N'خالد',   N'المنصور', 'M', '1980-09-25', N'شارع الملكة',         '11120', 'khaled@quran.jo'),
(N'منى',    N'العمر',   'F', '1990-02-14', N'شارع التقنية',        '11130', 'mona@quran.jo');
GO

-- Students: PersonID 1-5
INSERT INTO Student (StudentID, Level) VALUES (1, N'Beginner');
INSERT INTO Student (StudentID, Level) VALUES (2, N'Intermediate');
INSERT INTO Student (StudentID, Level) VALUES (3, N'Advanced');
INSERT INTO Student (StudentID, Level) VALUES (4, N'Beginner');
INSERT INTO Student (StudentID, Level) VALUES (5, N'Intermediate');
GO

-- Teachers: PersonID 6-7
INSERT INTO Teacher (TeacherID, Specialization) VALUES (6, N'Tajweed & Holy Quran');
INSERT INTO Teacher (TeacherID, Specialization) VALUES (7, N'Aqeedah & Hadith');
GO

-- Supervisor: PersonID 8
INSERT INTO Supervisor (SupervisorID, Department) VALUES (8, N'Academic Affairs');
GO

INSERT INTO Curriculum (CurriculumName, Description, Category) VALUES
(N'Quran Memorization', N'Full memorization of the Holy Quran', N'Holy Quran'),
(N'Tajweed Rules',      N'Rules of correct Quran recitation',   N'Tajweed'),
(N'Islamic Creed',      N'Fundamentals of Islamic belief',      N'Aqeedah'),
(N'Hadith Studies',     N'Study of Prophet traditions',         N'Hadith');
GO

INSERT INTO Classroom (ClassroomName, MaxSize, CurriculumID, TeacherID, SupervisorID) VALUES
(N'Quran Class A', 10, 1, 6, 8),
(N'Tajweed Class', 15, 2, 6, 8),
(N'Hadith Class',  12, 4, 7, 8);
GO

INSERT INTO PhoneNumbers (PersonID, PhoneNumber) VALUES
(1, '0791234567'), (2, '0792345678'), (3, '0793456789'),
(4, '0794567890'), (5, '0795678901'), (6, '0796789012'),
(7, '0797890123'), (8, '0798901234');
GO

INSERT INTO Enrollment (StudentID, ClassroomID) VALUES (1,1),(2,1),(3,2),(4,2),(5,3);
GO

INSERT INTO Attendance (StudentID, ClassroomID, AttendanceDate, Status) VALUES
(1,1,'2026-05-01','P'),(1,1,'2026-05-02','P'),(1,1,'2026-05-05','A'),
(2,1,'2026-05-01','P'),(2,1,'2026-05-02','L'),(2,1,'2026-05-05','P'),
(3,2,'2026-05-01','P'),(3,2,'2026-05-02','P'),(4,2,'2026-05-01','A'),
(5,3,'2026-05-01','P'),(5,3,'2026-05-02','P');
GO

INSERT INTO Memorization (StudentID, SurahName, FromAyah, ToAyah, DateCompleted, Rating) VALUES
(1, N'Al-Fatiha', 1, 7,  '2026-04-10', 5),
(1, N'Al-Baqara', 1, 20, '2026-04-25', 4),
(2, N'Al-Fatiha', 1, 7,  '2026-04-12', 5),
(3, N'Al-Imran',  1, 30, '2026-04-20', 5),
(5, N'Al-Fatiha', 1, 7,  '2026-05-01', 4);
GO

INSERT INTO Gift (GiftName, GiftType, Quantity) VALUES
(N'Quran Book',    N'Book',  20),
(N'Prayer Beads',  N'Item',  30),
(N'Certificate',   N'Award', 50);
GO

INSERT INTO GiftDistribution (GiftID, StudentID, DateReceived) VALUES
(1, 1, '2026-05-10'), (2, 1, '2026-05-15'),
(1, 3, '2026-05-12'), (3, 2, '2026-05-20');
GO

PRINT N'Database QuranCenter created and seeded successfully.';
GO
