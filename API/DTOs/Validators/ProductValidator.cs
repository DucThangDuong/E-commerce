using FastEndpoints;
using FluentValidation;

namespace API.DTOs.Validators;

public class ProductValidator : Validator<ReqCreateProductDto>
{
    public ProductValidator()
    {
        RuleFor(x => x.category_id)
            .GreaterThan(0).WithMessage("category_id phải lớn hơn 0");
        RuleFor(x=>x.brand_id)
            .GreaterThan(0).WithMessage("brand_id phải lớn hơn 0");

        RuleFor(x => x.name)
            .NotEmpty().WithMessage("Tên sản phẩm không được để trống");

        RuleFor(x => x.base_price)
            .GreaterThan(0).WithMessage("Giá sản phẩm phải lớn hơn 0");
        RuleFor(x => x.stock_quantity)
            .NotEmpty().GreaterThan(0).WithMessage("Số lượng phải lơns hơn 0");
        RuleFor(x=>x.brand_id)
            .GreaterThan(0).WithMessage("brand_id phải lớn hơn 0");
    }
}
