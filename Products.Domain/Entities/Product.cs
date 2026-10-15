using Products.Domain.Exceptions;

namespace Products.Domain.Entities;

/// <summary>
/// A product sold by the store.
/// </summary>
public class Product
{
    public const int MaxNameLength = 100;

    /// <summary>Database generated primary key.</summary>
    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public decimal Price { get; private set; }

    /// <summary>Used by Entity Framework Core to materialize instances.</summary>
    private Product()
    {
    }

    public static Product Create(string name, decimal price)
    {
        var product = new Product();
        product.Update(name, price);
        return product;
    }

    /// <summary>
    /// Applies new details to the product while enforcing the aggregate invariants.
    /// </summary>
    public void Update(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name must not be null, empty or whitespace.");
        }

        if (name.Length > MaxNameLength)
        {
            throw new DomainException($"Product name must not exceed {MaxNameLength} characters.");
        }

        if (price <= 0)
        {
            throw new DomainException("Product price must be greater than zero.");
        }

        Name = name;
        Price = price;
    }
}
