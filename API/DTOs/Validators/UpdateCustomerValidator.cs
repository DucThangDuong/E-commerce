using FastEndpoints;
using FluentValidation;

namespace API.DTOs.Validators;

public class UpdateCustomerNameValidator : Validator<ReqUpdateCustomerName>
{
    public UpdateCustomerNameValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên không được để trống")
            .MaximumLength(100).WithMessage("Tên không được vượt quá 100 ký tự")
            .Matches(@"^[\p{L}\p{M}\s\.\-]+$").WithMessage("Tên chỉ được chứa chữ cái, dấu chấm, gạch ngang và khoảng trắng")
            .Must(XssProtection.IsCleanText).WithMessage("Tên chứa nội dung không hợp lệ (phát hiện mã độc)");
    }
}

public class UpdateCustomerPhoneValidator : Validator<ReqUpdateCustomerPhone>
{
    public UpdateCustomerPhoneValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Số điện thoại không được để trống")
            .MaximumLength(20).WithMessage("Số điện thoại không được vượt quá 20 ký tự")
            .Matches(@"^[\d\+\-\s]*$").WithMessage("Số điện thoại chỉ được chứa số, dấu +, - và khoảng trắng");
    }
}

public class UpdateCustomerAddressValidator : Validator<ReqUpdateCustomerAddress>
{
    public UpdateCustomerAddressValidator()
    {
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Địa chỉ không được để trống")
            .MaximumLength(500).WithMessage("Địa chỉ không được vượt quá 500 ký tự")
            .Must(XssProtection.IsCleanText).WithMessage("Địa chỉ chứa nội dung không hợp lệ (phát hiện mã độc)");
    }
}

public class UpdateCustomerPasswordValidator : Validator<ReqUpdateCustomerPassword>
{
    public UpdateCustomerPasswordValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("Mật khẩu cũ không được để trống");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Mật khẩu mới không được để trống")
            .MinimumLength(6).WithMessage("Mật khẩu mới phải có ít nhất 6 ký tự");
    }
}
