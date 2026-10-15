using MediatR;
using Microsoft.EntityFrameworkCore;
using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.Application.Features.Products.Queries.GetAllProducts;

public sealed class GetAllProductsQueryHandler(IApplicationDbContext dbContext)
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

        return products
            .Select(product => new ProductDto(product.Id, product.Name, product.Price))
            .ToList();
    }
}
