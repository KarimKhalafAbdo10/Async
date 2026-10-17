using Microsoft.EntityFrameworkCore;
using Products.Application.Abstractions;
using Products.Domain.Entities;
using Products.Infrastructure.Persistence;

namespace Products.Infrastructure.Repositories;

public class EmployeeRepository(AppDbContext db) : IEmployeeRepository
{
    public Task<List<Employee>> GetAllAsync(CancellationToken ct)
        => db.Employees.AsNoTracking().OrderBy(e => e.Id).ToListAsync(ct);

    public Task<Employee?> GetByIdAsync(int id, CancellationToken ct)
        => db.Employees.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(Employee employee, CancellationToken ct)
    {
        db.Employees.Add(employee);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Employee employee, CancellationToken ct)
        => await db.SaveChangesAsync(ct);

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (employee is null) return false;
        db.Employees.Remove(employee);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
