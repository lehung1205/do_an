using FluentValidation;
using JobPortal.API.DTOs;

namespace JobPortal.API.Validators;

public class CreateApplicationRequestValidator : AbstractValidator<CreateApplicationRequest>
{
    public CreateApplicationRequestValidator()
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0).WithMessage("Job id is required.");

        RuleFor(x => x.ResumeId)
            .GreaterThan(0).WithMessage("Resume id is required.");
    }
}
