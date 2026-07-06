using FluentValidation;
using PayrollApi.Models.DTOs;

namespace PayrollApi.Validators;

public class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequest>
{
    public CreateLeaveRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty().WithMessage("Employee is required");
        RuleFor(x => x.LeaveTypeId).NotEmpty().WithMessage("Leave type is required");
        RuleFor(x => x.FromDate).NotEmpty().WithMessage("From date is required");
        RuleFor(x => x.ToDate).NotEmpty().WithMessage("To date is required");
        RuleFor(x => x.FromDate).LessThanOrEqualTo(x => x.ToDate).WithMessage("From date must be on or before to date");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason is required").MaximumLength(500);
    }
}

public class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Department).MaximumLength(100);
        RuleFor(x => x.Designation).MaximumLength(100);
    }
}

public class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeRequestValidator()
    {
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
        RuleFor(x => x.Phone).MaximumLength(20);
    }
}
