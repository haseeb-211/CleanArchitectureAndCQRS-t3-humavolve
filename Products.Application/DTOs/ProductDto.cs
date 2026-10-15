namespace Products.Application.DTOs;

/// <summary>
/// Response representation of a product.
/// </summary>
public sealed record ProductDto(int Id, string Name, decimal Price);
