# 🎯 Guia de Teste - Frontend CRM

## ✅ **Sistema Completo Implementado**

### **Backend (.NET 10)**
- ✅ Pessoa Física (Natural Person)
- ✅ Pessoa Jurídica (Legal Entity)
- ✅ Event Sourcing + CQRS
- ✅ Validações (CPF, CNPJ, Email, IE)
- ✅ Integração ViaCEP

### **Frontend (React + TypeScript)**
- ✅ Cadastro Pessoa Física
- ✅ Cadastro Pessoa Jurídica
- ✅ Listagem de clientes
- ✅ Autocomplete de CEP
- ✅ Interface responsiva

---

## 🚀 **Como Rodar**

### **1. Backend**
```powershell
cd C:\Projetos\CRM

# Rodar API
dotnet run --project CRM.Server
```

API disponível em: **http://localhost:5000**

### **2. Frontend**
```powershell
cd C:\Projetos\CRM\frontend

# Instalar dependências (primeira vez)
npm install

# Rodar
npm run dev
```

Frontend disponível em: **http://localhost:5173**

---

## 📋 **Testes Manuais**

### **Teste 1: Cadastrar Pessoa Física**

1. Acesse http://localhost:5173
2. Clique em "**+ Novo Cliente**"
3. Aba "**👤 Pessoa Física**" deve estar ativa
4. Preencha:
   - Nome: `João Silva`
   - CPF: `12345678909`
   - Data Nascimento: (escolha data com 18+ anos)
   - Telefone: `11987654321`
   - Email: `joao@example.com`
   - CEP: `01310100` (deve autocomplete)
   - Número: `1578`
5. Clique em "**Salvar Cliente**"
6. Cliente deve aparecer na lista com badge "**👤 PF**"

### **Teste 2: Cadastrar Pessoa Jurídica**

1. Clique em "**+ Novo Cliente**"
2. Clique na aba "**🏢 Pessoa Jurídica**"
3. Preencha:
   - Razão Social: `Empresa XYZ Ltda`
   - CNPJ: `12345678000195`
   - Data Fundação: (qualquer data passada)
   - Telefone: `1140041000`
   - Email: `contato@empresa.com`
   - CEP: `01310100`
   - Número: `1000`
   - IE: `123456789` OU marque "Isento"
4. Clique em "**Salvar Cliente**"
5. Cliente deve aparecer com badge "**🏢 PJ**" e mostrar CNPJ

### **Teste 3: Validação de Idade**

1. Tente cadastrar PF com menos de 18 anos
2. Deve mostrar erro: "Cliente deve ter no mínimo 18 anos"

### **Teste 4: Validação de IE**

1. Cadastre PJ sem IE e sem marcar "Isento"
2. Deve dar erro
3. Marque "Isento" → deve funcionar

### **Teste 5: Autocomplete de CEP**

1. Digite CEP: `01310100`
2. Campos devem preencher automaticamente:
   - Logradouro: Avenida Paulista
   - Bairro: Bela Vista
   - Cidade: São Paulo
   - UF: SP

### **Teste 6: CPF/CNPJ Duplicado**

1. Tente cadastrar cliente com mesmo CPF
2. Deve dar erro: "CPF já cadastrado"
3. Mesmo para CNPJ

---

## 🔍 **Ver Dados no Banco**

### **PostgreSQL**
```sql
-- Ver clientes
SELECT 
  "Id", 
  "PersonType", 
  "Name", 
  "Cpf", 
  "Cnpj", 
  "IsActive" 
FROM "Customers";

-- Ver eventos de auditoria
SELECT 
  "EventType", 
  "AggregateId", 
  "OccurredAt",
  "EventData"
FROM "EventStore" 
ORDER BY "OccurredAt" DESC;
```

---

## 📊 **Diferenças PF vs PJ**

| Campo | Pessoa Física | Pessoa Jurídica |
|-------|---------------|-----------------|
| **Nome** | Nome Completo | Razão Social |
| **Documento** | CPF (11 dígitos) | CNPJ (14 dígitos) |
| **Data** | Data Nascimento | Data Fundação |
| **Validação** | Idade ≥ 18 anos | - |
| **IE** | Não tem | Obrigatória ou Isenta |
| **Badge** | 👤 PF (azul) | 🏢 PJ (amarelo) |

---

## 🎨 **Features da UI**

### **Abas de Tipo**
- Clique entre "Pessoa Física" e "Pessoa Jurídica"
- Formulário muda dinamicamente

### **Badges**
- **👤 PF** (azul) = Pessoa Física
- **🏢 PJ** (amarelo) = Pessoa Jurídica
- **Ativo** (verde) / **Inativo** (vermelho)

### **Autocomplete CEP**
- Mostra "(buscando...)" enquanto consulta
- Preenche automaticamente endereço

### **Validações em Tempo Real**
- CPF: apenas números, max 11
- CNPJ: apenas números, max 14
- Telefone: apenas números, max 11
- UF: converte para maiúscula, max 2

---

## 🐛 **Solução de Problemas**

### **Erro: "Failed to fetch"**
✅ Certifique-se que o backend está rodando em http://localhost:5000

### **CEP não autocompleta**
✅ Verifique conexão com internet (ViaCEP é externo)
✅ Teste manualmente: http://localhost:5000/api/zipcode/01310100

### **Erro ao salvar**
✅ Abra DevTools (F12) → Console
✅ Veja erro específico
✅ Verifique se backend está rodando

### **Lista vazia**
✅ Verifique se PostgreSQL está rodando
✅ Veja logs do backend

---

## 📞 **Endpoints da API**

### **Criar Pessoa Física**
```http
POST /api/customers/natural-person
Content-Type: application/json

{
  "name": "João Silva",
  "cpf": "12345678909",
  "birthDate": "1990-05-15",
  "phone": "11987654321",
  "email": "joao@example.com",
  "address": {
    "zipCode": "01310100",
    "street": "Av. Paulista",
    "number": "1578",
    "neighborhood": "Bela Vista",
    "city": "São Paulo",
    "state": "SP"
  }
}
```

### **Criar Pessoa Jurídica**
```http
POST /api/customers/legal-entity
Content-Type: application/json

{
  "name": "Empresa XYZ Ltda",
  "cnpj": "12345678000195",
  "foundationDate": "2015-03-20",
  "phone": "1140041000",
  "email": "contato@empresa.com",
  "address": {
    "zipCode": "01310100",
    "street": "Av. Paulista",
    "number": "1000",
    "neighborhood": "Bela Vista",
    "city": "São Paulo",
    "state": "SP"
  },
  "stateRegistration": "123456789",
  "isStateRegistrationExempt": false
}
```

### **Listar Clientes**
```http
GET /api/customers
GET /api/customers?onlyActive=true
```

---

## ✅ **Checklist Final**

- ✅ Backend rodando na porta 5000
- ✅ Frontend rodando na porta 5173
- ✅ PostgreSQL configurado e rodando
- ✅ Pode cadastrar Pessoa Física
- ✅ Pode cadastrar Pessoa Jurídica
- ✅ Autocomplete de CEP funciona
- ✅ Validações funcionando
- ✅ Lista mostra todos os clientes
- ✅ Badges corretos (PF/PJ, Ativo/Inativo)

**Sistema está completo e funcionando!** 🎉
