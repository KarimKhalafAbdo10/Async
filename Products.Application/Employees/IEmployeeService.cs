namespace Products.Application.Employees;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetAllAsync(CancellationToken ct);
    Task<EmployeeDto> GetAsync(int id, CancellationToken ct);
    Task<EmployeeDto> CreateAsync(EmployeeRequest request, CancellationToken ct);
    Task<EmployeeDto> UpdateAsync(int id, EmployeeRequest request, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
