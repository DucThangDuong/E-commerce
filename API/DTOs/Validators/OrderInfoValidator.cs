using FastEndpoints;
using FluentValidation;

namespace API.DTOs.Validators;

public class OrderInfoValidator : Validator<ReqOrderInfo>
{
    public OrderInfoValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("OrderId phải lớn hơn 0");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount phải lớn hơn 0");

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
            .MaximumLength(10).WithMessage("Số điện thoại không được vượt quá 10 ký tự")
            .Matches(@"^[\d\+\-\s]*$").WithMessage("Số điện thoại chỉ được chứa số");

        RuleFor(x => x.OrderDescription)
            .MaximumLength(500).WithMessage("Mô tả đơn hàng không được vượt quá 500 ký tự")
            .Must(XssProtection.IsCleanDescription).WithMessage("Mô tả đơn hàng chứa nội dung không hợp lệ (phát hiện mã độc)")
            .When(x => !string.IsNullOrEmpty(x.OrderDescription));

        RuleFor(x => x.TypePayment)
            .InclusiveBetween(0, 1).WithMessage("TypePayment chỉ được là 0 hoặc 1");
    }
}
