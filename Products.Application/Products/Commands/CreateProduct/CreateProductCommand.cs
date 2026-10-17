using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Products.Application.Abstractions;
using Products.Domain.Entities;

namespace Products.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(string Name, decimal Price) : IRequest<ProductDto>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

public class CreateProductCommandHandler(IProductRepository repository, ILogger<CreateProductCommandHandler> logger)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var product = new Product(request.Name, request.Price);
        await repository.AddAsync(product, ct);
        logger.LogInformation("Product created with Id {ProductId}, Name {Name}, Price {Price}", product.Id, product.Name, product.Price);
        return ProductDto.From(product);
    }
}
