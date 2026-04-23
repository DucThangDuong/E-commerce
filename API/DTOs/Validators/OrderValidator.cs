using API.DTOs;
using FastEndpoints;
using FluentValidation;

namespace API.DTOs.Validators;

public class OrderValidator : Validator<ReqAddNewOrder>
{
    public OrderValidator()
    {
        RuleFor(x => x.Items)
            .NotNull().WithMessage("Danh sách sản phẩm không được null.")
            .NotEmpty().WithMessage("Danh sách sản phẩm phải có ít nhất 1 sản phẩm.");

        RuleForEach(x => x.Items).ChildRules(items =>
        {
            items.RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("ProductId phải lớn hơn 0.");
            
            items.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Số lượng mua phải lớn hơn 0.");
        });
    }
}
