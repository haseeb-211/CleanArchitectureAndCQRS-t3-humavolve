using MediatR;
using Microsoft.EntityFrameworkCore;
using Products.Application.Interfaces;

namespace Products.Application.Features.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(existing => existing.Id == request.Id, cancellationToken);

        if (product is null)
        {
            return false;
        }

        dbContext.Products.Remove(product);

        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
