using FastEndpoints;
using FluentValidation;

namespace API.DTOs.Validators;

public class CartValidator : Validator<ReqCreateCartDto>
{
    public CartValidator()
    {

        RuleFor(x => x.product_id)
            .GreaterThan(0).WithMessage("product_id phải lớn hơn 0");

        RuleFor(x => x.quantity)
            .GreaterThan(0).WithMessage("quantity phải lớn hơn 0");
    }
}
