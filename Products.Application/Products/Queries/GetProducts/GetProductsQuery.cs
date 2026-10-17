using MediatR;
using Microsoft.Extensions.Logging;
using Products.Application.Abstractions;

namespace Products.Application.Products.Queries.GetProducts;

public record GetProductsQuery : IRequest<List<ProductDto>>;

public class GetProductsQueryHandler(IProductRepository repository, ILogger<GetProductsQueryHandler> logger)
    : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var products = await repository.GetAllAsync(ct);
        logger.LogInformation("Retrieved {Count} products", products.Count);
        return products.Select(ProductDto.From).ToList();
    }
}
