using FluentValidation;
using JobPortal.API.DTOs.Auth;

namespace JobPortal.API.Validators.Auth;

public class RevokeTokenRequestValidator : AbstractValidator<RevokeTokenRequest>
{
    public RevokeTokenRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");
    }
}
