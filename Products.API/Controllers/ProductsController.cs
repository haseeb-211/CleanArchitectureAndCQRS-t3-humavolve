using MediatR;
using Microsoft.AspNetCore.Mvc;
using Products.Application.DTOs;
using Products.Application.Features.Products.Commands.CreateProduct;
using Products.Application.Features.Products.Commands.DeleteProduct;
using Products.Application.Features.Products.Commands.UpdateProduct;
using Products.Application.Features.Products.Queries.GetAllProducts;
using Products.Application.Features.Products.Queries.GetProductById;

namespace Products.API.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns every product, or an empty collection when none exist.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await mediator.Send(new GetAllProductsQuery(), cancellationToken);
        return Ok(products);
    }

    /// <summary>Returns a single product by its identifier.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await mediator.Send(new GetProductByIdQuery(id), cancellationToken);

        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>Creates a product and returns it with a Location header.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductDto request,
        CancellationToken cancellationToken)
    {
        var created = await mediator.Send(new CreateProductCommand(request.Name, request.Price), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing product. The route identifier is authoritative.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Update(
        int id,
        [FromBody] UpdateProductDto request,
        CancellationToken cancellationToken)
    {
        var updated = await mediator.Send(
            new UpdateProductCommand(id, request.Name, request.Price),
            cancellationToken);

        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Deletes an existing product.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteProductCommand(id), cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
