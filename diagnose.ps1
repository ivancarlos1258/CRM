# Script de Diagnóstico Completo - Erro 500

Write-Host "🔍 Diagnóstico do Sistema CRM" -ForegroundColor Cyan
Write-Host "=" * 60
Write-Host ""

# 1. Verificar PostgreSQL
Write-Host "1️⃣ Verificando PostgreSQL..." -ForegroundColor Yellow
try {
    $pgService = Get-Service -Name "postgresql*" -ErrorAction SilentlyContinue
    if ($pgService) {
        Write-Host "✅ PostgreSQL instalado: $($pgService.Name) - Status: $($pgService.Status)" -ForegroundColor Green
        if ($pgService.Status -ne "Running") {
            Write-Host "⚠️ PostgreSQL NÃO está rodando!" -ForegroundColor Red
            Write-Host "   Execute: Start-Service $($pgService.Name)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "⚠️ PostgreSQL não encontrado como serviço" -ForegroundColor Yellow
        Write-Host "   Verificando se está rodando via Docker..." -ForegroundColor Cyan
        
        $dockerPg = docker ps --filter "ancestor=postgres" --format "{{.Names}}" 2>$null
        if ($dockerPg) {
            Write-Host "✅ PostgreSQL rodando no Docker: $dockerPg" -ForegroundColor Green
        } else {
            Write-Host "❌ PostgreSQL NÃO está rodando!" -ForegroundColor Red
            Write-Host ""
            Write-Host "   🔧 SOLUÇÃO: Inicie o PostgreSQL com Docker:" -ForegroundColor Yellow
            Write-Host "   docker run --name crm-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=crm -p 5432:5432 -d postgres:17" -ForegroundColor Cyan
            exit 1
        }
    }
} catch {
    Write-Host "❌ Erro ao verificar PostgreSQL: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# 2. Testar conexão com PostgreSQL
Write-Host "2️⃣ Testando conexão com PostgreSQL..." -ForegroundColor Yellow
try {
    $connection = Test-NetConnection -ComputerName localhost -Port 5432 -WarningAction SilentlyContinue
    if ($connection.TcpTestSucceeded) {
        Write-Host "✅ Porta 5432 está aberta e aceitando conexões" -ForegroundColor Green
    } else {
        Write-Host "❌ Porta 5432 NÃO está acessível!" -ForegroundColor Red
        Write-Host "   PostgreSQL pode não estar rodando ou está em outra porta" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "❌ Erro ao testar conexão: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# 3. Verificar Connection String
Write-Host "3️⃣ Verificando Connection String..." -ForegroundColor Yellow
$appsettings = Get-Content "CRM.Server/appsettings.json" | ConvertFrom-Json
$connString = $appsettings.ConnectionStrings.CrmDatabase
Write-Host "   Connection String: $connString" -ForegroundColor Cyan

if ($connString -like "*localhost*" -or $connString -like "*127.0.0.1*") {
    Write-Host "✅ Connection String aponta para localhost" -ForegroundColor Green
} else {
    Write-Host "⚠️ Connection String não aponta para localhost" -ForegroundColor Yellow
}
Write-Host ""

# 4. Verificar se migrations foram aplicadas
Write-Host "4️⃣ Verificando Migrations..." -ForegroundColor Yellow
$migrationsFolder = "CRM.Infrastructure/Migrations"
if (Test-Path $migrationsFolder) {
    $migrations = Get-ChildItem $migrationsFolder -Filter "*.cs" | Where-Object { $_.Name -notlike "*Designer*" }
    Write-Host "✅ Migrations encontradas: $($migrations.Count)" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "   🔧 Aplicando migrations..." -ForegroundColor Cyan
    try {
        $output = dotnet ef database update --project CRM.Infrastructure --startup-project CRM.Server 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Migrations aplicadas com sucesso!" -ForegroundColor Green
        } else {
            Write-Host "❌ Erro ao aplicar migrations:" -ForegroundColor Red
            Write-Host $output
            exit 1
        }
    } catch {
        Write-Host "❌ Erro ao executar migrations: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "❌ Pasta de Migrations não encontrada!" -ForegroundColor Red
    Write-Host "   Migrations precisam ser criadas primeiro" -ForegroundColor Yellow
    exit 1
}
Write-Host ""

# 5. Compilar projeto
Write-Host "5️⃣ Compilando projeto..." -ForegroundColor Yellow
try {
    $buildOutput = dotnet build CRM.Server --no-incremental 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Compilação bem-sucedida!" -ForegroundColor Green
    } else {
        Write-Host "❌ Erro na compilação:" -ForegroundColor Red
        Write-Host $buildOutput
        exit 1
    }
} catch {
    Write-Host "❌ Erro ao compilar: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 6. Iniciar backend em modo de teste
Write-Host "6️⃣ Iniciando backend para teste..." -ForegroundColor Yellow
Write-Host "   Aguarde 5 segundos para o backend iniciar..." -ForegroundColor Cyan

$job = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    dotnet run --project CRM.Server --urls "http://localhost:5000" 2>&1
}

Start-Sleep -Seconds 8

# 7. Testar endpoints
Write-Host ""
Write-Host "7️⃣ Testando endpoints..." -ForegroundColor Yellow

# Health check
try {
    $health = Invoke-RestMethod -Uri "http://localhost:5000/api/customers/health" -Method GET -TimeoutSec 5
    Write-Host "✅ Health Check OK!" -ForegroundColor Green
} catch {
    Write-Host "❌ Health Check FALHOU!" -ForegroundColor Red
    Write-Host "   Erro: $($_.Exception.Message)" -ForegroundColor Red
    
    Write-Host ""
    Write-Host "📋 Logs do backend:" -ForegroundColor Yellow
    Write-Host "-" * 60
    Receive-Job -Job $job
    Write-Host "-" * 60
    
    Stop-Job -Job $job
    Remove-Job -Job $job
    exit 1
}

# Listar clientes
try {
    $customers = Invoke-RestMethod -Uri "http://localhost:5000/api/customers" -Method GET -TimeoutSec 5
    Write-Host "✅ GET /api/customers OK! ($($customers.Count) clientes)" -ForegroundColor Green
} catch {
    Write-Host "❌ GET /api/customers FALHOU!" -ForegroundColor Red
    Write-Host "   Erro: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    
    Write-Host ""
    Write-Host "📋 Logs do backend:" -ForegroundColor Yellow
    Write-Host "-" * 60
    Receive-Job -Job $job
    Write-Host "-" * 60
    
    Stop-Job -Job $job
    Remove-Job -Job $job
    exit 1
}

Write-Host ""
Write-Host "=" * 60
Write-Host "✅ TODOS OS TESTES PASSARAM!" -ForegroundColor Green
Write-Host ""
Write-Host "🎉 Sistema está funcionando corretamente!" -ForegroundColor Cyan
Write-Host ""
Write-Host "📝 Próximos passos:" -ForegroundColor Yellow
Write-Host "   1. Backend está rodando em http://localhost:5000" -ForegroundColor Cyan
Write-Host "   2. Abra OUTRO terminal e execute:" -ForegroundColor Cyan
Write-Host "      cd frontend" -ForegroundColor White
Write-Host "      npm run dev" -ForegroundColor White
Write-Host ""

# Manter backend rodando
Write-Host "⏳ Backend permanecerá rodando. Pressione Ctrl+C para parar." -ForegroundColor Yellow
Wait-Job -Job $job
