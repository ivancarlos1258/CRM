# Script para iniciar o Backend CRM com Swagger

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "🚀 Iniciando CRM Backend com Swagger" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "📦 Restaurando dependências..." -ForegroundColor Yellow
dotnet restore

Write-Host ""
Write-Host "🔨 Compilando projeto..." -ForegroundColor Yellow
dotnet build --no-restore

Write-Host ""
Write-Host "✅ Iniciando servidor..." -ForegroundColor Green
Write-Host ""
Write-Host "🌐 URLs disponíveis:" -ForegroundColor Cyan
Write-Host "   - API: http://localhost:5000" -ForegroundColor White
Write-Host "   - Swagger UI: http://localhost:5000/swagger" -ForegroundColor Yellow
Write-Host ""
Write-Host "Pressione Ctrl+C para parar o servidor" -ForegroundColor Gray
Write-Host ""

dotnet run --project CRM.Server
