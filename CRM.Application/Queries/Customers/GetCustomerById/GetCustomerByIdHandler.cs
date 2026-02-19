using CRM.Application.Common;
using CRM.Application.DTOs;
using CRM.Domain.Repositories;

namespace CRM.Application.Queries.Customers.GetCustomerById;

public class GetCustomerByIdHandler : IQueryHandler<GetCustomerByIdQuery, Result<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdHandler(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<CustomerDto>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (customer == null)
            return Result<CustomerDto>.Failure("Cliente não encontrado");

        var dto = new CustomerDto(
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
            customer.UpdatedAt);

        return Result<CustomerDto>.Success(dto);
    }
}
