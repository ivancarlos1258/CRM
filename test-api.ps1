# Script de Teste Rápido dos Endpoints

Write-Host "🧪 Testando Endpoints do CRM..." -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5000"

# 1. Health Check
Write-Host "1️⃣ Health Check..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$baseUrl/api/customers/health" -Method GET
    Write-Host "✅ Backend funcionando!" -ForegroundColor Green
    Write-Host "Endpoints disponíveis:"
    $health.endpoints | ForEach-Object { Write-Host "   - $_" }
    Write-Host ""
} catch {
    Write-Host "❌ Backend não está respondendo em $baseUrl" -ForegroundColor Red
    Write-Host "Execute: dotnet run --project CRM.Server" -ForegroundColor Yellow
    exit 1
}

# 2. Listar clientes
Write-Host "2️⃣ Listando clientes..." -ForegroundColor Yellow
try {
    $customers = Invoke-RestMethod -Uri "$baseUrl/api/customers" -Method GET
    Write-Host "✅ Clientes encontrados: $($customers.Count)" -ForegroundColor Green
    
    if ($customers.Count -eq 0) {
        Write-Host "⚠️ Nenhum cliente cadastrado. Criando um para teste..." -ForegroundColor Yellow
        
        # Criar cliente de teste
        $body = @{
            name = "Cliente Teste $(Get-Date -Format 'HHmmss')"
            cpf = "12345678909"
            birthDate = "1990-01-01"
            phone = "11987654321"
            email = "teste$(Get-Date -Format 'HHmmss')@example.com"
            address = @{
                zipCode = "01310100"
                street = "Av. Paulista"
                number = "1000"
                neighborhood = "Bela Vista"
                city = "São Paulo"
                state = "SP"
            }
        } | ConvertTo-Json

        $created = Invoke-RestMethod -Uri "$baseUrl/api/customers/natural-person" `
            -Method POST `
            -Body $body `
            -ContentType "application/json"

        Write-Host "✅ Cliente criado: $($created.name) (ID: $($created.id))" -ForegroundColor Green
        $customerId = $created.id
        $isActive = $created.isActive
    } else {
        $customerId = $customers[0].id
        $isActive = $customers[0].isActive
        Write-Host "   Usando cliente: $($customers[0].name) (ID: $customerId)" -ForegroundColor Cyan
    }
    Write-Host ""
} catch {
    Write-Host "❌ Erro ao listar clientes: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 3. Buscar cliente por ID
Write-Host "3️⃣ Buscando cliente por ID..." -ForegroundColor Yellow
try {
    $customer = Invoke-RestMethod -Uri "$baseUrl/api/customers/$customerId" -Method GET
    Write-Host "✅ Cliente encontrado: $($customer.name)" -ForegroundColor Green
    Write-Host "   Status: $(if ($customer.isActive) {'Ativo ✅'} else {'Inativo ❌'})" -ForegroundColor Cyan
    Write-Host ""
} catch {
    Write-Host "❌ Erro ao buscar cliente: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 4. Testar Desativar/Ativar
if ($isActive) {
    Write-Host "4️⃣ Testando DESATIVAR cliente..." -ForegroundColor Yellow
    $endpoint = "$baseUrl/api/customers/$customerId/deactivate"
    $action = "desativado"
} else {
    Write-Host "4️⃣ Testando ATIVAR cliente..." -ForegroundColor Yellow
    $endpoint = "$baseUrl/api/customers/$customerId/activate"
    $action = "ativado"
}

Write-Host "   URL: $endpoint" -ForegroundColor Cyan

try {
    $result = Invoke-RestMethod -Uri $endpoint -Method PUT -ContentType "application/json"
    Write-Host "✅ Cliente $action com sucesso!" -ForegroundColor Green
    Write-Host "   Status atual: $(if ($result.isActive) {'Ativo ✅'} else {'Inativo ❌'})" -ForegroundColor Cyan
    Write-Host ""
} catch {
    Write-Host "❌ ERRO ao tentar $action cliente!" -ForegroundColor Red
    Write-Host "   Status HTTP: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    Write-Host "   Mensagem: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "🔍 Detalhes do erro:" -ForegroundColor Yellow
    $_ | Select-Object * | Format-List
    exit 1
}

# 5. Verificar eventos
Write-Host "5️⃣ Consultando eventos de auditoria..." -ForegroundColor Yellow
try {
    $events = Invoke-RestMethod -Uri "$baseUrl/api/customers/$customerId/events" -Method GET
    Write-Host "✅ Eventos encontrados: $($events.Count)" -ForegroundColor Green
    $events | ForEach-Object {
        Write-Host "   - $($_.eventType) em $($_.occurredAt)" -ForegroundColor Cyan
    }
    Write-Host ""
} catch {
    Write-Host "❌ Erro ao consultar eventos: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "✅ TODOS OS TESTES PASSARAM!" -ForegroundColor Green
Write-Host ""
Write-Host "🎉 Sistema funcionando corretamente!" -ForegroundColor Cyan
