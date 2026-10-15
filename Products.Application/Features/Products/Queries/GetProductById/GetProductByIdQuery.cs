using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Features.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;
