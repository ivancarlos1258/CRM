using CRM.Domain.Entities;
using CRM.Domain.Repositories;
using CRM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly CrmDbContext _context;

    public CustomerRepository(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByCpfAsync(string cpf, CancellationToken cancellationToken = default)
    {
        var cleanCpf = new string(cpf.Where(char.IsDigit).ToArray());
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Cpf != null && c.Cpf.Value == cleanCpf, cancellationToken);
    }

    public async Task<Customer?> GetByCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        var cleanCnpj = new string(cnpj.Where(char.IsDigit).ToArray());
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Cnpj != null && c.Cnpj.Value == cleanCnpj, cancellationToken);
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var lowerEmail = email.ToLowerInvariant();
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Email.Value == lowerEmail, cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCpfAsync(string cpf, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var cleanCpf = new string(cpf.Where(char.IsDigit).ToArray());
        var query = _context.Customers.Where(c => c.Cpf != null && c.Cpf.Value == cleanCpf);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCnpjAsync(string cnpj, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var cleanCnpj = new string(cnpj.Where(char.IsDigit).ToArray());
        var query = _context.Customers.Where(c => c.Cnpj != null && c.Cnpj.Value == cleanCnpj);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var lowerEmail = email.ToLowerInvariant();
        var query = _context.Customers.Where(c => c.Email.Value == lowerEmail);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}
