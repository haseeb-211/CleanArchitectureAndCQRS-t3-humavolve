using MediatR;
using Microsoft.EntityFrameworkCore;
using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(existing => existing.Id == request.Id, cancellationToken);

        return product is null
            ? null
            : new ProductDto(product.Id, product.Name, product.Price);
    }
}
