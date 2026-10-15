namespace Products.Application.DTOs;

/// <summary>
/// Request payload used when updating a product. The identifier always comes from the route.
/// </summary>
public sealed class UpdateProductDto
{
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
}
