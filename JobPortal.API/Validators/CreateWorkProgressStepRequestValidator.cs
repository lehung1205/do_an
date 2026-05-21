using FluentValidation;
using JobPortal.API.DTOs;
using JobPortal.API.Helpers;

namespace JobPortal.API.Validators;

public class CreateWorkProgressStepRequestValidator : AbstractValidator<CreateWorkProgressStepRequest>
{
    public CreateWorkProgressStepRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Trạng thái tiến độ là bắt buộc.")
            .Must(WorkProgressCatalog.IsValidStatus)
            .WithMessage("Trạng thái tiến độ không hợp lệ.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Ghi chú không được vượt quá 2000 ký tự.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
