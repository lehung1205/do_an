using FluentValidation;
using JobPortal.API.DTOs.Auth;

namespace JobPortal.API.Validators.Auth;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(255).WithMessage("Name must not exceed 255 characters.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.ProfileImage)
            .MaximumLength(500).WithMessage("Profile image URL must not exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ProfileImage));

        RuleFor(x => x.ProfileImage)
            .Must(url => Uri.TryCreate(url!.Trim(), UriKind.Absolute, out var uri) &&
                         (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Profile image must be a valid http or https URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.ProfileImage));

        RuleFor(x => x.Gender)
            .InclusiveBetween((byte)0, (byte)2).WithMessage("Gender must be 0, 1, or 2.")
            .When(x => x.Gender.HasValue);

        RuleFor(x => x.Description).MaximumLength(65535).When(x => !string.IsNullOrWhiteSpace(x.Description));
        RuleFor(x => x.PermanentAddress).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.PermanentAddress));
        RuleFor(x => x.TemporaryAddress).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.TemporaryAddress));
        RuleFor(x => x.IdCard).MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.IdCard));
        RuleFor(x => x.IdCardIssueDate).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.IdCardIssueDate));
        RuleFor(x => x.IdCardIssuePlace).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.IdCardIssuePlace));
        RuleFor(x => x.BankName).MaximumLength(255).When(x => !string.IsNullOrWhiteSpace(x.BankName));
        RuleFor(x => x.BankAccountNumber).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.BankAccountNumber));
    }
}
