# Script para abrir a Solution no Visual Studio

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "🚀 Abrindo CRM Solution" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$solutionPath = "C:\Projetos\CRM\CRM.slnx"

if (Test-Path $solutionPath) {
    Write-Host "✅ Solution encontrada: $solutionPath" -ForegroundColor Green
    Write-Host "📂 Abrindo no Visual Studio..." -ForegroundColor Yellow
    Write-Host ""
    
    Start-Process $solutionPath
    
    Write-Host "✅ Visual Studio iniciado!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📦 Projetos na solution:" -ForegroundColor Cyan
    Write-Host "   1. CRM.Domain" -ForegroundColor White
    Write-Host "   2. CRM.Application" -ForegroundColor White
    Write-Host "   3. CRM.Infrastructure" -ForegroundColor White
    Write-Host "   4. CRM.Server" -ForegroundColor White
    Write-Host "   5. CRM.Tests" -ForegroundColor White
    Write-Host "   6. CRM.AppHost" -ForegroundColor White
    Write-Host "   7. frontend.esproj" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "❌ Solution não encontrada: $solutionPath" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Criando solution..." -ForegroundColor Yellow
    dotnet sln add CRM.Domain/CRM.Domain.csproj CRM.Application/CRM.Application.csproj CRM.Infrastructure/CRM.Infrastructure.csproj CRM.Server/CRM.Server.csproj CRM.Tests/CRM.Tests.csproj CRM.AppHost/CRM.AppHost.csproj
    Write-Host "✅ Solution criada!" -ForegroundColor Green
    Start-Process $solutionPath
}
