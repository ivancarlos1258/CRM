using CRM.Application.Common;
using CRM.Application.DTOs;
using CRM.Domain.Entities;
using CRM.Domain.Repositories;
using CRM.Domain.ValueObjects;

namespace CRM.Application.Commands.Customers.CreateNaturalPerson;

public class CreateNaturalPersonHandler : ICommandHandler<CreateNaturalPersonCommand, Result<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IEventStore _eventStore;

    public CreateNaturalPersonHandler(ICustomerRepository customerRepository, IEventStore eventStore)
    {
        _customerRepository = customerRepository;
        _eventStore = eventStore;
    }

    public async Task<Result<CustomerDto>> Handle(CreateNaturalPersonCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await _customerRepository.ExistsByCpfAsync(request.Cpf, cancellationToken: cancellationToken))
                return Result<CustomerDto>.Failure("CPF já cadastrado");

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

            var customer = Customer.CreateNaturalPerson(
                request.Name,
                request.Cpf,
                request.BirthDate,
                request.Phone,
                request.Email,
                address);

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
                customer.Cpf?.Value,
                null,
                customer.BirthDate,
                null,
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
                null,
                false,
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
