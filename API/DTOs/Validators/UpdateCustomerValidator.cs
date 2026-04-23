using FastEndpoints;
using FluentValidation;

namespace API.DTOs.Validators;

public class UpdateCustomerValidator : Validator<ReqUpdateCustomerProfile>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Tên không được vượt quá 100 ký tự")
            .Matches(@"^[\p{L}\p{M}\s\.\-]+$").WithMessage("Tên chỉ được chứa chữ cái, dấu chấm, gạch ngang và khoảng trắng")
            .Must(XssProtection.IsCleanText).WithMessage("Tên chứa nội dung không hợp lệ (phát hiện mã độc)")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Số điện thoại không được vượt quá 20 ký tự")
            .Matches(@"^[\d\+\-\s]*$").WithMessage("Số điện thoại chỉ được chứa số, dấu +, - và khoảng trắng")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.Address)
            .MaximumLength(500).WithMessage("Địa chỉ không được vượt quá 500 ký tự")
            .Must(XssProtection.IsCleanText).WithMessage("Địa chỉ chứa nội dung không hợp lệ (phát hiện mã độc)")
            .When(x => !string.IsNullOrEmpty(x.Address));
    }
}
