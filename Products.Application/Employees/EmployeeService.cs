using FluentValidation;
using Microsoft.Extensions.Logging;
using Products.Application.Abstractions;
using Products.Application.Common.Exceptions;
using Products.Domain.Entities;

namespace Products.Application.Employees;

public class EmployeeService(
    IEmployeeRepository repository,
    IValidator<EmployeeRequest> validator,
    ILogger<EmployeeService> logger) : IEmployeeService
{
    public async Task<List<EmployeeDto>> GetAllAsync(CancellationToken ct)
    {
        var employees = await repository.GetAllAsync(ct);
        logger.LogInformation("Retrieved {Count} employees", employees.Count);
        return employees.Select(EmployeeDto.From).ToList();
    }

    public async Task<EmployeeDto> GetAsync(int id, CancellationToken ct)
        => EmployeeDto.From(await FindAsync(id, ct));

    public async Task<EmployeeDto> CreateAsync(EmployeeRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        var employee = new Employee
        {
            Name = request.Name, Position = request.Position,
            Department = request.Department, Salary = request.Salary
        };
        await repository.AddAsync(employee, ct);
        logger.LogInformation("Employee {EmployeeId} created: {Name}", employee.Id, employee.Name);
        return EmployeeDto.From(employee);
    }

    public async Task<EmployeeDto> UpdateAsync(int id, EmployeeRequest request, CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);
        var employee = await FindAsync(id, ct);
        employee.Name = request.Name;
        employee.Position = request.Position;
        employee.Department = request.Department;
        employee.Salary = request.Salary;
        await repository.UpdateAsync(employee, ct);
        logger.LogInformation("Employee {EmployeeId} updated", id);
        return EmployeeDto.From(employee);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        if (!await repository.DeleteAsync(id, ct))
            throw new NotFoundException($"Employee {id} was not found.");
        logger.LogInformation("Employee {EmployeeId} deleted", id);
    }

    private async Task<Employee> FindAsync(int id, CancellationToken ct)
        => await repository.GetByIdAsync(id, ct)
           ?? throw new NotFoundException($"Employee {id} was not found.");
}
