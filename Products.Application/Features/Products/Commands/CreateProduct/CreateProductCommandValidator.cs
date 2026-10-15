using FluentValidation;
using Products.Domain.Entities;

namespace Products.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(Product.MaxNameLength);

        RuleFor(command => command.Price)
            .GreaterThan(0);
    }
}
