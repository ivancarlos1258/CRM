# ADR 001: Escolha do PostgreSQL como Banco de Dados

## Status
Aceito

## Contexto
O sistema CRM precisa de um banco de dados robusto, escalável e que suporte tanto dados relacionais quanto recursos avançados como JSONB para o Event Store.

## Decisão
Escolhemos o PostgreSQL 17 como banco de dados principal.

## Razões

### Vantagens
1. **Suporte ACID Completo**: Garantias transacionais essenciais para operações críticas de negócio
2. **JSONB**: Tipo de dados nativo para armazenar eventos em formato JSON com indexação eficiente
3. **Performance**: Excelente performance em consultas complexas e grande volume de dados
4. **Open Source**: Sem custos de licenciamento
5. **Maturidade**: Banco de dados maduro e confiável, usado por empresas globais
6. **Extensibilidade**: Suporte a extensões e tipos personalizados
7. **Conformidade com SQL**: Aderência aos padrões SQL

### Alternativas Consideradas

**MongoDB (NoSQL)**
- ✅ Flexibilidade de schema
- ✅ Bom para Event Sourcing
- ❌ Falta de JOINs complexos
- ❌ Menor garantia transacional (antes da versão 4.0)
- ❌ Menos familiar para a maioria dos desenvolvedores

**SQL Server**
- ✅ Excelente integração com .NET
- ✅ Ferramentas avançadas de monitoramento
- ❌ Custos de licenciamento (versões não-Express)
- ❌ Menos portável entre ambientes

**MySQL**
- ✅ Ampla adoção
- ✅ Performance em operações simples
- ❌ Suporte limitado a JSONB (apenas JSON)
- ❌ Recursos avançados menos robustos

## Consequências

### Positivas
- Event Store nativo com JSONB para auditabilidade completa
- Índices poderosos para queries complexas
- Suporte a transações distribuídas se necessário no futuro
- Facilita a implementação de Read Models (CQRS)

### Negativas
- Requer conhecimento específico de PostgreSQL
- Necessita ajustes finos de performance para grandes volumes
- Backup e replicação requerem configuração adequada

## Notas de Implementação
- Usar Npgsql.EntityFrameworkCore.PostgreSQL para integração com EF Core
- Configurar conexão pooling para otimizar performance
- Implementar migrations para versionamento de schema
- Utilizar JSONB para Event Store com índices apropriados
