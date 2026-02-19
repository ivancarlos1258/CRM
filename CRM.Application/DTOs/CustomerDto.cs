using CRM.Domain.Enums;

namespace CRM.Application.DTOs;

public record CustomerDto(
    Guid Id,
    PersonType PersonType,
    string Name,
    string? Cpf,
    string? Cnpj,
    DateTime? BirthDate,
    DateTime? FoundationDate,
    string Phone,
    string Email,
    AddressDto Address,
    string? StateRegistration,
    bool IsStateRegistrationExempt,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record AddressDto(
    string ZipCode,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State
);

public record CreateAddressDto(
    string ZipCode,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State
);

public record ZipCodeInfoDto(
    string Cep,
    string Logradouro,
    string Complemento,
    string Bairro,
    string Localidade,
    string Uf,
    bool Erro
);
