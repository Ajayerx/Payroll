namespace PayrollApi.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string HRManager = "HRManager";
    public const string Employee = "Employee";

    public static readonly string[] All = { Admin, HRManager, Employee };
    public static readonly string[] AdminAndHR = { Admin, HRManager };
}

public static class ClaimKeys
{
    public const string UserId = "userId";
    public const string Role = "role";
    public const string UserName = "userName";
    public const string UserEmail = "userEmail";
}

public static class EntityNames
{
    public const string Employee = "Employee";
    public const string Payroll = "Payroll";
    public const string SalaryComponent = "SalaryComponent";
    public const string Deduction = "Deduction";
    public const string User = "User";
    public const string LeaveRequest = "LeaveRequest";
    public const string CompanySetting = "CompanySetting";
}

public static class PayrollStatuses
{
    public const string Draft = "Draft";
    public const string Processed = "Processed";
    public const string Paid = "Paid";
}

public static class LeaveRequestStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}
