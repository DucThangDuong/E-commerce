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
            .MaximumLength(255).WithMessage("Email không được vượt quá 255 ký tự");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự.")
            .MaximumLength(64).WithMessage("Mật khẩu không được vượt quá 64 ký tự.")
            .Matches("[A-Z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ cái in hoa.")
            .Matches("[a-z]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ cái in thường.")
            .Matches("[0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 chữ số.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt (VD: @, #, $, ...).");

        RuleFor(x => x.Fullname)
            .NotEmpty().WithMessage("Họ tên không được để trống")
            .MaximumLength(100).WithMessage("Họ tên không được vượt quá 100 ký tự")
            .Matches(@"^[\p{L}\p{M}\s\.\-]+$").WithMessage("Họ tên chỉ được chứa chữ cái, dấu chấm, gạch ngang và khoảng trắng")
            .Must(XssProtection.IsCleanText).WithMessage("Họ tên chứa nội dung không hợp lệ ");
    }
}
