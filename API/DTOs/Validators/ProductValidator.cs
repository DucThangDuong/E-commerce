using FastEndpoints;
using FluentValidation;

namespace API.DTOs.Validators;

public class ProductValidator : Validator<ReqCreateProductDto>
{
    public ProductValidator()
    {
        RuleFor(x => x.category_id)
            .GreaterThan(0).WithMessage("category_id phải lớn hơn 0");
        RuleFor(x => x.brand_id)
            .GreaterThan(0).WithMessage("brand_id phải lớn hơn 0");

        RuleFor(x => x.name)
            .NotEmpty().WithMessage("Tên sản phẩm không được để trống")
            .MaximumLength(255).WithMessage("Tên sản phẩm không được vượt quá 255 ký tự")
            .Must(XssProtection.IsCleanText).WithMessage("Tên sản phẩm chứa nội dung không hợp lệ (phát hiện mã độc)");

        RuleFor(x => x.description)
            .MaximumLength(5000).WithMessage("Mô tả không được vượt quá 5000 ký tự")
            .Must(XssProtection.IsCleanDescription).WithMessage("Mô tả chứa nội dung không hợp lệ (phát hiện mã độc)")
            .When(x => !string.IsNullOrEmpty(x.description));

        RuleFor(x => x.base_price)
            .GreaterThan(0).WithMessage("Giá sản phẩm phải lớn hơn 0");
        RuleFor(x => x.stock_quantity)
            .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0");
    }
}
