using MediatR;
using Microsoft.Extensions.Logging;
using Products.Application.Abstractions;
using Products.Application.Common.Exceptions;

namespace Products.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler(IProductRepository repository, ILogger<GetProductByIdQueryHandler> logger)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await repository.GetByIdAsync(request.Id, ct);
        if (product is null)
        {
            logger.LogWarning("Product {ProductId} was not found", request.Id);
            throw new NotFoundException($"Product {request.Id} was not found.");
        }

        return ProductDto.From(product);
    }
}
