using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(string Name, decimal Price) : IRequest<ProductDto>;
