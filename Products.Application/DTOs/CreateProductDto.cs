namespace Products.Application.DTOs;

/// <summary>
/// Request payload used when creating a product.
/// </summary>
public sealed class CreateProductDto
{
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
}
