using Products.Domain.Entities;

namespace Products.Application.Products;

public record ProductDto(Guid Id, string Name, decimal Price)
{
    public static ProductDto From(Product p) => new(p.Id, p.Name, p.Price);
}
