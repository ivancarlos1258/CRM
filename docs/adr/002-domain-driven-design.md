# ADR 002: Arquitetura com Domain-Driven Design (DDD)

## Status
Aceito

## Contexto
O sistema CRM é crítico para o negócio e servirá de base para futuras integrações. Precisamos de uma arquitetura que:
- Mantenha a lógica de negócio isolada e testável
- Facilite a manutenção e evolução
- Permita escalabilidade e mudanças futuras
- Seja clara para novos desenvolvedores

## Decisão
Implementar Domain-Driven Design (DDD) com separação clara em camadas:

### Camadas

**1. Domain (Núcleo)**
- Entities: Customer
- Value Objects: CPF, CNPJ, Email, Phone, Address
- Domain Events: CustomerCreated, CustomerUpdated, etc.
- Repository Interfaces
- Regras de negócio puras (sem dependências externas)

**2. Application**
- Commands e Queries (CQRS)
- Handlers (MediatR)
- DTOs
- Validators (FluentValidation)
- Interfaces de serviços externos

**3. Infrastructure**
- Implementação de Repositórios
- DbContext (Entity Framework Core)
- Event Store
- Serviços externos (ViaCEP)
- Configurações de persistência

**4. API/Presentation**
- Controllers
- Middlewares
- Configuração de dependências
- DTOs de entrada/saída

## Razões

### Vantagens do DDD
1. **Separação de Responsabilidades**: Cada camada tem propósito claro
2. **Testabilidade**: Domínio isolado facilita testes unitários
3. **Expressividade**: Código reflete o negócio (Ubiquitous Language)
4. **Manutenibilidade**: Mudanças em uma camada não afetam outras
5. **Escalabilidade**: Facilita divisão em microserviços no futuro

### Value Objects vs Primitive Obsession
Usamos Value Objects (CPF, CNPJ, Email) ao invés de strings porque:
- Encapsulam validações
- Garantem invariantes
- Tornam o código mais expressivo
- Facilitam refatorações

### Entities com Comportamento Rico
A entidade Customer tem:
- Métodos de criação específicos (CreateNaturalPerson, CreateLegalEntity)
- Validações internas
- Emissão de Domain Events
- Proteção de estado interno (setters privados)

## Alternativas Consideradas

**Transaction Script**
- ✅ Mais simples para começar
- ❌ Dificulta manutenção em sistemas complexos
- ❌ Lógica de negócio espalhada

**Anemic Domain Model**
- ✅ Familiar para muitos devs
- ❌ Entidades sem comportamento (apenas getters/setters)
- ❌ Lógica em services, não no domínio

## Consequências

### Positivas
- Código altamente testável
- Regras de negócio centralizadas no domínio
- Fácil evolução e adição de funcionalidades
- Documentação viva através do código

### Negativas
- Curva de aprendizado mais alta
- Mais código boilerplate inicial
- Requer disciplina da equipe

## Padrões Implementados

### Entity Base Class
```csharp
public abstract class Entity
{
    public Guid Id { get; protected set; }
    private readonly List<IDomainEvent> _domainEvents;
    // ...
}
```

### Value Object Base Class
```csharp
public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();
    // Equality by value
}
```

### Domain Events
```csharp
public class CustomerCreatedEvent : IDomainEvent
{
    // Event data for audit and integration
}
```

## Notas de Implementação
- Domínio não tem dependência de outras camadas
- Application depende apenas do Domain
- Infrastructure implementa interfaces do Domain
- API orquestra tudo através de DI
