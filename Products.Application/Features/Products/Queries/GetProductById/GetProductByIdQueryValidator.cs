using FluentValidation;

namespace Products.Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(query => query.Id)
            .GreaterThan(0);
    }
}
