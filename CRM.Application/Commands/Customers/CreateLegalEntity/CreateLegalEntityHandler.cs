using CRM.Application.Common;
using CRM.Application.DTOs;
using CRM.Domain.Entities;
using CRM.Domain.Repositories;
using CRM.Domain.ValueObjects;

namespace CRM.Application.Commands.Customers.CreateLegalEntity;

public class CreateLegalEntityHandler : ICommandHandler<CreateLegalEntityCommand, Result<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IEventStore _eventStore;

    public CreateLegalEntityHandler(ICustomerRepository customerRepository, IEventStore eventStore)
    {
        _customerRepository = customerRepository;
        _eventStore = eventStore;
    }

    public async Task<Result<CustomerDto>> Handle(CreateLegalEntityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await _customerRepository.ExistsByCnpjAsync(request.Cnpj, cancellationToken: cancellationToken))
                return Result<CustomerDto>.Failure("CNPJ já cadastrado");

            if (await _customerRepository.ExistsByEmailAsync(request.Email, cancellationToken: cancellationToken))
                return Result<CustomerDto>.Failure("Email já cadastrado");

            var address = Address.Create(
                request.Address.ZipCode,
                request.Address.Street,
                request.Address.Number,
                request.Address.Complement,
                request.Address.Neighborhood,
                request.Address.City,
                request.Address.State);

            var customer = Customer.CreateLegalEntity(
                request.Name,
                request.Cnpj,
                request.FoundationDate,
                request.Phone,
                request.Email,
                address,
                request.StateRegistration,
                request.IsStateRegistrationExempt);

            await _customerRepository.AddAsync(customer, cancellationToken);

            foreach (var domainEvent in customer.DomainEvents)
            {
                await _eventStore.SaveEventAsync(domainEvent, "system", cancellationToken);
            }

            customer.ClearDomainEvents();

            var dto = new CustomerDto(
                customer.Id,
                customer.PersonType,
                customer.Name,
                null,
                customer.Cnpj?.Value,
                null,
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
        catch (ArgumentException ex)
        {
            return Result<CustomerDto>.Failure(ex.Message);
        }
    }
}
