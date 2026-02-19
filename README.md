# CRM - Sistema de Gerenciamento de Clientes

Sistema CRM corporativo desenvolvido com .NET 10, seguindo princípios de **Domain-Driven Design (DDD)**, **CQRS** e **Event Sourcing**.

---

## ✅ Checklist de Entrega 

### Requisitos Funcionais Implementados
- ✅ Cadastro de Pessoa Física (Nome, CPF, Data de Nascimento, Telefone, Email, Endereço)
- ✅ Cadastro de Pessoa Jurídica (Razão Social, CNPJ, Data de Fundação, Telefone, Email, Endereço, IE)
- ✅ Atualização de clientes
- ✅ Listagem de clientes (com filtro por ativos)
- ✅ Busca de cliente por ID
- ✅ Consulta de CEP via ViaCEP
- ✅ Histórico completo de eventos (auditoria)

### Regras de Negócio Implementadas
- ✅ **Unicidade**: Bloqueio de CPF/CNPJ/Email duplicados
- ✅ **Compliance**: Pessoa Física com idade mínima de 18 anos
- ✅ **Tributação**: Pessoa Jurídica com IE obrigatória ou marcada como isenta
- ✅ **Consistência de Endereço**: Integração com ViaCEP para validação de CEP

### Arquitetura e Padrões
- ✅ **DDD**: Domain separado com Entities, Value Objects, Domain Events
- ✅ **CQRS**: Commands e Queries separados com MediatR
- ✅ **Event Sourcing**: Histórico imutável de todas as alterações
- ✅ **FluentValidation**: Validações declarativas e testáveis
- ✅ **Repository Pattern**: Abstração da camada de dados

### Resiliência e Qualidade
- ✅ **Polly**: Retry policies com backoff exponencial para ViaCEP
- ✅ **Global Exception Handler**: Tratamento centralizado de erros
- ✅ **Serilog**: Logging estruturado e rastreável
- ✅ **Validações**: CPF, CNPJ, Email, Telefone com algoritmos corretos

### Testes
- ✅ **Testes Unitários**: Domínio (Entities, Value Objects)
- ✅ **Testes de Integração**: Fluxo completo Command → Handler → Repository
- ✅ **FluentAssertions**: Assertions expressivas e legíveis

### Documentação
- ✅ **README.md**: Instruções completas de como rodar
- ✅ **ADRs**: 7 Architecture Decision Records explicando decisões
  - PostgreSQL, DDD, CQRS, Event Sourcing, FluentValidation, Polly, ViaCEP
- ✅ **OpenAPI/Swagger**: Documentação interativa da API

### Containerização
- ✅ **Dockerfile**: Build multi-stage otimizado
- ✅ **docker-compose.yml**: Sobe aplicação completa (API + PostgreSQL) com um comando
- ✅ **.dockerignore**: Otimização de build

### Banco de Dados
- ✅ **PostgreSQL 17**: Banco relacional com suporte a JSONB
- ✅ **Entity Framework Core 10**: ORM moderno
- ✅ **Migrations**: Versionamento de schema
- ✅ **Event Store**: Tabela dedicada para eventos de auditoria

---

## 🏗️ Arquitetura

### Camadas
- **Domain**: Entidades, Value Objects, Domain Events, Repository Interfaces
- **Application**: Commands, Queries, Handlers, Validators, DTOs
- **Infrastructure**: Repositórios, DbContext, Event Store, Serviços Externos
- **API**: Controllers, Middlewares, Configurações

### Tecnologias
- **.NET 10.0**
- **PostgreSQL 17** (JSONB para Event Store)
- **Entity Framework Core 10**
- **MediatR** (CQRS)
- **FluentValidation** (Validações)
- **Polly** (Retry Policies)
- **Serilog** (Logging estruturado)
- **xUnit + FluentAssertions** (Testes)

## 🚀 Como Rodar

> 💡 **Guia Rápido**: Ver [QUICK_START.md](QUICK_START.md) para instruções resumidas

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker](https://www.docker.com/get-started) e Docker Compose

### Opção 1: Docker Compose (Recomendado)

```bash
# Na raiz do projeto
docker-compose up --build
```

A API estará disponível em: `http://localhost:5000`

### Opção 2: Local (Desenvolvimento)

**1. Subir PostgreSQL:**
```bash
docker run --name crm-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=crm -p 5432:5432 -d postgres:17
```

**2. Restaurar dependências:**
```bash
dotnet restore
```

**3. Aplicar migrations:**
```bash
dotnet ef database update --project CRM.Infrastructure --startup-project CRM.Server
```

**4. Rodar a aplicação:**
```bash
dotnet run --project CRM.Server
```

A API estará disponível em: `https://localhost:7000` ou `http://localhost:5000`

### Swagger
Acesse a documentação interativa: `http://localhost:5000/swagger`

## 📋 Funcionalidades

### Gestão de Clientes

#### Pessoa Física 
```http
POST /api/customers/natural-person
Content-Type: application/json

{
  "name": "João Silva",
  "cpf": "123.456.789-09",
  "birthDate": "1990-05-15",
  "phone": "(11) 98765-4321",
  "email": "joao@example.com",
  "address": {
    "zipCode": "01310-100",
    "street": "Av. Paulista",
    "number": "1578",
    "complement": "Andar 10",
    "neighborhood": "Bela Vista",
    "city": "São Paulo",
    "state": "SP"
  }
}
```

**Validações:**
- ✅ CPF único e válido
- ✅ Email único e válido
- ✅ Idade mínima de 18 anos
- ✅ CEP válido (8 dígitos)

#### Pessoa Jurídica
```http
POST /api/customers/legal-entity
Content-Type: application/json

{
  "name": "Empresa XYZ Ltda",
  "cnpj": "12.345.678/0001-95",
  "foundationDate": "2015-03-20",
  "phone": "(11) 4004-1000",
  "email": "contato@empresa.com",
  "address": {
    "zipCode": "01310-100",
    "street": "Av. Paulista",
    "number": "1578",
    "neighborhood": "Bela Vista",
    "city": "São Paulo",
    "state": "SP"
  },
  "stateRegistration": "123456789",
  "isStateRegistrationExempt": false
}
```

**Validações:**
- ✅ CNPJ único e válido
- ✅ Email único e válido
- ✅ Inscrição Estadual obrigatória OU marcada como isenta

#### Listar Clientes
```http
GET /api/customers
GET /api/customers?onlyActive=true
```

#### Buscar Cliente por ID
```http
GET /api/customers/{id}
```

#### Atualizar Cliente
```http
PUT /api/customers/{id}
Content-Type: application/json

{
  "name": "João Silva Santos",
  "phone": "(11) 98765-4322",
  "email": "joao.novo@example.com",
  "address": { ... },
  "stateRegistration": null,
  "isStateRegistrationExempt": null
}
```

#### Histórico de Eventos (Auditoria)
```http
GET /api/customers/{id}/events
```

Retorna todos os eventos do cliente:
- CustomerCreatedEvent
- CustomerUpdatedEvent (com dados antigos e novos)
- CustomerDeactivatedEvent
- CustomerActivatedEvent

### Consulta de CEP (ViaCEP)
```http
GET /api/zipcode/01310100
```

Retorna:
```json
{
  "cep": "01310-100",
  "logradouro": "Avenida Paulista",
  "complemento": "",
  "bairro": "Bela Vista",
  "localidade": "São Paulo",
  "uf": "SP",
  "erro": false
}
```

## 🧪 Testes

### Executar todos os testes:
```bash
dotnet test
```

### Testes Unitários
- **Domain**: Value Objects (CPF, CNPJ, Email, etc.), Entidades (Customer)
- **Application**: Commands, Queries, Validators

### Testes de Integração
- Fluxo completo: Command → Handler → Repository → Database

## 📊 Event Sourcing & Auditoria

Todas as mudanças em clientes são registradas como eventos imutáveis:

```json
{
  "eventId": "uuid...",
  "aggregateId": "customer-id",
  "eventType": "CustomerUpdatedEvent",
  "eventData": {
    "oldData": { "email": "old@example.com" },
    "newData": { "email": "new@example.com" }
  },
  "userId": "system",
  "occurredAt": "2024-02-18T10:30:00Z"
}
```

**Benefícios:**
- ✅ Histórico completo de alterações
- ✅ "Quem alterou o quê e quando"
- ✅ Compliance (LGPD, auditorias)
- ✅ Debugging facilitado

## 🔒 Regras de Negócio

### Unicidade
- ❌ CPF duplicado
- ❌ CNPJ duplicado
- ❌ Email duplicado

### Compliance
- ✅ Pessoa Física: idade mínima de 18 anos
- ✅ Pessoa Jurídica: Inscrição Estadual obrigatória ou isenta

### Validações
- CPF: algoritmo de dígitos verificadores
- CNPJ: algoritmo de dígitos verificadores
- Email: formato válido (RFC 5322)
- Telefone: 10 ou 11 dígitos
- CEP: 8 dígitos

## 🛡️ Resiliência

### Retry Policy (Polly)
Chamadas para ViaCEP com retry exponencial:
- 3 tentativas
- Intervalo: 2s, 4s, 8s

### Global Exception Handler
Tratamento centralizado de erros:
- ArgumentException → 400 Bad Request
- KeyNotFoundException → 404 Not Found
- Exception → 500 Internal Server Error

### Logging Estruturado (Serilog)
```
[10:30:15 INF] Creating natural person customer: João Silva
[10:30:15 INF] Natural person created successfully: abc123...
```

## 📁 Estrutura do Projeto

```
CRM/
├── CRM.Domain/              # Domínio (regras de negócio)
│   ├── Common/              # Entity, ValueObject base
│   ├── Entities/            # Customer
│   ├── ValueObjects/        # CPF, CNPJ, Email, Phone, Address
│   ├── Events/              # Domain Events
│   ├── Enums/               # PersonType
│   └── Repositories/        # Interfaces
│
├── CRM.Application/         # Casos de uso
│   ├── Commands/            # Criar, Atualizar
│   ├── Queries/             # Buscar, Listar
│   ├── Behaviors/           # ValidationBehavior
│   ├── DTOs/                # Data Transfer Objects
│   └── Services/            # Interfaces de serviços
│
├── CRM.Infrastructure/      # Implementações
│   ├── Persistence/         # DbContext, Configurations, Migrations
│   ├── Repositories/        # CustomerRepository, EventStore
│   └── Services/            # ViaCepService
│
├── CRM.Server/              # API
│   ├── Controllers/         # CustomersController, ZipCodeController
│   ├── Middleware/          # GlobalExceptionHandler
│   └── Program.cs           # Configuração DI
│
├── CRM.Tests/               # Testes
│   ├── Domain/              # Testes unitários
│   └── Integration/         # Testes de integração
│
├── docs/
│   └── adr/                 # Architecture Decision Records
│
├── docker-compose.yml
├── Dockerfile
└── README.md
```

## 📖 Architecture Decision Records (ADRs)

Documentação completa das decisões arquiteturais:

- [ADR 001: PostgreSQL como Banco de Dados](docs/adr/001-postgresql-database.md)
  - Por que PostgreSQL vs MySQL/MongoDB/SQL Server
  - Vantagens do JSONB para Event Store

- [ADR 002: Domain-Driven Design (DDD)](docs/adr/002-domain-driven-design.md)
  - Estrutura em camadas
  - Entities vs Value Objects
  - Aggregate Roots

- [ADR 003: CQRS Pattern](docs/adr/003-cqrs-pattern.md)
  - Separação Command/Query
  - MediatR como Mediator
  - Pipeline Behaviors

- [ADR 004: Event Sourcing para Auditabilidade](docs/adr/004-event-sourcing.md)
  - Por que Event Sourcing?
  - Estrutura de eventos
  - Event Store no PostgreSQL

- [ADR 005: FluentValidation](docs/adr/005-fluentvalidation.md)
  - Validações vs Invariantes
  - Pipeline de validação automática
  - Validators testáveis

- [ADR 006: Resiliência com Polly](docs/adr/006-resilience-polly.md)
  - Retry Policies
  - Circuit Breaker (futuro)
  - Padrões de resiliência

- [ADR 007: Integração com ViaCEP](docs/adr/007-viacep-integration.md)
  - Por que ViaCEP vs Google Maps
  - Tratamento de erros
  - Cache (futuro)

## 🔄 Evolução Futura

### Integrações
- [ ] Message Bus (RabbitMQ/Kafka) para eventos
- [ ] Autenticação/Autorização (JWT + Azure AD)
- [ ] Integração com módulos de Faturamento e Suporte

### Performance
- [ ] Cache (Redis) para queries frequentes
- [ ] Read Models separados (CQRS completo)
- [ ] Snapshots para Event Sourcing

### Monitoramento
- [ ] Application Insights / Prometheus
- [ ] Health Checks avançados
- [ ] Distributed Tracing (OpenTelemetry)

## 👥 Desenvolvedor

Sistema desenvolvido, demonstrando:
- ✅ Clean Architecture / DDD
- ✅ CQRS + Event Sourcing
- ✅ Testes automatizados
- ✅ Containerização (Docker)
- ✅ Resiliência e Logging
- ✅ Documentação técnica (ADRs)

---

**Nota**: Este é um MVP funcional. Em produção, considere adicionar autenticação, rate limiting, cache, monitoring e testes end-to-end.
