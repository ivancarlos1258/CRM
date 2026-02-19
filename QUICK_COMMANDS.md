# 🚀 CRM Solution - Comandos Rápidos

## ✅ **Solution Configurada com Sucesso!**

---

## 📦 **Abrir Solution:**

### **Visual Studio:**
```powershell
.\open-solution.ps1
```

**Ou abrir manualmente:**
```
C:\Projetos\CRM\CRM.slnx
```

---

## 🏗️ **Comandos Build:**

### **Compilar tudo:**
```powershell
dotnet build
```

### **Restaurar pacotes:**
```powershell
dotnet restore
```

### **Limpar build:**
```powershell
dotnet clean
```

### **Executar testes:**
```powershell
dotnet test
```

---

## 🚀 **Executar Aplicação:**

### **Opção 1 - .NET Aspire (Recomendado):**
```powershell
dotnet run --project CRM.AppHost
```
**Acesse:** https://localhost:17265 (Dashboard)

### **Opção 2 - Backend + Swagger:**
```powershell
.\start-backend.ps1
```
**Acesse:** http://localhost:5000/swagger

### **Opção 3 - Frontend:**
```powershell
cd frontend
npm run dev
```
**Acesse:** http://localhost:5173

---

## 📋 **Ver Projetos:**

```powershell
dotnet sln list
```

**Output:**
```
Projetos
--------
CRM.AppHost\CRM.AppHost.csproj
CRM.Application\CRM.Application.csproj
CRM.Domain\CRM.Domain.csproj
CRM.Infrastructure\CRM.Infrastructure.csproj
CRM.Server\CRM.Server.csproj
CRM.Tests\CRM.Tests.csproj
frontend\frontend.esproj
```

---

## 📚 **Documentação:**

- 📄 `SOLUTION_COMPLETE.md` - Status completo
- 📄 `SOLUTION_STRUCTURE.md` - Estrutura detalhada
- 📄 `SWAGGER_SETUP.md` - Swagger UI
- 📄 `QUICK_START.md` - Início rápido
- 📄 `README.md` - Visão geral

---

## 🎯 **URLs Importantes:**

| Serviço | URL |
|---------|-----|
| 🌐 API | http://localhost:5000 |
| 📚 Swagger | http://localhost:5000/swagger |
| ⚛️ Frontend | http://localhost:5173 |
| 📊 Aspire Dashboard | https://localhost:17265 |

---

## ✅ **Status:**

- ✅ 7 projetos na solution
- ✅ Compilação bem-sucedida
- ✅ Backend funcionando
- ✅ Frontend funcionando
- ✅ Swagger operacional
- ✅ PostgreSQL configurado
- ✅ Testes passando

**Sistema pronto para uso!** 🎉🚀✨
