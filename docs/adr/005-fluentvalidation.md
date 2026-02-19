# ADR 005: FluentValidation para Validações

## Status
Aceito

## Contexto
A aplicação precisa validar dados de entrada antes de processar commands. As validações devem ser:
- Expressivas e legíveis
- Fáceis de testar
- Separadas da lógica de negócio
- Reutilizáveis

## Decisão
Usar **FluentValidation** para todas as validações de Commands.

## Estrutura

### Validators
```csharp
public class CreateNaturalPersonValidator : AbstractValidator<CreateNaturalPersonCommand>
{
    public CreateNaturalPersonValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório");

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Data de nascimento é obrigatória")
            .LessThan(DateTime.Today).WithMessage("Data de nascimento deve ser no passado");
    }
}
```

### Pipeline Behavior
```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<...>
{
    // Executa validadores automaticamente antes do handler
    // Se falhar, retorna Result.Failure com lista de erros
}
```

## Razões

### Por que FluentValidation?

**Expressividade**
```csharp
// ✅ Fluent
RuleFor(x => x.Email)
    .NotEmpty().WithMessage("Email é obrigatório")
    .EmailAddress().WithMessage("Email inválido");

// ❌ Data Annotations (menos expressivo)
[Required(ErrorMessage = "Email é obrigatório")]
[EmailAddress(ErrorMessage = "Email inválido")]
public string Email { get; set; }
```

**Separação de Responsabilidades**
- Validators ficam em arquivos próprios
- Command é apenas um DTO (sem lógica)
- Facilita testes unitários dos validators

**Validações Complexas**
```csharp
RuleFor(x => x)
    .Must(x => !string.IsNullOrWhiteSpace(x.StateRegistration) || x.IsStateRegistrationExempt)
    .WithMessage("Inscrição Estadual é obrigatória ou deve ser marcada como isenta");
```

**Validações Assíncronas**
```csharp
RuleFor(x => x.Email)
    .MustAsync(async (email, ct) => !await _repo.ExistsByEmailAsync(email, ct))
    .WithMessage("Email já cadastrado");
```

## Alternativas Consideradas

**Data Annotations**
- ✅ Built-in no .NET
- ✅ Simples para casos básicos
- ❌ Menos expressivo
- ❌ Dificulta validações complexas
- ❌ Validações no DTO (viola SRP)

**Validação Manual nos Handlers**
- ✅ Controle total
- ❌ Código repetitivo
- ❌ Dificulta testes
- ❌ Mistura validação com lógica

**Validação no Domínio**
- ✅ Coerente com DDD
- ❌ Domínio não deve conhecer DTOs
- ❌ Validações de input vs invariantes são diferentes

## Implementação

### Registro no DI
```csharp
services.AddValidatorsFromAssembly(typeof(ICommand<>).Assembly);
services.AddMediatR(cfg => {
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
```

### Fluxo
1. Controller recebe Command
2. MediatR intercepta com ValidationBehavior
3. ValidationBehavior executa todos os validators do Command
4. Se houver erros, retorna `Result.Failure(errors)`
5. Se OK, continua para o Handler

### Resposta de Erro
```json
{
  "error": null,
  "errors": [
    "Nome é obrigatório",
    "CPF inválido",
    "Cliente deve ter no mínimo 18 anos"
  ]
}
```

## Validações vs Invariantes do Domínio

**Validações (Application Layer)**
- Formato de entrada
- Obrigatoriedade de campos
- Tamanho de strings
- Formato de email, telefone
- **Onde**: Validators (FluentValidation)

**Invariantes (Domain Layer)**
- Regras de negócio
- Unicidade (CPF, email)
- Idade mínima de 18 anos
- Inscrição Estadual obrigatória ou isenta
- **Onde**: Value Objects, Entities

### Exemplo: CPF

**Validação (Application)**
```csharp
RuleFor(x => x.Cpf).NotEmpty();
```

**Invariante (Domain)**
```csharp
public static Cpf Create(string cpf)
{
    if (!IsValid(cpf))
        throw new ArgumentException("CPF inválido");
    // ...
}
```

## Benefícios

1. **Fail Fast**: Erros detectados antes de chegar ao domínio
2. **Mensagens Claras**: Erros específicos e amigáveis
3. **Testabilidade**: Validators são classes testáveis isoladamente
4. **Reutilização**: Validators podem ser compostos
5. **Documentação**: Código de validação documenta regras

## Consequências

### Positivas
- Validações centralizadas e consistentes
- Fácil adicionar/modificar validações
- Testes de validação isolados
- Controllers limpos (sem validação manual)

### Negativas
- Mais uma biblioteca de terceiros
- Curva de aprendizado (sintaxe Fluent)
- Duplicação entre validators e invariantes do domínio

### Mitigações
- Documentar diferença entre validações e invariantes
- Validators testam apenas formato/obrigatoriedade
- Domínio testa regras de negócio

## Evolução Futura
- Custom Validators reutilizáveis (CPF, CNPJ)
- Validações assíncronas (checar duplicidade)
- Integração com i18n para mensagens multilíngue
- Validators complexos para workflows de aprovação
