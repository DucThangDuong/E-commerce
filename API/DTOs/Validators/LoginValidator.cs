using FastEndpoints;
using FluentValidation;

namespace API.DTOs.Validators;

public class LoginValidator : Validator<ReqLoginDTo>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Email không đúng định dạng");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống");
    }
}
public class LoginGoogle : Validator<ReqGoogleLoginDTO>
{
    public LoginGoogle()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Yêu Cầu TokenID ");
    }
}
