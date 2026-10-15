using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Features.Products.Commands.UpdateProduct;

/// <summary>
/// <see cref="Id"/> is always supplied from the route so a request body can never override it.
/// </summary>
public sealed record UpdateProductCommand(int Id, string Name, decimal Price) : IRequest<ProductDto?>;
