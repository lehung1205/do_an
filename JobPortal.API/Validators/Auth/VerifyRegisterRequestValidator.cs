using FluentValidation;
using JobPortal.API.DTOs.Auth;

namespace JobPortal.API.Validators.Auth;

public class VerifyRegisterRequestValidator : AbstractValidator<VerifyRegisterRequest>
{
    public VerifyRegisterRequestValidator()
    {
        RuleFor(x => x.RegistrationToken)
            .NotEmpty().WithMessage("Mã phiên đăng ký không hợp lệ.");

        RuleFor(x => x.OtpCode)
            .NotEmpty().WithMessage("Vui lòng nhập mã OTP.")
            .Matches(@"^\d{6}$").WithMessage("Mã OTP phải gồm 6 chữ số.");
    }
}
