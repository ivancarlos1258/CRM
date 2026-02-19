# 📦 Solution CRM - Estrutura de Projetos

## ✅ **Todos os Projetos Adicionados à Solution!**

### **📊 Estrutura da Solution:**

```
CRM.sln (C:\Projetos\CRM\CRM.slnx)
├── CRM.Domain              (Camada de Domínio - DDD)
├── CRM.Application         (Camada de Aplicação - CQRS)
├── CRM.Infrastructure      (Camada de Infraestrutura)
├── CRM.Server              (API REST - Controllers)
├── CRM.Tests               (Testes - Unit + Integration)
├── CRM.AppHost             (.NET Aspire Orchestration)
└── frontend.esproj         (React + TypeScript + Vite)
```

---

## 🏗️ **Arquitetura dos Projetos:**

### **1. CRM.Domain** 📘
**Camada**: Domínio (DDD - Domain-Driven Design)

**Responsabilidades:**
- ✅ Entidades (`Customer`)
- ✅ Value Objects (`CPF`, `CNPJ`, `Email`, `Phone`, `Address`)
- ✅ Interfaces de repositórios
- ✅ Domain Events (`CustomerCreated`, `CustomerUpdated`, etc)
- ✅ Enums (`PersonType`)
- ✅ Regras de negócio puras

**Dependências:** Nenhuma (camada mais interna)

---

### **2. CRM.Application** 📗
**Camada**: Aplicação (CQRS + Mediator Pattern)

**Responsabilidades:**
- ✅ **Commands** (Write operations)
  - `CreateNaturalPersonCommand`
  - `CreateLegalEntityCommand`
  - `UpdateCustomerCommand`
  - `ActivateCustomerCommand`
  - `DeactivateCustomerCommand`
- ✅ **Queries** (Read operations)
  - `GetAllCustomersQuery`
  - `GetCustomerByIdQuery`
  - `GetCustomerEventsQuery`
  - `GetZipCodeInfoQuery`
- ✅ **Handlers** (Processamento via MediatR)
- ✅ **Validators** (FluentValidation)
- ✅ **DTOs** (Data Transfer Objects)
- ✅ **Behaviors** (ValidationBehavior)

**Dependências:**
- `CRM.Domain`
- `MediatR`
- `FluentValidation`

---

### **3. CRM.Infrastructure** 📙
**Camada**: Infraestrutura (Persistência + Integrações)

**Responsabilidades:**
- ✅ **EF Core + PostgreSQL**
  - `CrmDbContext`
  - Configurações de entidades
  - Migrations
- ✅ **Repositórios**
  - `CustomerRepository`
  - `EventStore` (Event Sourcing)
- ✅ **Serviços Externos**
  - `ViaCepService` (Integração ViaCEP)
- ✅ **Persistência de Eventos**

**Dependências:**
- `CRM.Domain`
- `CRM.Application`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `Microsoft.EntityFrameworkCore`

---

### **4. CRM.Server** 🌐
**Camada**: Apresentação (API REST)

**Responsabilidades:**
- ✅ **Controllers**
  - `CustomersController` (CRUD completo)
  - `ZipCodeController` (Consulta CEP)
- ✅ **Middlewares**
  - `GlobalExceptionHandlerMiddleware`
- ✅ **Configuração**
  - Dependency Injection
  - Swagger/OpenAPI
  - CORS
  - Serilog (Logging)
- ✅ **Endpoints REST**
  - `GET /api/customers`
  - `POST /api/customers/natural-person`
  - `POST /api/customers/legal-entity`
  - `PUT /api/customers/{id}`
  - `PUT /api/customers/{id}/activate`
  - `PUT /api/customers/{id}/deactivate`
  - `GET /api/customers/{id}/events`
  - `GET /api/zipcode/{zipCode}`

**Dependências:**
- `CRM.Application`
- `CRM.Infrastructure`
- `Swashbuckle.AspNetCore` (Swagger)
- `Serilog`
- `.NET Aspire`

**URLs:**
- 🌐 API: http://localhost:5000
- 📚 Swagger: http://localhost:5000/swagger

---

### **5. CRM.Tests** 🧪
**Camada**: Testes (Quality Assurance)

**Responsabilidades:**
- ✅ **Unit Tests**
  - Testes de Value Objects
  - Testes de entidades
  - Testes de regras de negócio
- ✅ **Integration Tests**
  - Testes de API end-to-end
  - `CrmWebApplicationFactory`
  - Testes com banco de dados em memória

**Dependências:**
- `CRM.Domain`
- `CRM.Application`
- `CRM.Server`
- `xUnit`
- `Microsoft.AspNetCore.Mvc.Testing`

---

### **6. CRM.AppHost** 🚀
**Camada**: Orquestração (.NET Aspire)

**Responsabilidades:**
- ✅ Orquestração de serviços
- ✅ Service Discovery
- ✅ Dashboard de monitoramento
- ✅ Configuração de recursos distribuídos
- ✅ PostgreSQL container
- ✅ Frontend proxy

**Dependências:**
- `.NET Aspire`
- `CRM.Server`

---

### **7. frontend.esproj** ⚛️
**Camada**: Frontend SPA (Single Page Application)

**Stack:**
- ⚛️ **React 18**
- 📘 **TypeScript**
- ⚡ **Vite**
- 🎨 **CSS3**

**Funcionalidades:**
- ✅ CRUD completo de clientes
- ✅ Grid com paginação (1, 5, 10, 25, 50, 100 itens)
- ✅ Busca em tempo real
- ✅ Filtros (Todos, Ativos, Inativos)
- ✅ Ordenação (Nome A-Z, Data ↑↓)
- ✅ Formulário PF/PJ
- ✅ Autocomplete de CEP (ViaCEP)
- ✅ Validação de CPF/CNPJ
- ✅ Máscaras de formatação
- ✅ Responsivo (Desktop + Mobile)

**URLs:**
- 🌐 Frontend: http://localhost:5173

---

## 🔗 **Dependências entre Projetos:**

```
┌─────────────────────────────────────────────────┐
│               CRM.Server (API)                   │
│         http://localhost:5000/swagger            │
└─────────────┬───────────────────────────────────┘
              │ usa
              ▼
┌─────────────────────────────────────────────────┐
│          CRM.Application (CQRS)                  │
│     Commands + Queries + Handlers                │
└─────────────┬───────────────────────────────────┘
              │ usa
              ▼
┌─────────────────────────────────────────────────┐
│      CRM.Infrastructure (Persistência)           │
│       EF Core + PostgreSQL + ViaCEP             │
└─────────────┬───────────────────────────────────┘
              │ implementa
              ▼
┌─────────────────────────────────────────────────┐
│          CRM.Domain (Domínio DDD)                │
│    Entidades + Value Objects + Events           │
└─────────────────────────────────────────────────┘

       ┌──────────────────────────┐
       │    CRM.Tests (Testes)     │
       │  Unit + Integration Tests │
       └───────────┬───────────────┘
                   │ testa todos
                   ▼
             [Todos os Projetos]

┌─────────────────────────────────────────────────┐
│      CRM.AppHost (.NET Aspire)                   │
│     Orquestra Server + PostgreSQL + Frontend     │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│      frontend (React + TypeScript)               │
│      http://localhost:5173                       │
└─────────────────────────────────────────────────┘
```

---

## 📋 **Comandos Úteis:**

### **Compilar Solution:**
```powershell
dotnet build
```

### **Restaurar Pacotes:**
```powershell
dotnet restore
```

### **Executar Testes:**
```powershell
dotnet test
```

### **Listar Projetos:**
```powershell
dotnet sln list
```

### **Adicionar Novo Projeto:**
```powershell
dotnet sln add <caminho-do-projeto.csproj>
```

### **Remover Projeto:**
```powershell
dotnet sln remove <caminho-do-projeto.csproj>
```

---

## 🚀 **Como Executar:**

### **Opção 1 - .NET Aspire (Recomendado):**
```powershell
dotnet run --project CRM.AppHost
```
Acesse: https://localhost:17265 (Dashboard Aspire)

### **Opção 2 - Backend Standalone:**
```powershell
dotnet run --project CRM.Server
```
Acesse: http://localhost:5000/swagger

### **Opção 3 - Frontend Standalone:**
```powershell
cd frontend
npm install
npm run dev
```
Acesse: http://localhost:5173

---

## 📦 **Pacotes NuGet Instalados:**

### **CRM.Domain:**
- ✅ Sem dependências externas (Clean Architecture)

### **CRM.Application:**
- ✅ `MediatR` (CQRS)
- ✅ `FluentValidation` (Validações)

### **CRM.Infrastructure:**
- ✅ `Microsoft.EntityFrameworkCore`
- ✅ `Npgsql.EntityFrameworkCore.PostgreSQL`
- ✅ `Microsoft.EntityFrameworkCore.Design`

### **CRM.Server:**
- ✅ `Swashbuckle.AspNetCore` (Swagger/OpenAPI)
- ✅ `Serilog.AspNetCore` (Logging)
- ✅ `Aspire.Hosting.PostgreSQL` (.NET Aspire)

### **CRM.Tests:**
- ✅ `xUnit`
- ✅ `Microsoft.AspNetCore.Mvc.Testing`
- ✅ `Microsoft.EntityFrameworkCore.InMemory`

---

## 🎯 **Padrões Arquiteturais Implementados:**

### **1. Clean Architecture** 🏛️
- Separação clara de camadas
- Dependências apontando para dentro
- Domain no centro (sem dependências externas)

### **2. Domain-Driven Design (DDD)** 📘
- Entidades com comportamento rico
- Value Objects imutáveis
- Aggregate Roots
- Domain Events

### **3. CQRS (Command Query Responsibility Segregation)** 🔀
- Separação de operações de leitura e escrita
- Commands para mutações
- Queries para consultas

### **4. Event Sourcing** 📜
- Armazenamento de todos os eventos
- Histórico completo de mudanças
- Auditoria nativa

### **5. Repository Pattern** 🗄️
- Abstração de acesso a dados
- Interfaces no Domain
- Implementação na Infrastructure

### **6. Mediator Pattern** 📡
- Desacoplamento via MediatR
- Handlers centralizados
- Pipeline de behaviors

### **7. Dependency Injection** 💉
- Inversão de controle
- Lifetime management
- Configuração no Startup

---

## 🔍 **Estrutura de Arquivos:**

```
C:\Projetos\CRM\
│
├── CRM.sln / CRM.slnx               # Solution principal
│
├── CRM.Domain/                      # Camada de Domínio
│   ├── Entities/
│   │   └── Customer.cs
│   ├── ValueObjects/
│   │   ├── CPF.cs
│   │   ├── CNPJ.cs
│   │   ├── Email.cs
│   │   ├── Phone.cs
│   │   └── Address.cs
│   ├── Events/
│   │   └── CustomerEvents.cs
│   ├── Enums/
│   │   └── PersonType.cs
│   ├── Repositories/
│   │   ├── ICustomerRepository.cs
│   │   └── IEventStore.cs
│   └── Common/
│       ├── Entity.cs
│       ├── ValueObject.cs
│       └── IDomainEvent.cs
│
├── CRM.Application/                 # Camada de Aplicação
│   ├── Commands/
│   │   └── Customers/
│   │       ├── CreateNaturalPerson/
│   │       ├── CreateLegalEntity/
│   │       ├── UpdateCustomer/
│   │       ├── ActivateCustomer/
│   │       └── DeactivateCustomer/
│   ├── Queries/
│   │   ├── Customers/
│   │   │   ├── GetAllCustomers/
│   │   │   ├── GetCustomerById/
│   │   │   └── GetCustomerEvents/
│   │   └── ZipCode/
│   │       └── GetZipCodeInfo/
│   ├── DTOs/
│   │   ├── CustomerDto.cs
│   │   └── EventDto.cs
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs
│   ├── Services/
│   │   └── IZipCodeService.cs
│   └── Common/
│       ├── ICommand.cs
│       ├── IQuery.cs
│       └── Result.cs
│
├── CRM.Infrastructure/              # Camada de Infraestrutura
│   ├── Persistence/
│   │   ├── CrmDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── CustomerConfiguration.cs
│   │   │   └── EventStoreConfiguration.cs
│   │   └── Migrations/
│   ├── Repositories/
│   │   ├── CustomerRepository.cs
│   │   └── EventStore.cs
│   └── Services/
│       └── ViaCepService.cs
│
├── CRM.Server/                      # API REST
│   ├── Controllers/
│   │   ├── CustomersController.cs
│   │   └── ZipCodeController.cs
│   ├── Middleware/
│   │   └── GlobalExceptionHandlerMiddleware.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── CRM.Tests/                       # Testes
│   ├── Domain/
│   │   ├── CustomerTests.cs
│   │   └── ValueObjectsTests.cs
│   └── Integration/
│       ├── CustomerIntegrationTests.cs
│       └── CrmWebApplicationFactory.cs
│
├── CRM.AppHost/                     # .NET Aspire
│   └── Program.cs
│
├── frontend/                        # React SPA
│   ├── src/
│   │   ├── App.tsx
│   │   ├── App.css
│   │   └── main.tsx
│   ├── index.html
│   ├── package.json
│   └── vite.config.ts
│
└── docs/                            # Documentação
    ├── adr/                         # Architecture Decision Records
    ├── QUICK_START.md
    ├── GUIA_GRID.md
    ├── GUIA_PAGINACAO.md
    └── SWAGGER_SETUP.md
```

---

## ✅ **Status do Projeto:**

### **Backend:**
- ✅ Todos os projetos adicionados à solution
- ✅ Compilando sem erros
- ✅ Swagger configurado
- ✅ PostgreSQL configurado
- ✅ Event Sourcing implementado
- ✅ CQRS implementado
- ✅ Validações com FluentValidation
- ✅ Integração com ViaCEP
- ✅ Testes unitários e de integração

### **Frontend:**
- ✅ Grid com paginação avançada
- ✅ CRUD completo
- ✅ Busca e filtros
- ✅ Ordenação
- ✅ Design responsivo
- ✅ Integração com backend

### **Infraestrutura:**
- ✅ .NET Aspire configurado
- ✅ Docker support
- ✅ PostgreSQL containerizado
- ✅ Migrations automatizadas

---

## 🎯 **Próximos Passos:**

### **Melhorias Sugeridas:**
1. ✅ ~~Adicionar todos projetos à solution~~ (Concluído!)
2. 📝 Adicionar testes de integração para Event Sourcing
3. 🔐 Implementar autenticação JWT
4. 📊 Adicionar paginação no backend
5. 🔍 Implementar busca avançada
6. 📧 Adicionar notificações por email
7. 📱 Melhorar UX mobile
8. 🚀 Deploy para produção
9. 📈 Monitoramento e telemetria
10. 📖 Documentação de API expandida

---

**Solution CRM completa com todos os projetos organizados!** 🎉📦✨
