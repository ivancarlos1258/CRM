# 🐛 Guia de Troubleshooting - Erro 404 no Deactivate/Activate

## ✅ **Checklist de Verificação:**

### **1. Backend está rodando?**

```powershell
# Ver se processo está rodando
Get-Process -Name "CRM.Server" -ErrorAction SilentlyContinue

# Testar se porta 5000 está aberta
Test-NetConnection -ComputerName localhost -Port 5000
```

**Se NÃO estiver rodando:**
```powershell
cd C:\Projetos\CRM
dotnet run --project CRM.Server
```

---

### **2. Testar endpoints diretamente**

**PowerShell:**
```powershell
# Listar todos os clientes
$response = Invoke-WebRequest -Uri "http://localhost:5000/api/customers" -Method GET
$customers = $response.Content | ConvertFrom-Json
Write-Host "Clientes: $($customers.Count)"

# Pegar ID do primeiro cliente
$customerId = $customers[0].id
Write-Host "ID do primeiro cliente: $customerId"

# Testar desativar
$deactivateUrl = "http://localhost:5000/api/customers/$customerId/deactivate"
Write-Host "Testando: $deactivateUrl"

try {
    $result = Invoke-WebRequest -Uri $deactivateUrl -Method PUT -ContentType "application/json"
    Write-Host "✅ SUCESSO: $($result.StatusCode)"
    Write-Host $result.Content
} catch {
    Write-Host "❌ ERRO: $($_.Exception.Message)"
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)"
}
```

**curl (se tiver instalado):**
```bash
# Listar clientes
curl http://localhost:5000/api/customers

# Desativar (substitua o GUID)
curl -X PUT http://localhost:5000/api/customers/SEU-GUID-AQUI/deactivate
```

---

### **3. Verificar rotas registradas**

Adicione logging temporário no `Program.cs`:

```csharp
app.UseRouting();

// ADICIONE ISSO PARA DEBUG
app.Use(async (context, next) =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {context.Request.Method} {context.Request.Path}");
    await next();
});

app.MapControllers();
```

---

### **4. Verificar se Commands estão compilados**

```powershell
# Limpar e recompilar
cd C:\Projetos\CRM
dotnet clean
dotnet build

# Ver se os arquivos existem
Test-Path "CRM.Application\bin\Debug\net10.0\CRM.Application.dll"
```

---

### **5. Verificar logs do backend**

Quando rodar `dotnet run --project CRM.Server`, procure por:

```
✅ SUCESSO:
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5000

❌ PROBLEMA:
Unhandled exception. System.InvalidOperationException...
```

---

### **6. Testar com Swagger**

1. Acesse: `http://localhost:5000/swagger`
2. Expanda `PUT /api/customers/{id}/deactivate`
3. Clique em "Try it out"
4. Insira um GUID válido
5. Execute

**Se der 404 no Swagger também** → Problema no backend
**Se funcionar no Swagger mas não no frontend** → Problema no frontend/proxy

---

### **7. Verificar proxy do Vite**

`frontend/vite.config.ts` deve ter:

```typescript
export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
        secure: false
      }
    }
  }
})
```

---

### **8. Teste Manual Completo**

#### **Passo 1: Criar cliente**
```powershell
$body = @{
    name = "Cliente Teste"
    cpf = "12345678909"
    birthDate = "1990-01-01"
    phone = "11987654321"
    email = "teste@example.com"
    address = @{
        zipCode = "01310100"
        street = "Av. Paulista"
        number = "1000"
        neighborhood = "Bela Vista"
        city = "São Paulo"
        state = "SP"
    }
} | ConvertTo-Json

$created = Invoke-RestMethod -Uri "http://localhost:5000/api/customers/natural-person" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"

Write-Host "✅ Cliente criado: $($created.id)"
$customerId = $created.id
```

#### **Passo 2: Desativar**
```powershell
$result = Invoke-RestMethod -Uri "http://localhost:5000/api/customers/$customerId/deactivate" `
    -Method PUT `
    -ContentType "application/json"

Write-Host "✅ Cliente desativado. IsActive: $($result.isActive)"
```

#### **Passo 3: Verificar**
```powershell
$customer = Invoke-RestMethod -Uri "http://localhost:5000/api/customers/$customerId" -Method GET
Write-Host "Status atual - IsActive: $($customer.isActive)"
```

---

## 🔍 **Causas Comuns do Erro 404:**

| Causa | Como Verificar | Solução |
|---|---|---|
| **Backend não rodando** | `Test-NetConnection localhost -Port 5000` | `dotnet run --project CRM.Server` |
| **Commands não compilados** | Verificar bin/Debug | `dotnet clean && dotnet build` |
| **GUID inválido** | Verificar se é GUID válido | Usar GUID de cliente existente |
| **Proxy Vite não funcionando** | Testar diretamente `localhost:5000` | Reiniciar Vite com `npm run dev` |
| **CORS bloqueando** | Ver console do browser (F12) | Verificar política CORS no backend |
| **Rota mal formatada** | Ver logs do backend | Conferir URL exata |

---

## 📞 **Debug Avançado**

### **Adicionar logging detalhado no Controller:**

```csharp
[HttpPut("{id:guid}/deactivate")]
public async Task<IActionResult> Deactivate(Guid id)
{
    Console.WriteLine($"[DEBUG] Received Deactivate request for ID: {id}");
    _logger.LogInformation("Deactivating customer: {Id}", id);

    try
    {
        var command = new DeactivateCustomerCommand(id);
        Console.WriteLine($"[DEBUG] Command created: {command}");
        
        var result = await _mediator.Send(command);
        Console.WriteLine($"[DEBUG] Result: Success={result.IsSuccess}, Error={result.Error}");

        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to deactivate customer {Id}: {Error}", id, result.Error);
            return BadRequest(new { error = result.Error });
        }

        _logger.LogInformation("Customer deactivated successfully: {Id}", id);
        return Ok(result.Data);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DEBUG] Exception: {ex.Message}");
        throw;
    }
}
```

---

## ✅ **Solução Rápida**

Execute em ordem:

```powershell
# 1. Parar tudo
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*" -or $_.ProcessName -like "*node*"} | Stop-Process -Force

# 2. Limpar
cd C:\Projetos\CRM
dotnet clean
Remove-Item -Recurse -Force frontend/node_modules/.vite -ErrorAction SilentlyContinue

# 3. Compilar
dotnet build

# 4. Terminal 1 - Backend
dotnet run --project CRM.Server

# 5. Terminal 2 - Frontend (nova janela)
cd frontend
npm run dev
```

---

**Se ainda assim der erro 404, execute o teste manual completo e me envie o output!** 🚀
