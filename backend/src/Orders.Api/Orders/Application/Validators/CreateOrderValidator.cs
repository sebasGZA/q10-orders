using FluentValidation;
using Orders.Api.Orders.Application.Dtos;

namespace Orders.Api.Orders.Application.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrder>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.ClientName)
            .NotEmpty().WithMessage("ClientName is required")
            .MaximumLength(50).WithMessage("The max character title size is 50");

        RuleFor(x => x.Sku).NotEmpty().WithMessage("Sku is required");

        RuleFor(x => x.Quantity)
            .NotNull().WithMessage("Quantity is required")
            .GreaterThanOrEqualTo(1).WithMessage("The min quantity allowed is 1");
    }
}