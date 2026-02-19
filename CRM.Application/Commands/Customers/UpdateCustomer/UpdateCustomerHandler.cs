using CRM.Application.Common;
using CRM.Application.DTOs;
using CRM.Domain.Repositories;
using CRM.Domain.ValueObjects;

namespace CRM.Application.Commands.Customers.UpdateCustomer;

public class UpdateCustomerHandler : ICommandHandler<UpdateCustomerCommand, Result<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IEventStore _eventStore;

    public UpdateCustomerHandler(ICustomerRepository customerRepository, IEventStore eventStore)
    {
        _customerRepository = customerRepository;
        _eventStore = eventStore;
    }

    public async Task<Result<CustomerDto>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);

            if (customer == null)
                return Result<CustomerDto>.Failure("Cliente não encontrado");

            var existingEmailCustomer = await _customerRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingEmailCustomer != null && existingEmailCustomer.Id != customer.Id)
                return Result<CustomerDto>.Failure("Email já cadastrado para outro cliente");

            var address = Address.Create(
                request.Address.ZipCode,
                request.Address.Street,
                request.Address.Number,
                request.Address.Complement,
                request.Address.Neighborhood,
                request.Address.City,
                request.Address.State);

            customer.Update(
                request.Name,
                request.Phone,
                request.Email,
                address,
                request.StateRegistration,
                request.IsStateRegistrationExempt);

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
        catch (ArgumentException ex)
        {
            return Result<CustomerDto>.Failure(ex.Message);
        }
    }
}
