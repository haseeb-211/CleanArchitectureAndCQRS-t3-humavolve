using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    IApplicationDbContext dbContext,
    ILogger<UpdateProductCommandHandler> logger)
    : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    public async Task<ProductDto?> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(existing => existing.Id == request.Id, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Product {ProductId} was not found while updating", request.Id);
            return null;
        }

        product.Update(request.Name, request.Price);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Updated product {ProductId} with name {ProductName}",
            product.Id,
            product.Name);

        return new ProductDto(product.Id, product.Name, product.Price);
    }
}
