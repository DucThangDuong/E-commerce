using FastEndpoints;
using FluentValidation;

namespace API.DTOs.Validators;

public class RegisterValidator : Validator<ReqRegisterDto>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Email không đúng định dạng")
            .MaximumLength(255).WithMessage("Email không được vượt quá 255 ký tự")

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống")
            .MinimumLength(6).WithMessage("Mật khẩu phải dài ít nhất 6 ký tự")
            .MaximumLength(100).WithMessage("Mật khẩu không được vượt quá 100 ký tự");

        RuleFor(x => x.Fullname)
            .NotEmpty().WithMessage("Họ tên không được để trống")
            .MaximumLength(100).WithMessage("Họ tên không được vượt quá 100 ký tự")
            .Matches(@"^[\p{L}\p{M}\s\.\-]+$").WithMessage("Họ tên chỉ được chứa chữ cái, dấu chấm, gạch ngang và khoảng trắng")
            .Must(XssProtection.IsCleanText).WithMessage("Họ tên chứa nội dung không hợp lệ ");
    }
}
