-- ============================================================
-- PayrollApp Stored Procedures (GUID-compatible schema)
-- ============================================================

-- ============================================================
-- sp_PayrollList - Paginated payroll listing with search/filter
-- ============================================================
CREATE OR ALTER PROCEDURE sp_PayrollList
    @Month    INT = NULL,
    @Year     INT = NULL,
    @Status   NVARCHAR(20) = NULL,
    @Search   NVARCHAR(100) = NULL,
    @PageNum  INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Skip INT = (@PageNum - 1) * @PageSize;

    SELECT
        p.Id, p.EmployeeId,
        e.EmployeeCode,
        e.FirstName + ' ' + e.LastName AS EmployeeName,
        e.Department,
        pm.Month, pm.Year,
        p.GrossSalary, p.TaxDeduction, p.OtherDeductions, p.NetSalary,
        p.Status, p.ProcessedDate
    FROM Payrolls p
    INNER JOIN Employees e ON e.Id = p.EmployeeId
    INNER JOIN PayrollMonths pm ON pm.Id = p.PayrollMonthId
    WHERE ISNULL(p.IsDeleted, 0) = 0
      AND (@Month IS NULL OR pm.Month = @Month)
      AND (@Year IS NULL OR pm.Year = @Year)
      AND (@Status IS NULL OR p.Status = @Status)
      AND (@Search IS NULL
           OR e.FirstName LIKE '%' + @Search + '%'
           OR e.LastName LIKE '%' + @Search + '%'
           OR e.EmployeeCode LIKE '%' + @Search + '%')
    ORDER BY pm.Year DESC, pm.Month DESC, e.EmployeeCode
    OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*)
    FROM Payrolls p
    INNER JOIN Employees e ON e.Id = p.EmployeeId
    INNER JOIN PayrollMonths pm ON pm.Id = p.PayrollMonthId
    WHERE ISNULL(p.IsDeleted, 0) = 0
      AND (@Month IS NULL OR pm.Month = @Month)
      AND (@Year IS NULL OR pm.Year = @Year)
      AND (@Status IS NULL OR p.Status = @Status)
      AND (@Search IS NULL
           OR e.FirstName LIKE '%' + @Search + '%'
           OR e.LastName LIKE '%' + @Search + '%'
           OR e.EmployeeCode LIKE '%' + @Search + '%');
END;
GO

-- ============================================================
-- sp_GetPayrollById - Payroll detail with joins
-- ============================================================
CREATE OR ALTER PROCEDURE sp_GetPayrollById
    @PayrollId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id, p.EmployeeId,
        e.EmployeeCode, e.FirstName + ' ' + e.LastName AS EmployeeName,
        e.Department, e.Designation,
        p.PayrollMonthId, pm.Month, pm.Year,
        p.GrossSalary, p.TaxDeduction, p.OtherDeductions,
        p.NetSalary, p.Status, p.ProcessedDate,
        u.FirstName + ' ' + u.LastName AS ProcessedByName,
        p.CreatedDate
    FROM Payrolls p
    INNER JOIN Employees e ON e.Id = p.EmployeeId
    INNER JOIN PayrollMonths pm ON pm.Id = p.PayrollMonthId
    LEFT JOIN Users u ON u.Id = p.CreatedBy
    WHERE p.Id = @PayrollId;
END;
GO

-- ============================================================
-- sp_CreatePayroll - Insert with output
-- ============================================================
CREATE OR ALTER PROCEDURE sp_CreatePayroll
    @EmployeeId      UNIQUEIDENTIFIER,
    @PayrollMonthId  UNIQUEIDENTIFIER,
    @GrossSalary     DECIMAL(18,2),
    @TaxDeduction    DECIMAL(18,2),
    @OtherDeductions DECIMAL(18,2),
    @NetSalary       DECIMAL(18,2),
    @Status          NVARCHAR(20),
    @CreatedBy       UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLare @NewId TABLE (Id UNIQUEIDENTIFIER);

    INSERT INTO Payrolls
        (Id, EmployeeId, PayrollMonthId, GrossSalary, TaxDeduction,
         OtherDeductions, NetSalary, Status, ProcessedDate,
         CreatedBy, CreatedDate)
    OUTPUT INSERTED.Id INTO @NewId
    VALUES
        (NEWID(), @EmployeeId, @PayrollMonthId, @GrossSalary, @TaxDeduction,
         @OtherDeductions, @NetSalary, @Status, GETUTCDATE(),
         @CreatedBy, GETUTCDATE());

    SELECT p.* FROM Payrolls p INNER JOIN @NewId n ON n.Id = p.Id;
END;
GO

-- ============================================================
-- sp_UpdatePayroll - Partial update
-- ============================================================
CREATE OR ALTER PROCEDURE sp_UpdatePayroll
    @PayrollId       UNIQUEIDENTIFIER,
    @GrossSalary     DECIMAL(18,2) = NULL,
    @TaxDeduction    DECIMAL(18,2) = NULL,
    @OtherDeductions DECIMAL(18,2) = NULL,
    @NetSalary       DECIMAL(18,2) = NULL,
    @Status          NVARCHAR(20) = NULL,
    @UpdatedBy       UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Payrolls
    SET
        GrossSalary     = ISNULL(@GrossSalary, GrossSalary),
        TaxDeduction    = ISNULL(@TaxDeduction, TaxDeduction),
        OtherDeductions = ISNULL(@OtherDeductions, OtherDeductions),
        NetSalary       = ISNULL(@NetSalary, NetSalary),
        Status          = ISNULL(@Status, Status),
        UpdatedBy       = @UpdatedBy,
        UpdatedDate     = GETUTCDATE()
    WHERE Id = @PayrollId AND ISNULL(IsDeleted, 0) = 0;
END;
GO

-- ============================================================
-- sp_DeletePayroll - Soft delete
-- ============================================================
CREATE OR ALTER PROCEDURE sp_DeletePayroll
    @PayrollId UNIQUEIDENTIFIER,
    @UpdatedBy UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Payrolls
    SET IsDeleted = 1,
        UpdatedBy = @UpdatedBy,
        UpdatedDate = GETUTCDATE()
    WHERE Id = @PayrollId AND ISNULL(IsDeleted, 0) = 0;
END;
GO

-- ============================================================
-- sp_SalaryRegister - Salary register report
-- ============================================================
CREATE OR ALTER PROCEDURE sp_SalaryRegister
    @Month       INT = NULL,
    @Year        INT = NULL,
    @Department  NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.EmployeeCode,
        e.FirstName + ' ' + e.LastName AS EmployeeName,
        e.Department, e.Designation,
        e.BankName, e.BankAccount,
        p.GrossSalary,
        p.TaxDeduction,
        p.OtherDeductions,
        p.NetSalary,
        p.Status,
        p.ProcessedDate
    FROM Payrolls p
    INNER JOIN Employees e ON e.Id = p.EmployeeId
    INNER JOIN PayrollMonths pm ON pm.Id = p.PayrollMonthId
    WHERE ISNULL(p.IsDeleted, 0) = 0
      AND (@Month IS NULL OR pm.Month = @Month)
      AND (@Year IS NULL OR pm.Year = @Year)
      AND (@Department IS NULL OR e.Department = @Department)
    ORDER BY e.Department, e.EmployeeCode;
END;
GO

-- ============================================================
-- sp_EmployeeList - Paginated employee listing
-- ============================================================
CREATE OR ALTER PROCEDURE sp_EmployeeList
    @Search     NVARCHAR(100) = NULL,
    @Department NVARCHAR(100) = NULL,
    @PageNum    INT = 1,
    @PageSize   INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Skip INT = (@PageNum - 1) * @PageSize;

    SELECT
        e.Id, e.EmployeeCode,
        e.FirstName, e.LastName,
        e.FirstName + ' ' + e.LastName AS FullName,
        e.Email, e.Phone,
        e.Department, e.Designation,
        e.DateOfJoining, e.IsActive
    FROM Employees e
    WHERE ISNULL(e.IsDeleted, 0) = 0
      AND (@Search IS NULL
           OR e.FirstName LIKE '%' + @Search + '%'
           OR e.LastName LIKE '%' + @Search + '%'
           OR e.EmployeeCode LIKE '%' + @Search + '%'
           OR e.Email LIKE '%' + @Search + '%')
      AND (@Department IS NULL OR e.Department = @Department)
    ORDER BY e.EmployeeCode
    OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*)
    FROM Employees e
    WHERE ISNULL(e.IsDeleted, 0) = 0
      AND (@Search IS NULL
           OR e.FirstName LIKE '%' + @Search + '%'
           OR e.LastName LIKE '%' + @Search + '%'
           OR e.EmployeeCode LIKE '%' + @Search + '%'
           OR e.Email LIKE '%' + @Search + '%')
      AND (@Department IS NULL OR e.Department = @Department);
END;
GO
