# ADR 006: Resiliência com Polly

## Status
Aceito

## Contexto
O sistema integra com serviços externos (ViaCEP) que podem:
- Estar temporariamente indisponíveis
- Responder com latência alta
- Falhar intermitentemente

Precisamos tornar o sistema resiliente a falhas transitórias.

## Decisão
Usar **Polly** para implementar retry policies em chamadas externas.

## Implementação

### ViaCEP com Retry Exponencial
```csharp
_retryPolicy = Policy
    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .Or<HttpRequestException>()
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            _logger.LogWarning(
                "Retry {RetryCount} after {Delay}s due to: {Reason}",
                retryCount,
                timespan.TotalSeconds,
                outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
        });
```

### Fluxo de Retry
```
Tentativa 1 → Falha → Aguarda 2s
Tentativa 2 → Falha → Aguarda 4s
Tentativa 3 → Falha → Aguarda 8s
Tentativa 4 → Retorna erro ao cliente
```

## Razões

### Por que Polly?

**Padrões de Resiliência**
- ✅ Retry (tentativas)
- ✅ Circuit Breaker (proteção contra cascata)
- ✅ Timeout (limites de tempo)
- ✅ Bulkhead (isolamento de recursos)
- ✅ Fallback (plano B)
- ✅ Cache (otimização)

**Flexibilidade**
```csharp
// Combinar políticas
var policy = Policy
    .Wrap(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
```

**Observabilidade**
- Callbacks para logging
- Métricas de falhas/sucessos
- Rastreamento de tentativas

## Alternativas Consideradas

**Retry Manual**
```csharp
// ❌ Código repetitivo
for (int i = 0; i < 3; i++)
{
    try
    {
        return await httpClient.GetAsync(url);
    }
    catch
    {
        if (i == 2) throw;
        await Task.Delay(1000 * (i + 1));
    }
}
```

**Sem Retry**
- ✅ Mais simples
- ❌ Falhas transitórias causam erros desnecessários
- ❌ Má experiência do usuário

## Padrões Implementados

### 1. Retry Policy (Implementado)
**Quando usar**: Falhas transitórias (network glitches, timeouts)

**Configuração**:
- 3 tentativas
- Backoff exponencial (2s, 4s, 8s)
- Log de cada tentativa

### 2. Circuit Breaker (Futuro)
**Quando usar**: Proteger contra serviços completamente indisponíveis

```csharp
var circuitBreakerPolicy = Policy
    .Handle<HttpRequestException>()
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30));
```

**Estados**:
- **Closed**: Funcional (tentativas normais)
- **Open**: Serviço falhou muito → bloqueia chamadas
- **Half-Open**: Testa se serviço voltou

### 3. Timeout Policy (Futuro)
**Quando usar**: Evitar esperas infinitas

```csharp
var timeoutPolicy = Policy
    .TimeoutAsync(TimeSpan.FromSeconds(5));
```

### 4. Fallback (Futuro)
**Quando usar**: Prover resposta alternativa

```csharp
var fallbackPolicy = Policy<ZipCodeInfoDto>
    .Handle<Exception>()
    .FallbackAsync(
        fallbackValue: new ZipCodeInfoDto { Erro = true },
        onFallbackAsync: async (result, context) =>
        {
            _logger.LogWarning("Using fallback for ViaCEP");
        });
```

## Casos de Uso

### ViaCEP (Implementado)
```csharp
public async Task<ZipCodeInfoDto?> GetAddressByZipCodeAsync(string zipCode)
{
    var response = await _retryPolicy.ExecuteAsync(async () =>
        await _httpClient.GetAsync($"/ws/{zipCode}/json/"));
    
    return await response.Content.ReadFromJsonAsync<ZipCodeInfoDto>();
}
```

**Benefício**: Se ViaCEP estiver lento ou com problema momentâneo, sistema tenta novamente automaticamente.

### Logs de Resiliência
```
[10:30:15 WRN] Retry 1 after 2s due to: ServiceUnavailable
[10:30:17 WRN] Retry 2 after 4s due to: ServiceUnavailable
[10:30:21 INF] Successfully fetched address for zip code: 01310100
```

## Configuração Recomendada por Serviço

### APIs Externas (ViaCEP)
- ✅ Retry: 3 tentativas, exponencial
- ✅ Timeout: 5 segundos
- ⏳ Circuit Breaker: 5 falhas → 30s aberto
- ⏳ Fallback: Retornar erro amigável

### Banco de Dados
- ✅ Retry: 2 tentativas, linear (deadlocks)
- ❌ Circuit Breaker: Não (se banco cai, app deve cair)
- ✅ Timeout: Por query

### Message Bus (Futuro)
- ✅ Retry: 5 tentativas, exponencial
- ✅ Circuit Breaker: Proteger contra broker down
- ✅ Bulkhead: Limitar conexões

## Consequências

### Positivas
- **Resiliência**: Sistema tolera falhas transitórias
- **Experiência**: Usuário não vê erros temporários
- **Estabilidade**: Protege contra cascata de falhas
- **Observabilidade**: Logs de tentativas

### Negativas
- **Latência**: Retries aumentam tempo de resposta
- **Complexidade**: Mais lógica para debugar
- **Custos**: Mais requisições = mais uso de recursos

### Mitigações
- Retry apenas para operações idempotentes
- Limitar número de tentativas (3 é razoável)
- Monitorar taxa de retry (se alta, problema sistêmico)
- Circuit breaker para evitar "retry storm"

## Boas Práticas

### ✅ Fazer
- Retry em operações idempotentes (GET)
- Log de cada tentativa
- Backoff exponencial (evita DDoS acidental)
- Timeout nas requisições
- Circuit breaker para proteção

### ❌ Não Fazer
- Retry em POST/PUT sem idempotência
- Retry infinito
- Retry imediato (sem delay)
- Ignorar erros (sempre logar)

## Monitoramento

### Métricas a Acompanhar
- Taxa de retry (% de chamadas que precisaram retry)
- Tentativas médias até sucesso
- Taxa de falha após retries
- Latência P50, P95, P99
- Circuit breaker trips

### Alertas
- 🚨 Taxa de retry > 20%
- 🚨 Circuit breaker aberto > 5 minutos
- 🚨 Taxa de falha após retries > 5%

## Evolução Futura

### Fase 2: Circuit Breaker
```csharp
services.AddHttpClient<IZipCodeService, ViaCepService>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());
```

### Fase 3: Resilience Strategies (.NET 8+)
```csharp
// Microsoft.Extensions.Resilience (mais moderno)
services.AddHttpClient<IZipCodeService, ViaCepService>()
    .AddStandardResilienceHandler();
```

### Fase 4: Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddCheck<ViaCepHealthCheck>("viacep");
```

## Testes

### Unit Test: Retry Policy
```csharp
[Fact]
public async Task Should_Retry_3_Times_On_Failure()
{
    // Mock HttpClient para retornar erro 2x, sucesso na 3ª
    var attempts = 0;
    _mockHandler.Setup(x => x.SendAsync(...))
        .ReturnsAsync(() => {
            attempts++;
            if (attempts < 3)
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

    var result = await _sut.GetAddressByZipCodeAsync("01310100");

    Assert.Equal(3, attempts);
    Assert.NotNull(result);
}
```

## Referências
- [Polly Documentation](https://www.pollydocs.org/)
- [Cloud Design Patterns - Retry](https://learn.microsoft.com/en-us/azure/architecture/patterns/retry)
- [Release It! - Michael Nygard](https://pragprog.com/titles/mnee2/release-it-second-edition/)
