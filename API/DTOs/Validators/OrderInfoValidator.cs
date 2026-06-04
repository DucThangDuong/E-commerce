using FastEndpoints;
using FluentValidation;

namespace API.DTOs.Validators;

public class CreatePaymentValidator : Validator<ReqCreatePayment>
{
    public CreatePaymentValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Danh sách sản phẩm không được để trống");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ColorId).GreaterThan(0).WithMessage("ColorId phải lớn hơn 0");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity phải lớn hơn 0");
        });

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống")
            .MaximumLength(100).WithMessage("Họ tên không được vượt quá 100 ký tự")
            .Matches(@"^[\p{L}\p{M}\s\.\-]+$").WithMessage("Họ tên chỉ được chứa chữ cái, dấu chấm, gạch ngang và khoảng trắng")
            .Must(XssProtection.IsCleanText).WithMessage("Họ tên chứa nội dung không hợp lệ (phát hiện mã độc)");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Địa chỉ không được để trống")
            .MaximumLength(500).WithMessage("Địa chỉ không được vượt quá 500 ký tự")
            .Must(XssProtection.IsCleanText).WithMessage("Địa chỉ chứa nội dung không hợp lệ (phát hiện mã độc)");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Số điện thoại không được để trống")
            .MinimumLength(10).WithMessage("Số điện thoại không được ít hơn 10")
            .MaximumLength(10).WithMessage("Số điện thoại không được vượt quá 10 ký tự")
            .Matches(@"^[\d\+\-\s]*$").WithMessage("Số điện thoại chỉ được chứa số");

        RuleFor(x => x.TypePayment)
            .InclusiveBetween(0, 1).WithMessage("TypePayment chỉ được là 0 hoặc 1");
    }
}
