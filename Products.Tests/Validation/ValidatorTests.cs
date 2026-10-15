using Products.Application.Features.Products.Commands.CreateProduct;
using Products.Application.Features.Products.Commands.DeleteProduct;
using Products.Application.Features.Products.Commands.UpdateProduct;
using Products.Application.Features.Products.Queries.GetProductById;

namespace Products.Tests.Validation;

public sealed class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_ProducesNoErrors()
    {
        var result = _validator.Validate(new CreateProductCommand("Laptop", 25000.00m));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Name_AtMaximumLength_IsAccepted()
    {
        var result = _validator.Validate(new CreateProductCommand(new string('a', 100), 1m));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_Whitespace_IsRejected(string name)
    {
        var result = _validator.Validate(new CreateProductCommand(name, 1m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Fact]
    public void Name_Null_IsRejected()
    {
        var result = _validator.Validate(new CreateProductCommand(null!, 1m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Fact]
    public void Name_ExceedingMaximumLength_IsRejected()
    {
        var result = _validator.Validate(new CreateProductCommand(new string('a', 101), 1m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateProductCommand.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Price_NotGreaterThanZero_IsRejected(int price)
    {
        var result = _validator.Validate(new CreateProductCommand("Laptop", price));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateProductCommand.Price));
    }

    [Fact]
    public void Price_GreaterThanZero_IsAccepted()
    {
        var result = _validator.Validate(new CreateProductCommand("Laptop", 0.01m));

        Assert.True(result.IsValid);
    }
}

public sealed class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_ProducesNoErrors()
    {
        var result = _validator.Validate(new UpdateProductCommand(1, "Updated Laptop", 27000.00m));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Id_NotGreaterThanZero_IsRejected(int id)
    {
        var result = _validator.Validate(new UpdateProductCommand(id, "Updated Laptop", 1m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateProductCommand.Id));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_Whitespace_IsRejected(string name)
    {
        var result = _validator.Validate(new UpdateProductCommand(1, name, 1m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateProductCommand.Name));
    }

    [Fact]
    public void Name_ExceedingMaximumLength_IsRejected()
    {
        var result = _validator.Validate(new UpdateProductCommand(1, new string('a', 101), 1m));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateProductCommand.Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Price_NotGreaterThanZero_IsRejected(int price)
    {
        var result = _validator.Validate(new UpdateProductCommand(1, "Updated Laptop", price));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateProductCommand.Price));
    }
}

public sealed class DeleteProductCommandValidatorTests
{
    private readonly DeleteProductCommandValidator _validator = new();

    [Fact]
    public void ValidId_ProducesNoErrors()
    {
        var result = _validator.Validate(new DeleteProductCommand(1));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Id_NotGreaterThanZero_IsRejected(int id)
    {
        var result = _validator.Validate(new DeleteProductCommand(id));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(DeleteProductCommand.Id));
    }
}

public sealed class GetProductByIdQueryValidatorTests
{
    private readonly GetProductByIdQueryValidator _validator = new();

    [Fact]
    public void ValidId_ProducesNoErrors()
    {
        var result = _validator.Validate(new GetProductByIdQuery(42));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-7)]
    public void Id_NotGreaterThanZero_IsRejected(int id)
    {
        var result = _validator.Validate(new GetProductByIdQuery(id));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(GetProductByIdQuery.Id));
    }
}
