using MediatR;
using Microsoft.EntityFrameworkCore;
using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.Application.Features.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    public async Task<ProductDto?> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .FirstOrDefaultAsync(existing => existing.Id == request.Id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        product.Update(request.Name, request.Price);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ProductDto(product.Id, product.Name, product.Price);
    }
}
