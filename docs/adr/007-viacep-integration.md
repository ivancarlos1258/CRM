# ADR 007: Integração com ViaCEP

## Status
Aceito

## Contexto
O cadastro de clientes exige endereço completo. Para melhorar a experiência do usuário e garantir consistência dos dados, precisamos de uma forma de autocompletar endereços a partir do CEP.

## Decisão
Integrar com a API pública **ViaCEP** (https://viacep.com.br) para consulta de endereços.

## Implementação

### Serviço
```csharp
public class ViaCepService : IZipCodeService
{
    private readonly HttpClient _httpClient;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public async Task<ZipCodeInfoDto?> GetAddressByZipCodeAsync(string zipCode)
    {
        var response = await _retryPolicy.ExecuteAsync(
            async () => await _httpClient.GetAsync($"/ws/{zipCode}/json/"));
        
        return await response.Content.ReadFromJsonAsync<ZipCodeInfoDto>();
    }
}
```

### Endpoint
```http
GET /api/zipcode/01310100

Response 200:
{
  "cep": "01310-100",
  "logradouro": "Avenida Paulista",
  "bairro": "Bela Vista",
  "localidade": "São Paulo",
  "uf": "SP",
  "erro": false
}
```

## Por que ViaCEP?

### Vantagens
- ✅ **Gratuito**: Sem custos de uso
- ✅ **Sem autenticação**: Não requer API key
- ✅ **Confiável**: Dados oficiais dos Correios
- ✅ **Simples**: API REST básica
- ✅ **Brasileiro**: Focado em CEPs brasileiros
- ✅ **Sem rate limit**: Para uso normal

### Alternativas Consideradas

**Google Maps Geocoding API**
- ✅ Mais recursos (coordenadas, validação avançada)
- ❌ Pago após quota grátis (200 USD/mês)
- ❌ Requer API key e billing
- ❌ Over-engineering para nosso caso

**APIs dos Correios**
- ✅ Fonte oficial
- ❌ Instável historicamente
- ❌ Sem garantias de SLA
- ❌ Interface antiga

**Busca Endereço (BrasilAPI)**
- ✅ Gratuito
- ✅ Várias APIs consolidadas
- ✅ Open source
- ⚠️ Menos conhecido/testado

## Fluxo de Uso

### Frontend (Futuro)
```typescript
// Usuário digita CEP
onCepChange(cep: string) {
    if (cep.length === 8) {
        const address = await api.get(`/zipcode/${cep}`);
        
        // Autocompleta campos
        form.street = address.logradouro;
        form.neighborhood = address.bairro;
        form.city = address.localidade;
        form.state = address.uf;
    }
}
```

### Backend
1. Cliente chama `/api/zipcode/01310100`
2. Controller delega para `GetZipCodeInfoQuery`
3. Handler chama `IZipCodeService`
4. `ViaCepService` chama API externa (com retry)
5. Retorna dados para cliente

## Resiliência

### Retry Policy (Polly)
- 3 tentativas
- Backoff exponencial (2s, 4s, 8s)
- Log de cada tentativa

### Tratamento de Erros
```csharp
// CEP não encontrado
{ "erro": true }

// Erro de rede
catch (HttpRequestException ex) {
    _logger.LogError("Error fetching ViaCEP");
    return null;
}
```

## Considerações de Design

### Por que não obrigatória?
A consulta ao ViaCEP é **opcional**. O usuário pode:
- ✅ Usar o autocomplete (recomendado)
- ✅ Digitar manualmente o endereço completo

**Motivo**: ViaCEP pode estar fora do ar. Não podemos bloquear cadastros por isso.

### Validação de CEP
```csharp
// Application Layer (formato)
RuleFor(x => x.ZipCode).NotEmpty();

// Domain Layer (formato + tamanho)
if (cleanZipCode.Length != 8)
    throw new ArgumentException("CEP inválido");
```

### Não persistimos dados do ViaCEP
❌ Não criamos tabela de CEPs
❌ Não fazemos cache local

**Por quê?**
- Dados mudam (novos CEPs, alterações de logradouros)
- ViaCEP é rápido e confiável
- Evita sincronização de dados

## Performance

### Latência Esperada
- ViaCEP: ~100-300ms
- Com retry: até ~8s (caso 3 falhas)

### Cache (Futuro)
```csharp
// Distributed cache (Redis)
var cached = await _cache.GetAsync($"zipcode:{zipCode}");
if (cached != null)
    return cached;

var result = await _viaCepService.GetAddressByZipCodeAsync(zipCode);
await _cache.SetAsync($"zipcode:{zipCode}", result, TimeSpan.FromDays(30));
```

## Testes

### Mock do Serviço
```csharp
var mockZipCodeService = new Mock<IZipCodeService>();
mockZipCodeService
    .Setup(x => x.GetAddressByZipCodeAsync("01310100", It.IsAny<CancellationToken>()))
    .ReturnsAsync(new ZipCodeInfoDto { 
        Cep = "01310-100", 
        Logradouro = "Avenida Paulista" 
    });
```

### Testes de Integração
```csharp
[Fact]
public async Task GetZipCode_WithValidCep_ReturnsAddress()
{
    var response = await _client.GetAsync("/api/zipcode/01310100");
    
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## Limitações

### CEPs Não Encontrados
ViaCEP retorna `{ "erro": true }` para CEPs inexistentes.

**Nossa resposta:**
```http
404 Not Found
{ "error": "CEP não encontrado" }
```

### Sem Validação de Número
ViaCEP retorna logradouro, mas não valida se o número existe.

**Exemplo**: "Av. Paulista, 9999999" → ViaCEP aceita, mas endereço não existe.

**Solução**: Não há validação de número. Responsabilidade do usuário.

## LGPD e Privacidade

### Dados Sensíveis?
❌ CEP não é dado pessoal (LGPD)
✅ OK compartilhar com ViaCEP

### Logs
```csharp
// ✅ OK logar CEP
_logger.LogInformation("Fetching address for zip code: {ZipCode}", cleanZipCode);

// ❌ NÃO logar dados do cliente junto
```

## Evolução Futura

### Fase 2: Validação Avançada
- Verificar se número está no range válido
- Google Maps API para validação completa
- Coordenadas geográficas para mapa

### Fase 3: Cache Distribuído
- Redis para cache de CEPs
- TTL de 30 dias
- Invalidação manual se necessário

### Fase 4: Fallback para BrasilAPI
```csharp
try {
    return await _viaCepService.GetAddressByZipCodeAsync(zipCode);
} catch {
    return await _brasilApiService.GetAddressByZipCodeAsync(zipCode);
}
```

## Referências
- [ViaCEP - Documentação](https://viacep.com.br/)
- [BrasilAPI - Alternativa](https://brasilapi.com.br/)
