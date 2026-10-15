using FluentValidation;
using Products.Domain.Entities;

namespace Products.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0);

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(Product.MaxNameLength);

        RuleFor(command => command.Price)
            .GreaterThan(0);
    }
}
