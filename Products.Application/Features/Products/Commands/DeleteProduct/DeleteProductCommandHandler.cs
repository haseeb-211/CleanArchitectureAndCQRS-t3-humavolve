using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Products.Application.Interfaces;

namespace Products.Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(
    IApplicationDbContext dbContext,
    ILogger<DeleteProductCommandHandler> logger)
    : IRequestHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(existing => existing.Id == request.Id, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Product {ProductId} was not found while deleting", request.Id);
            return false;
        }

        dbContext.Products.Remove(product);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted product {ProductId}", product.Id);

        return true;
    }
}
