using FastEndpoints;
using FluentValidation;

namespace API.DTOs.Validators;

public class LoginValidator : Validator<ReqLoginDTo>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Email không đúng định dạng")
            .MaximumLength(255).WithMessage("Email không được vượt quá 255 ký tự");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống")
            .MaximumLength(100).WithMessage("Mật khẩu không được vượt quá 100 ký tự");
    }
}
public class LoginGoogle : Validator<ReqGoogleLoginDTO>
{
    public LoginGoogle()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Yêu Cầu TokenID")
            .MaximumLength(4096).WithMessage("TokenID vượt quá độ dài cho phép")
            .Must(XssProtection.IsCleanText).WithMessage("TokenID chứa nội dung không hợp lệ");
    }
}
