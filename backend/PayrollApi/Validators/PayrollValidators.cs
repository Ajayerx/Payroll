using FluentValidation;
using PayrollApi.Models.DTOs;

namespace PayrollApi.Validators;

public class ProcessPayrollRequestValidator : AbstractValidator<ProcessPayrollRequest>
{
    public ProcessPayrollRequestValidator()
    {
        RuleFor(x => x.EmployeeIds).NotEmpty().WithMessage("At least one employee must be selected");
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).InclusiveBetween(2020, 2030);
    }
}

public class UpdatePayrollRequestValidator : AbstractValidator<UpdatePayrollRequest>
{
    public UpdatePayrollRequestValidator()
    {
        When(x => x.TaxDeduction.HasValue, () =>
        {
            RuleFor(x => x.TaxDeduction!.Value).GreaterThanOrEqualTo(0);
        });
        When(x => x.OtherDeductions.HasValue, () =>
        {
            RuleFor(x => x.OtherDeductions!.Value).GreaterThanOrEqualTo(0);
        });
    }
}
