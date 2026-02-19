using CRM.Domain.Common;
using CRM.Domain.Enums;
using CRM.Domain.ValueObjects;

namespace CRM.Domain.Entities;

public class Customer : Entity
{
    public PersonType PersonType { get; private set; }
    public string Name { get; private set; }
    public Cpf? Cpf { get; private set; }
    public Cnpj? Cnpj { get; private set; }
    public DateTime? BirthDate { get; private set; }
    public DateTime? FoundationDate { get; private set; }
    public Phone Phone { get; private set; }
    public Email Email { get; private set; }
    public Address Address { get; private set; }
    public string? StateRegistration { get; private set; }
    public bool IsStateRegistrationExempt { get; private set; }
    public bool IsActive { get; private set; }

    private Customer() { }

    private Customer(
        PersonType personType,
        string name,
        Cpf? cpf,
        Cnpj? cnpj,
        DateTime? birthDate,
        DateTime? foundationDate,
        Phone phone,
        Email email,
        Address address,
        string? stateRegistration,
        bool isStateRegistrationExempt)
    {
        PersonType = personType;
        Name = name;
        Cpf = cpf;
        Cnpj = cnpj;
        BirthDate = birthDate;
        FoundationDate = foundationDate;
        Phone = phone;
        Email = email;
        Address = address;
        StateRegistration = stateRegistration;
        IsStateRegistrationExempt = isStateRegistrationExempt;
        IsActive = true;

        ValidateCustomer();
    }

    public static Customer CreateNaturalPerson(
        string name,
        string cpf,
        DateTime birthDate,
        string phone,
        string email,
        Address address)
    {
        var customer = new Customer(
            PersonType.NaturalPerson,
            name,
            Cpf.Create(cpf),
            null,
            birthDate,
            null,
            Phone.Create(phone),
            Email.Create(email),
            address,
            null,
            false);

        customer.AddDomainEvent(new CustomerCreatedEvent(customer.Id, customer.Name, PersonType.NaturalPerson));
        return customer;
    }

    public static Customer CreateLegalEntity(
        string name,
        string cnpj,
        DateTime foundationDate,
        string phone,
        string email,
        Address address,
        string? stateRegistration,
        bool isStateRegistrationExempt)
    {
        var customer = new Customer(
            PersonType.LegalEntity,
            name,
            null,
            Cnpj.Create(cnpj),
            null,
            foundationDate,
            Phone.Create(phone),
            Email.Create(email),
            address,
            stateRegistration,
            isStateRegistrationExempt);

        customer.AddDomainEvent(new CustomerCreatedEvent(customer.Id, customer.Name, PersonType.LegalEntity));
        return customer;
    }

    public void Update(
        string name,
        string phone,
        string email,
        Address address,
        string? stateRegistration = null,
        bool? isStateRegistrationExempt = null)
    {
        var oldData = new
        {
            Name,
            Phone = Phone.Value,
            Email = Email.Value,
            Address,
            StateRegistration,
            IsStateRegistrationExempt
        };

        Name = name;
        Phone = Phone.Create(phone);
        Email = Email.Create(email);
        Address = address;
        UpdatedAt = DateTime.UtcNow;

        if (PersonType == PersonType.LegalEntity)
        {
            StateRegistration = stateRegistration;
            IsStateRegistrationExempt = isStateRegistrationExempt ?? false;
        }

        ValidateCustomer();

        AddDomainEvent(new CustomerUpdatedEvent(
            Id,
            Name,
            PersonType,
            System.Text.Json.JsonSerializer.Serialize(oldData),
            System.Text.Json.JsonSerializer.Serialize(new
            {
                Name,
                Phone = Phone.Value,
                Email = Email.Value,
                Address,
                StateRegistration,
                IsStateRegistrationExempt
            })));
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CustomerDeactivatedEvent(Id, Name, PersonType));
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new CustomerActivatedEvent(Id, Name, PersonType));
    }

    private void ValidateCustomer()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Nome não pode ser vazio");

        if (PersonType == PersonType.NaturalPerson)
        {
            if (Cpf == null)
                throw new ArgumentException("CPF é obrigatório para Pessoa Física");

            if (BirthDate == null)
                throw new ArgumentException("Data de nascimento é obrigatória para Pessoa Física");

            var age = CalculateAge(BirthDate.Value);
            if (age < 18)
                throw new ArgumentException("Cliente deve ter no mínimo 18 anos");
        }
        else if (PersonType == PersonType.LegalEntity)
        {
            if (Cnpj == null)
                throw new ArgumentException("CNPJ é obrigatório para Pessoa Jurídica");

            if (FoundationDate == null)
                throw new ArgumentException("Data de fundação é obrigatória para Pessoa Jurídica");

            if (string.IsNullOrWhiteSpace(StateRegistration) && !IsStateRegistrationExempt)
                throw new ArgumentException("Inscrição Estadual é obrigatória ou deve ser marcada como isenta");
        }
    }

    private int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
            age--;
        return age;
    }
}
