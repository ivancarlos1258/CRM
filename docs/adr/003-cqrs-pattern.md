# ADR 003: CQRS (Command Query Responsibility Segregation)

## Status
Aceito

## Contexto
O sistema precisa lidar com operações de escrita (criação, atualização) e leitura de clientes. Estas operações têm requisitos diferentes:
- **Escritas**: Validações complexas, regras de negócio, auditoria
- **Leituras**: Performance, diferentes formatos de dados, sem validações

## Decisão
Implementar CQRS separando Commands (escrita) de Queries (leitura).

### Estrutura

**Commands (Escrita)**
```csharp
public record CreateNaturalPersonCommand(...) : ICommand<Result<CustomerDto>>;
public class CreateNaturalPersonHandler : ICommandHandler<...>
public class CreateNaturalPersonValidator : AbstractValidator<...>
```

**Queries (Leitura)**
```csharp
public record GetCustomerByIdQuery(Guid Id) : IQuery<Result<CustomerDto>>;
public class GetCustomerByIdHandler : IQueryHandler<...>
```

### MediatR como Mediator
Usamos MediatR para implementar o padrão Mediator:
- Desacopla Controllers dos Handlers
- Pipeline de comportamentos (validação, logging, etc.)
- Facilita testes

## Razões

### Vantagens
1. **Separação Clara**: Commands modificam estado, Queries apenas leem
2. **Otimização Independente**: Posso otimizar leituras sem afetar escritas
3. **Escalabilidade**: Facilita escalar leitura e escrita separadamente
4. **Simplicidade**: Queries podem ignorar complexidades do domínio
5. **Read Models**: Permite diferentes projeções dos dados
6. **Performance**: Queries podem ir direto no banco, sem passar pelo domínio

### Por que MediatR?
- ✅ Padrão Mediator bem implementado
- ✅ Pipeline behaviors (validação automática)
- ✅ Reduz acoplamento
- ✅ Facilita testes (mockar IMediator)
- ✅ Código mais limpo nos controllers

## Alternativas Consideradas

**Controllers direto com Services**
- ✅ Mais simples inicialmente
- ❌ Controllers inchados
- ❌ Dificulta testes
- ❌ Acoplamento alto

**Event Bus completo**
- ✅ Desacoplamento total
- ❌ Complexidade excessiva para o escopo atual
- ❌ Over-engineering

## Consequências

### Positivas
- Controllers magros (apenas delegam para MediatR)
- Handlers focados em uma responsabilidade
- Fácil adicionar comportamentos no pipeline (cache, retry, etc.)
- Testabilidade excelente

### Negativas
- Mais arquivos/classes
- Curva de aprendizado do MediatR
- Pode parecer over-engineering para CRUDs simples

## Implementação

### Pipeline Behavior para Validação
```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<...>
{
    // Valida automaticamente antes de executar o handler
}
```

### Controller Limpo
```csharp
[HttpPost("natural-person")]
public async Task<IActionResult> CreateNaturalPerson(CreateNaturalPersonCommand command)
{
    var result = await _mediator.Send(command);
    return result.IsSuccess ? Created(...) : BadRequest(...);
}
```

## Evolução Futura
- Adicionar cache em queries específicas
- Implementar Read Models separados (se necessário)
- Adicionar Command/Query logging
- Métricas de performance por comando/query
