using FluentValidation;
using JobPortal.API.DTOs;

namespace JobPortal.API.Validators;

public class CreateResumeRequestValidator : AbstractValidator<CreateResumeRequest>
{
    public CreateResumeRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters.");

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("URL is required.")
            .MaximumLength(500).WithMessage("URL must not exceed 500 characters.")
            .Must(url => Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) &&
                         (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("URL must be a valid http or https link.");
    }
}
