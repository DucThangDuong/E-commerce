using FastEndpoints;
using FluentValidation;

namespace API.DTOs.Validators;

public class BrandValidator : Validator<ReqCreateBrandDto>
{
    public BrandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên thương hiệu không được để trống")
            .MaximumLength(100).WithMessage("Tên thương hiệu không được vượt quá 100 ký tự")
            .Must(XssProtection.IsCleanText).WithMessage("Tên thương hiệu chứa nội dung không hợp lệ (phát hiện mã độc)");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Mô tả không được vượt quá 500 ký tự")
            .Must(XssProtection.IsCleanDescription).WithMessage("Mô tả chứa nội dung không hợp lệ (phát hiện mã độc)")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.LogoUrl)
            .MaximumLength(2048).WithMessage("URL logo không được vượt quá 2048 ký tự")
            .Must(XssProtection.IsCleanUrl).WithMessage("URL logo không hợp lệ (chỉ chấp nhận http/https)")
            .When(x => !string.IsNullOrEmpty(x.LogoUrl));
    }
}
