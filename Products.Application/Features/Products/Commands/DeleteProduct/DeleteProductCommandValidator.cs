using FluentValidation;

namespace Products.Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0);
    }
}
