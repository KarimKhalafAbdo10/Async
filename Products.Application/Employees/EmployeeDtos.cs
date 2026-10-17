using FluentValidation;
using Products.Domain.Entities;

namespace Products.Application.Employees;

public record EmployeeDto(int Id, string Name, string Position, string Department, decimal Salary)
{
    public static EmployeeDto From(Employee e) => new(e.Id, e.Name, e.Position, e.Department, e.Salary);
}

public record EmployeeRequest(string Name, string Position, string Department, decimal Salary);

public class EmployeeRequestValidator : AbstractValidator<EmployeeRequest>
{
    public EmployeeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Position).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Department).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Salary).GreaterThan(0);
    }
}
