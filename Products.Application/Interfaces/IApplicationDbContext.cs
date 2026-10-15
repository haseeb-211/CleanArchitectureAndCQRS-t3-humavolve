using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;

namespace Products.Application.Interfaces;

/// <summary>
/// Persistence abstraction implemented by the Infrastructure project so that
/// Application never depends on a concrete database technology.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
