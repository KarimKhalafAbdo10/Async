using Products.Domain.Entities;

namespace Products.Application.Abstractions;

public interface IEmployeeRepository
{
    Task<List<Employee>> GetAllAsync(CancellationToken ct);
    Task<Employee?> GetByIdAsync(int id, CancellationToken ct);
    Task AddAsync(Employee employee, CancellationToken ct);
    Task UpdateAsync(Employee employee, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
