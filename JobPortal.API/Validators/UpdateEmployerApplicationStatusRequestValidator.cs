using FluentValidation;
using JobPortal.API.DTOs;

namespace JobPortal.API.Validators;

public class UpdateEmployerApplicationStatusRequestValidator : AbstractValidator<UpdateEmployerApplicationStatusRequest>
{
    private static readonly string[] Allowed =
        ["reviewed", "accepted", "rejected"];

    public UpdateEmployerApplicationStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Allowed.Contains(s.Trim().ToLowerInvariant()))
            .WithMessage($"Status must be one of: {string.Join(", ", Allowed)}.");
    }
}
