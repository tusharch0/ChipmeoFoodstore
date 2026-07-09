using FoodstoreApi.Core.Entities;
using FoodstoreApi.Infrastructure.Data;
using FoodstoreApi.Usecase.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FoodstoreApi.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly StoreDbContext _context;

    public EmployeeRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.User)
            .Include(e => e.Role)
            .Include(e => e.Branch!)
                .ThenInclude(e => e.Organization)
            .ToListAsync(cancellationToken);
    }

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.User)
            .Include(e => e.Role)
            .Include(e => e.Branch!)
                .ThenInclude(e => e.Organization)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Employee?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .IgnoreQueryFilters()
            .Include(e => e.User)
            .Include(e => e.Role)
            .Include(e => e.Branch!)
                .ThenInclude(e => e.Organization)
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);
    }

    public async Task<Employee?> GetByEmployeeCodeAsync(string employeeCode, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.User)
            .Include(e => e.Role)
            .Include(e => e.Branch!)
                .ThenInclude(e => e.Organization)
            .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode, cancellationToken);
    }

    public async Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);
        return employee;
    }

    public async Task<bool> UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Update(employee);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees.FindAsync(new object[] { id }, cancellationToken);
        if (employee == null) return false;

        _context.Employees.Remove(employee);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}
