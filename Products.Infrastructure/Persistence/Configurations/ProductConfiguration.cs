using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Domain.Entities;

namespace Products.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Id)
            .ValueGeneratedOnAdd();

        builder.Property(product => product.Name)
            .IsRequired()
            .HasMaxLength(Product.MaxNameLength);

        builder.Property(product => product.Price)
            .IsRequired()
            .HasPrecision(18, 2);
    }
}
