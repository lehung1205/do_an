using FluentValidation;
using JobPortal.API.DTOs.Auth;

namespace JobPortal.API.Validators.Auth;

public class ResendRegisterOtpRequestValidator : AbstractValidator<ResendRegisterOtpRequest>
{
    public ResendRegisterOtpRequestValidator()
    {
        RuleFor(x => x.RegistrationToken)
            .NotEmpty().WithMessage("Mã phiên đăng ký không hợp lệ.");
    }
}
