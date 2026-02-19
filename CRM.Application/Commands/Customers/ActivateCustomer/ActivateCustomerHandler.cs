using CRM.Application.Common;
using CRM.Application.DTOs;
using CRM.Domain.Repositories;

namespace CRM.Application.Commands.Customers.ActivateCustomer;

public class ActivateCustomerHandler : ICommandHandler<ActivateCustomerCommand, Result<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IEventStore _eventStore;

    public ActivateCustomerHandler(ICustomerRepository customerRepository, IEventStore eventStore)
    {
        _customerRepository = customerRepository;
        _eventStore = eventStore;
    }

    public async Task<Result<CustomerDto>> Handle(ActivateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (customer == null)
            return Result<CustomerDto>.Failure("Cliente não encontrado");

        if (customer.IsActive)
            return Result<CustomerDto>.Failure("Cliente já está ativo");

        customer.Activate();

        await _customerRepository.UpdateAsync(customer, cancellationToken);

        foreach (var domainEvent in customer.DomainEvents)
        {
            await _eventStore.SaveEventAsync(domainEvent, "system", cancellationToken);
        }

        customer.ClearDomainEvents();

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
