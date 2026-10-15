using MediatR;

namespace Products.Application.Features.Products.Commands.DeleteProduct;

/// <summary>Returns <c>true</c> when the product existed and was deleted.</summary>
public sealed record DeleteProductCommand(int Id) : IRequest<bool>;
