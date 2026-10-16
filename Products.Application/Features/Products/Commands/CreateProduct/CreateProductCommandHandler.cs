using MediatR;
using Microsoft.Extensions.Logging;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Domain.Entities;

namespace Products.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    IApplicationDbContext dbContext,
    ILogger<CreateProductCommandHandler> logger)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Price);

        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Created product {ProductId} with name {ProductName}",
            product.Id,
            product.Name);

        return new ProductDto(product.Id, product.Name, product.Price);
    }
}
