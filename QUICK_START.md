# 🚀 Guia Rápido de Execução

## ⚡ Início Rápido (1 comando)

```bash
docker-compose up --build
```

✅ Aguarde ~2 minutos para build e inicialização  
✅ API disponível em: **http://localhost:5000**  
✅ Swagger UI: **http://localhost:5000/swagger**

---

## 📝 Testar a API

### 1. Criar Pessoa Física
```bash
curl -X POST http://localhost:5000/api/customers/natural-person \
  -H "Content-Type: application/json" \
  -d '{
    "name": "João Silva",
    "cpf": "12345678909",
    "birthDate": "1990-05-15",
    "phone": "11987654321",
    "email": "joao@example.com",
    "address": {
      "zipCode": "01310100",
      "street": "Av. Paulista",
      "number": "1578",
      "complement": null,
      "neighborhood": "Bela Vista",
      "city": "São Paulo",
      "state": "SP"
    }
  }'
```

### 2. Consultar CEP
```bash
curl http://localhost:5000/api/zipcode/01310100
```

### 3. Listar Clientes
```bash
curl http://localhost:5000/api/customers
```

### 4. Ver Eventos de Auditoria
```bash
# Substitua {id} pelo ID do cliente criado
curl http://localhost:5000/api/customers/{id}/events
```

---

## 🧪 Executar Testes

```bash
dotnet test
```

Testes incluem:
- ✅ Testes unitários do domínio (Value Objects, Entities)
- ✅ Testes de integração (fluxo completo E2E)

---

## 🐘 Acessar PostgreSQL

```bash
docker exec -it crm-postgres psql -U postgres -d crm
```

Queries úteis:
```sql
-- Ver clientes
SELECT * FROM "Customers";

-- Ver eventos de auditoria
SELECT * FROM "EventStore" ORDER BY "OccurredAt" DESC;

-- Ver último evento
SELECT * FROM "EventStore" 
WHERE "AggregateId" = 'seu-customer-id' 
ORDER BY "OccurredAt" DESC LIMIT 1;
```

---

## 🛑 Parar Tudo

```bash
docker-compose down
```

Para limpar volumes (apagar dados):
```bash
docker-compose down -v
```

---

## 📚 Documentação Completa

- [README.md](README.md) - Documentação completa
- [docs/adr/](docs/adr/) - Architecture Decision Records

---

## 🔧 Desenvolvimento Local (sem Docker)

1. **Subir PostgreSQL:**
```bash
docker run --name crm-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=crm \
  -p 5432:5432 \
  -d postgres:17
```

2. **Restaurar e rodar:**
```bash
dotnet restore
dotnet run --project CRM.Server
```

3. **Migrations (se necessário):**
```bash
dotnet ef database update --project CRM.Infrastructure --startup-project CRM.Server
```

---

## ✅ Checklist de Funcionalidades

| Funcionalidade | Status | Endpoint |
|---|---|---|
| Criar Pessoa Física | ✅ | POST /api/customers/natural-person |
| Criar Pessoa Jurídica | ✅ | POST /api/customers/legal-entity |
| Atualizar Cliente | ✅ | PUT /api/customers/{id} |
| Buscar por ID | ✅ | GET /api/customers/{id} |
| Listar Clientes | ✅ | GET /api/customers |
| Histórico de Eventos | ✅ | GET /api/customers/{id}/events |
| Consultar CEP | ✅ | GET /api/zipcode/{cep} |

---

## 🎯 Validações Implementadas

- ✅ CPF: Algoritmo de dígitos verificadores + unicidade
- ✅ CNPJ: Algoritmo de dígitos verificadores + unicidade
- ✅ Email: RFC 5322 + unicidade
- ✅ Idade: Mínimo 18 anos para PF
- ✅ Inscrição Estadual: Obrigatória ou isenta para PJ
- ✅ CEP: 8 dígitos + integração ViaCEP

---

## 💡 Dicas

### Swagger UI
Acesse http://localhost:5000/swagger para documentação interativa e testar endpoints diretamente no navegador.

### Logs
Os logs estruturados são exibidos no console:
```
[10:30:15 INF] Creating natural person customer: João Silva
[10:30:15 INF] Natural person created successfully: abc123...
```

### Event Sourcing
Todos os eventos são persistidos e podem ser consultados:
- CustomerCreatedEvent
- CustomerUpdatedEvent (com dados antigos e novos)
- CustomerDeactivatedEvent
- CustomerActivatedEvent

---

## 🆘 Problemas Comuns

### Porta 5432 já em uso
```bash
# Ver o que está usando a porta
netstat -ano | findstr :5432

# Parar PostgreSQL local se estiver rodando
# Ou usar outra porta no docker-compose.yml
```

### Migrations não aplicadas
```bash
dotnet ef database update --project CRM.Infrastructure --startup-project CRM.Server
```

### Docker build lento
O primeiro build é mais lento (download de imagens). Builds subsequentes usam cache.

---

## 📞 Contato

Em caso de dúvidas, consulte:
- README.md completo
- ADRs em docs/adr/
- Código-fonte (bem documentado)
