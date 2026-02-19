# 📚 Configuração do Swagger - CRM API

## ✅ **Swagger Configurado com Sucesso!**

### **🔧 O que foi feito:**

1. ✅ Instalado `Swashbuckle.AspNetCore` v10.1.4
2. ✅ Configurado `SwaggerGen` no Program.cs
3. ✅ Habilitado Swagger UI

---

## 🚀 **Como Acessar o Swagger:**

### **1. Iniciar o Backend:**

```powershell
cd C:\Projetos\CRM
dotnet run --project CRM.Server
```

### **2. Acessar no navegador:**

**URLs disponíveis:**
- 🌐 **Swagger UI**: http://localhost:5000/swagger
- 📄 **JSON Schema**: http://localhost:5000/swagger/v1/swagger.json
- 🏠 **API Base**: http://localhost:5000/api/customers

---

## 📋 **Endpoints Disponíveis no Swagger:**

### **Customers (Clientes):**

#### **GET** `/api/customers`
- Lista todos os clientes
- Retorna array de CustomerDto

#### **GET** `/api/customers/{id}`
- Busca cliente por ID
- Parâmetro: `id` (GUID)

#### **POST** `/api/customers/natural-person`
- Cria Pessoa Física
- Body: CreateNaturalPersonCommand

#### **POST** `/api/customers/legal-entity`
- Cria Pessoa Jurídica
- Body: CreateLegalEntityCommand

#### **PUT** `/api/customers/{id}`
- Atualiza cliente
- Body: UpdateCustomerCommand

#### **PUT** `/api/customers/{id}/activate`
- Ativa cliente
- Parâmetro: `id` (GUID)

#### **PUT** `/api/customers/{id}/deactivate`
- Desativa cliente
- Parâmetro: `id` (GUID)

#### **GET** `/api/customers/{id}/events`
- Lista eventos do cliente (Event Sourcing)
- Parâmetro: `id` (GUID)

### **ZipCode (CEP):**

#### **GET** `/api/zipcode/{zipCode}`
- Busca informações de CEP
- Parâmetro: `zipCode` (string, 8 dígitos)
- Integração: ViaCEP

---

## 🎨 **Interface do Swagger UI:**

### **Recursos:**
- ✅ **Testável**: Teste todos endpoints direto do navegador
- ✅ **Documentação**: Descrição completa de cada endpoint
- ✅ **Schemas**: Modelos JSON detalhados
- ✅ **Try it out**: Execute requisições em tempo real
- ✅ **Response samples**: Exemplos de resposta

### **Como testar:**
1. Acesse http://localhost:5000/swagger
2. Clique no endpoint desejado
3. Clique em "Try it out"
4. Preencha os parâmetros
5. Clique em "Execute"
6. Veja a resposta

---

## 📦 **Exemplo de Requisição - Criar Pessoa Física:**

```http
POST /api/customers/natural-person
Content-Type: application/json

{
  "name": "João Silva",
  "cpf": "12345678909",
  "birthDate": "1990-01-15",
  "phone": "11987654321",
  "email": "joao@email.com",
  "address": {
    "zipCode": "01310100",
    "street": "Avenida Paulista",
    "number": "1000",
    "complement": "Apto 101",
    "neighborhood": "Bela Vista",
    "city": "São Paulo",
    "state": "SP"
  }
}
```

**Resposta 200 OK:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "personType": 1,
  "name": "João Silva",
  "cpf": "123.456.789-09",
  "email": "joao@email.com",
  "phone": "(11) 98765-4321",
  "isActive": true,
  "createdAt": "2025-02-19T10:30:00Z"
}
```

---

## 🔧 **Configuração no Program.cs:**

```csharp
// AddSwaggerGen
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CRM API",
        Version = "v1",
        Description = "API para gerenciamento de clientes - Sistema CRM com CQRS e Event Sourcing"
    });
});

// UseSwagger + UseSwaggerUI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CRM API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "CRM API - Documentação";
});
```

---

## 🐛 **Troubleshooting:**

### **Problema: 404 em /swagger**

**Solução:**
1. Verifique se o projeto está rodando: `dotnet run --project CRM.Server`
2. Acesse: http://localhost:5000/swagger (não http://localhost:5173)
3. Confirme que Swashbuckle está instalado: `dotnet list CRM.Server package`

### **Problema: Porta diferente**

**Solução:**
- Verifique em `CRM.Server/Properties/launchSettings.json`
- Ou use a porta exibida no console ao iniciar

### **Problema: CORS**

**Solução:**
- CORS já está configurado com `AllowAll`
- Frontend pode consumir de qualquer origem

---

## 📊 **Arquitetura da API:**

```
┌─────────────────────────────────────────────────┐
│          Swagger UI (http://localhost:5000)     │
├─────────────────────────────────────────────────┤
│              CustomersController                 │
│              ZipCodeController                   │
├─────────────────────────────────────────────────┤
│            MediatR (CQRS Pattern)               │
│   Commands ←→ Handlers ←→ Queries               │
├─────────────────────────────────────────────────┤
│          Domain Layer (DDD + Events)            │
├─────────────────────────────────────────────────┤
│     Infrastructure (EF Core + PostgreSQL)       │
└─────────────────────────────────────────────────┘
```

---

## 🎯 **Funcionalidades Testáveis no Swagger:**

### **1. CRUD Completo:**
- ✅ Criar PF/PJ
- ✅ Listar todos
- ✅ Buscar por ID
- ✅ Atualizar
- ✅ Ativar/Desativar

### **2. Event Sourcing:**
- ✅ Ver histórico de eventos de cada cliente

### **3. Integração Externa:**
- ✅ Buscar CEP (ViaCEP)

### **4. Validações:**
- ✅ FluentValidation automático
- ✅ Mensagens de erro detalhadas

---

## 📈 **Vantagens do Swagger:**

1. ✅ **Documentação automática** - sempre atualizada
2. ✅ **Interface interativa** - teste sem Postman
3. ✅ **Padrão OpenAPI 3.0** - amplamente adotado
4. ✅ **Geração de clientes** - TypeScript, C#, etc
5. ✅ **Onboarding rápido** - novos desenvolvedores entendem API rapidamente

---

## 🚀 **Próximos Passos:**

### **Para produção:**
1. Adicionar autenticação (JWT/OAuth)
2. Limitar Swagger apenas para Development
3. Adicionar exemplos XML nos comentários
4. Configurar OperationId customizados
5. Adicionar tags e agrupamentos

### **Melhorias:**
```csharp
// Adicionar autenticação
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
{
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT"
});

// Incluir comentários XML
c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "CRM.Server.xml"));
```

---

**Sistema CRM com Swagger UI totalmente funcional!** 🎉📚✨
