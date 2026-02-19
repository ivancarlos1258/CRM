using System.Net;
using System.Net.Http.Json;
using CRM.Application.Commands.Customers.CreateNaturalPerson;
using CRM.Application.DTOs;
using FluentAssertions;

namespace CRM.Tests.Integration;

public class CustomerIntegrationTests : IClassFixture<CrmWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CustomerIntegrationTests(CrmWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateNaturalPerson_WithValidData_ShouldReturnCreated()
    {
        var command = new CreateNaturalPersonCommand(
            Name: "João Silva",
            Cpf: "12345678909",
            BirthDate: DateTime.Now.AddYears(-25),
            Phone: "11987654321",
            Email: $"joao.{Guid.NewGuid()}@example.com",
            Address: new CreateAddressDto(
                ZipCode: "01310100",
                Street: "Av. Paulista",
                Number: "1578",
                Complement: null,
                Neighborhood: "Bela Vista",
                City: "São Paulo",
                State: "SP"
            )
        );

        var response = await _client.PostAsJsonAsync("/api/customers/natural-person", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        customer.Should().NotBeNull();
        customer!.Name.Should().Be("João Silva");
        customer.Cpf.Should().Be("12345678909");
        customer.Email.Should().Be(command.Email.ToLowerInvariant());
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateNaturalPerson_WithInvalidAge_ShouldReturnBadRequest()
    {
        var command = new CreateNaturalPersonCommand(
            Name: "João Jovem",
            Cpf: "98765432109",
            BirthDate: DateTime.Now.AddYears(-16),
            Phone: "11987654321",
            Email: $"jovem.{Guid.NewGuid()}@example.com",
            Address: new CreateAddressDto(
                ZipCode: "01310100",
                Street: "Av. Paulista",
                Number: "1578",
                Complement: null,
                Neighborhood: "Bela Vista",
                City: "São Paulo",
                State: "SP"
            )
        );

        var response = await _client.PostAsJsonAsync("/api/customers/natural-person", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCustomerById_AfterCreation_ShouldReturnCustomer()
    {
        var createCommand = new CreateNaturalPersonCommand(
            Name: "Maria Santos",
            Cpf: "11122233344",
            BirthDate: DateTime.Now.AddYears(-30),
            Phone: "11987654321",
            Email: $"maria.{Guid.NewGuid()}@example.com",
            Address: new CreateAddressDto(
                ZipCode: "01310100",
                Street: "Av. Paulista",
                Number: "1578",
                Complement: null,
                Neighborhood: "Bela Vista",
                City: "São Paulo",
                State: "SP"
            )
        );

        var createResponse = await _client.PostAsJsonAsync("/api/customers/natural-person", createCommand);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();

        var getResponse = await _client.GetAsync($"/api/customers/{createdCustomer!.Id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var customer = await getResponse.Content.ReadFromJsonAsync<CustomerDto>();
        customer.Should().NotBeNull();
        customer!.Id.Should().Be(createdCustomer.Id);
        customer.Name.Should().Be("Maria Santos");
    }

    [Fact]
    public async Task GetAllCustomers_ShouldReturnList()
    {
        var response = await _client.GetAsync("/api/customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var customers = await response.Content.ReadFromJsonAsync<IEnumerable<CustomerDto>>();
        customers.Should().NotBeNull();
        customers.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCustomerEvents_AfterCreation_ShouldReturnEvents()
    {
        var createCommand = new CreateNaturalPersonCommand(
            Name: "Pedro Alves",
            Cpf: "55566677788",
            BirthDate: DateTime.Now.AddYears(-28),
            Phone: "11987654321",
            Email: $"pedro.{Guid.NewGuid()}@example.com",
            Address: new CreateAddressDto(
                ZipCode: "01310100",
                Street: "Av. Paulista",
                Number: "1578",
                Complement: null,
                Neighborhood: "Bela Vista",
                City: "São Paulo",
                State: "SP"
            )
        );

        var createResponse = await _client.PostAsJsonAsync("/api/customers/natural-person", createCommand);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();

        var eventsResponse = await _client.GetAsync($"/api/customers/{createdCustomer!.Id}/events");

        eventsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var events = await eventsResponse.Content.ReadFromJsonAsync<IEnumerable<EventDto>>();
        events.Should().NotBeNull();
        events.Should().NotBeEmpty();
        events!.First().EventType.Should().Be("CustomerCreatedEvent");
    }

    [Fact]
    public async Task UpdateCustomer_WithValidData_ShouldReturnOk()
    {
        var createCommand = new CreateNaturalPersonCommand(
            Name: "Ana Costa",
            Cpf: "99988877766",
            BirthDate: DateTime.Now.AddYears(-27),
            Phone: "11987654321",
            Email: $"ana.{Guid.NewGuid()}@example.com",
            Address: new CreateAddressDto(
                ZipCode: "01310100",
                Street: "Av. Paulista",
                Number: "1578",
                Complement: null,
                Neighborhood: "Bela Vista",
                City: "São Paulo",
                State: "SP"
            )
        );

        var createResponse = await _client.PostAsJsonAsync("/api/customers/natural-person", createCommand);
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();

        var updateRequest = new
        {
            Name = "Ana Costa Silva",
            Phone = "11987654322",
            Email = createdCustomer!.Email,
            Address = new
            {
                ZipCode = "04567890",
                Street = "Rua Nova",
                Number = "100",
                Complement = (string?)null,
                Neighborhood = "Centro",
                City = "São Paulo",
                State = "SP"
            },
            StateRegistration = (string?)null,
            IsStateRegistrationExempt = (bool?)null
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/customers/{createdCustomer.Id}", updateRequest);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedCustomer = await updateResponse.Content.ReadFromJsonAsync<CustomerDto>();
        updatedCustomer.Should().NotBeNull();
        updatedCustomer!.Name.Should().Be("Ana Costa Silva");
        updatedCustomer.Phone.Should().Be("11987654322");
        updatedCustomer.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateNaturalPerson_WithDuplicateCpf_ShouldReturnBadRequest()
    {
        var cpf = "11111111111";

        var command1 = new CreateNaturalPersonCommand(
            Name: "Cliente 1",
            Cpf: cpf,
            BirthDate: DateTime.Now.AddYears(-25),
            Phone: "11987654321",
            Email: $"cliente1.{Guid.NewGuid()}@example.com",
            Address: new CreateAddressDto(
                ZipCode: "01310100",
                Street: "Av. Paulista",
                Number: "1578",
                Complement: null,
                Neighborhood: "Bela Vista",
                City: "São Paulo",
                State: "SP"
            )
        );

        await _client.PostAsJsonAsync("/api/customers/natural-person", command1);

        var command2 = new CreateNaturalPersonCommand(
            Name: "Cliente 2",
            Cpf: cpf,
            BirthDate: DateTime.Now.AddYears(-30),
            Phone: "11987654322",
            Email: $"cliente2.{Guid.NewGuid()}@example.com",
            Address: new CreateAddressDto(
                ZipCode: "01310100",
                Street: "Av. Paulista",
                Number: "1578",
                Complement: null,
                Neighborhood: "Bela Vista",
                City: "São Paulo",
                State: "SP"
            )
        );

        var response = await _client.PostAsJsonAsync("/api/customers/natural-person", command2);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
