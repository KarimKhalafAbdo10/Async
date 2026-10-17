using MediatR;
using Microsoft.AspNetCore.Mvc;
using Products.Application.Products;
using Products.Application.Products.Commands.CreateProduct;
using Products.Application.Products.Queries.GetProductById;
using Products.Application.Products.Queries.GetProducts;

namespace Products.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll(CancellationToken ct)
        => Ok(await sender.Send(new GetProductsQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetProductByIdQuery(id), ct));

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductCommand command, CancellationToken ct)
    {
        var created = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
