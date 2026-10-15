using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Features.Products.Queries.GetAllProducts;

public sealed record GetAllProductsQuery : IRequest<IReadOnlyList<ProductDto>>;
