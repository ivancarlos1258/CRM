# ADR 004: Event Sourcing para Auditabilidade

## Status
Aceito

## Contexto
O CRM é um sistema crítico que precisa de **auditabilidade completa**:
- Quem alterou o quê?
- Quando foi alterado?
- Qual era o valor anterior?
- Histórico completo de mudanças

Requisitos regulatórios (LGPD, compliance) exigem rastreabilidade.

## Decisão
Implementar Event Sourcing para manter histórico imutável de todas as alterações em clientes.

### Estrutura

**Domain Events**
```csharp
- CustomerCreatedEvent
- CustomerUpdatedEvent (com OldData e NewData)
- CustomerDeactivatedEvent
- CustomerActivatedEvent
```

**Event Store**
- Tabela PostgreSQL com JSONB
- Campos: EventId, AggregateId, EventType, EventData, UserId, OccurredAt
- Índices em AggregateId e OccurredAt para queries rápidas

### Fluxo
1. Domínio emite evento ao modificar estado
2. Handler salva entidade no repositório
3. Handler persiste eventos no Event Store
4. Eventos nunca são deletados (imutáveis)

## Razões

### Por que Event Sourcing?

**Auditabilidade**
- ✅ Histórico completo de mudanças
- ✅ "Quem fez o quê e quando"
- ✅ Valores anteriores e novos
- ✅ Impossível alterar o passado (imutável)

**Compliance**
- ✅ LGPD: rastreabilidade de alterações
- ✅ Auditorias regulatórias
- ✅ Debugging: reproduzir estado em qualquer momento

**Insights de Negócio**
- ✅ Analytics: padrões de mudança
- ✅ Relatórios: quem mais altera dados?
- ✅ Detecção de anomalias

## Implementação

### Domain Event Interface
```csharp
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    Guid AggregateId { get; }
    string EventType { get; }
}
```

### Entity Base Class
```csharp
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
```

### Event Store com JSONB
```sql
CREATE TABLE EventStore (
    Id BIGSERIAL PRIMARY KEY,
    EventId UUID NOT NULL,
    AggregateId UUID NOT NULL,
    EventType VARCHAR(200) NOT NULL,
    EventData JSONB NOT NULL,  -- Permite queries no JSON
    UserId VARCHAR(100) NOT NULL,
    OccurredAt TIMESTAMP NOT NULL
);

CREATE INDEX idx_aggregate ON EventStore(AggregateId);
CREATE INDEX idx_occurred ON EventStore(OccurredAt);
```

### Query de Auditoria
```csharp
GET /api/customers/{id}/events
// Retorna todos os eventos daquele cliente
```

## Alternativas Consideradas

**Audit Tables Tradicionais**
- ✅ Mais simples
- ❌ Menos expressivo
- ❌ Dificulta reconstrução de estado
- ❌ Campos de audit em cada tabela (duplicação)

**Change Data Capture (CDC)**
- ✅ Automático
- ❌ Nível muito baixo (SQL)
- ❌ Não captura intenção de negócio
- ❌ Específico de banco

**Event Store Dedicado (EventStoreDB)**
- ✅ Feito para Event Sourcing
- ✅ Recursos avançados (projeções, streams)
- ❌ Infraestrutura adicional
- ❌ Curva de aprendizado
- ❌ Over-engineering para MVP

## Consequências

### Positivas
- **Auditoria Completa**: Nunca perdemos histórico
- **Debugging**: Reproduzir bugs é mais fácil
- **Compliance**: Atende requisitos legais
- **Temporal Queries**: "Como estava o cliente em X data?"
- **Event-Driven**: Base para integrações futuras

### Negativas
- **Espaço em Disco**: Eventos crescem indefinidamente
- **Complexidade**: Mais código para manter
- **Performance**: Queries temporais podem ser lentas
- **GDPR/LGPD**: Direito ao esquecimento é complicado

### Mitigações
- **Espaço**: Compressão, arquivamento de eventos antigos
- **Performance**: Índices apropriados, cache de snapshots
- **GDPR**: Anonimização ao invés de delete (manter estrutura do evento)

## Event Store vs Event Streaming

**Nosso Event Store:**
- ✅ Simples: tabela PostgreSQL
- ✅ Transacional: mesmo banco
- ✅ Queries SQL normais
- ❌ Não é pub/sub

**Para Event Streaming (futuro):**
- Considerar RabbitMQ/Kafka para integrações
- Event Store permanece como "source of truth"
- Publicar eventos para bus após salvar

## Exemplo de Auditoria

```json
{
  "eventId": "uuid...",
  "aggregateId": "customer-id...",
  "eventType": "CustomerUpdatedEvent",
  "eventData": {
    "customerName": "João Silva",
    "personType": "NaturalPerson",
    "oldData": {
      "email": "joao.old@example.com",
      "phone": "11987654321"
    },
    "newData": {
      "email": "joao.new@example.com",
      "phone": "11987654322"
    }
  },
  "userId": "admin@system",
  "occurredAt": "2024-02-18T10:30:00Z"
}
```

## Evolução Futura
- Snapshots para performance (reconstruir estado sem replay completo)
- Event Upcasting para migração de eventos antigos
- Projeções materializadas para queries complexas
- Integração com message bus para notificações
