using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler(
    IApplicationDbContext dbContext,
    ILogger<GetProductByIdQueryHandler> logger)
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(existing => existing.Id == request.Id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        logger.LogDebug("Retrieved product {ProductId} ({ProductName})", product.Id, product.Name);

        return new ProductDto(product.Id, product.Name, product.Price);
    }
}
