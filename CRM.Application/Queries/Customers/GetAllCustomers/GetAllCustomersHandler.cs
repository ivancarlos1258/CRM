using CRM.Application.Common;
using CRM.Application.DTOs;
using CRM.Domain.Repositories;

namespace CRM.Application.Queries.Customers.GetAllCustomers;

public class GetAllCustomersHandler : IQueryHandler<GetAllCustomersQuery, Result<IEnumerable<CustomerDto>>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetAllCustomersHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<IEnumerable<CustomerDto>>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = request.OnlyActive
            ? await _customerRepository.GetActiveAsync(cancellationToken)
            : await _customerRepository.GetAllAsync(cancellationToken);

        var dtos = customers.Select(customer => new CustomerDto(
            customer.Id,
            customer.PersonType,
            customer.Name,
            customer.Cpf?.Value,
            customer.Cnpj?.Value,
            customer.BirthDate,
            customer.FoundationDate,
            customer.Phone.Value,
            customer.Email.Value,
            new AddressDto(
                customer.Address.ZipCode,
                customer.Address.Street,
                customer.Address.Number,
                customer.Address.Complement,
                customer.Address.Neighborhood,
                customer.Address.City,
                customer.Address.State),
            customer.StateRegistration,
            customer.IsStateRegistrationExempt,
            customer.IsActive,
            customer.CreatedAt,
            customer.UpdatedAt));

        return Result<IEnumerable<CustomerDto>>.Success(dtos);
    }
}
