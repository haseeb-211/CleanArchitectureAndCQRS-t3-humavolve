using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.Application.Features.Products.Queries.GetAllProducts;

public sealed class GetAllProductsQueryHandler(
    IApplicationDbContext dbContext,
    ILogger<GetAllProductsQueryHandler> logger)
    : IRequestHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Id)
            .ToListAsync(cancellationToken);

        var result = products
            .Select(product => new ProductDto(product.Id, product.Name, product.Price))
            .ToList();

        logger.LogDebug("Retrieved {ProductCount} products", result.Count);

        return result;
    }
}
