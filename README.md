## Teste Técnico BTG — Desenvolvedor Backend

API de Gerenciamento de Contas (Onboarding) desenvolvida em **.NET 8**, aplicando princípios de **DDD**, **Clean Architecture** e **CQRS** (MediatR). Inclui **Redis** como cache distribuído e um modelo de **eventos** (publish/subscribe) para simular integração com serviços externos (ex.: emissão de cartão e prevenção a fraude).

## Sumário

- [Requisitos do desafio (da especificação)](#requisitos-do-desafio-da-especificação)
- [Tecnologias e ferramentas](#tecnologias-e-ferramentas)
- [Arquitetura](#arquitetura)
- [Como executar](#como-executar)
- [Credenciais padrão (seed)](#credenciais-padrão-seed)
- [Endpoints](#endpoints)
- [Respostas e erros](#respostas-e-erros)
- [Cache (Redis)](#cache-redis)
- [Eventos e mensageria (mock)](#eventos-e-mensageria-mock)
- [Health check e logs](#health-check-e-logs)
- [Testes](#testes)

## Requisitos do desafio (da especificação)

Com base no enunciado do teste técnico:

- **Objetivo**: criar uma API para gerenciar contas do onboarding (CRUD).
- **Dados da conta**: `Id`, `Nome do titular`, `CPF`, `Status` (Ativa/Inativa).
- **Requisitos técnicos**:
  - Notificar outros domínios quando uma conta for **criada/atualizada/deletada** (ex.: antifraude e cartões).
  - Reduzir custo de consultas repetidas “no mesmo dia” (cache).
- **Tecnologias esperadas**: .NET 8, banco à escolha, Clean Code, SOLID, MVC, DDD, testes unitários, publicar código no git.

Como este projeto atende:

- **CRUD**: implementado via endpoints em `KrtBank.Api/Controllers/AccountController.cs` (Create, Get por CPF, Update status, Delete).
- **Eventos de integração**: `AccountCreatedEvent` / `AccountUpdatedEvent` / `AccountDeletedEvent` publicados após operações de escrita; handlers simulam integração com **Cartões** e **Antifraude**.
- **Cache “mesmo dia”**: consulta por CPF usa Redis com **TTL de 24h** (equivalente a “naquele mesmo dia”); em **Update/Delete** o cache é invalidado para evitar dados desatualizados.
- **MVC**: API exposta via Controllers (ASP.NET Core MVC) + middlewares.
- **Testes unitários**: projeto `KrtBank.Tests` cobre entidades e handlers.

## Tecnologias e ferramentas

- **Runtime**: .NET 8
- **Web**: ASP.NET Core
- **Banco**: SQL Server 2022 (Docker)
- **Cache**: Redis (Docker)
- **CQRS/Mediator**: MediatR + Pipeline de validação (FluentValidation)
- **ORM**: Entity Framework Core (Code-First + Migrations)
- **Auth**: JWT Bearer
- **Logs**: Serilog (Console + arquivo)
- **Docs**: Swagger (OpenAPI)

## Arquitetura

O projeto segue separação por camadas:

- **`KrtBank.Domain`**: entidades, enums, exceptions e interfaces (contratos).
- **`KrtBank.Application`**: Commands/Queries + Handlers (MediatR), validações e eventos de domínio (INotification).
- **`KrtBank.Infrastructure`**: EF Core (DbContext/Migrations), repositórios, serviços (Redis/JWT/Password) e mensageria mock.
- **`KrtBank.Api`**: Controllers, middlewares, configurações (JWT/Swagger/CORS/HealthChecks) e composição via DI.

## Como executar

### Pré-requisitos

- **Docker Desktop** (ou equivalente) para subir SQL Server e Redis
- **.NET 8 SDK** instalado e no PATH (`dotnet`)
- (Opcional) **EF CLI** (`dotnet-ef`) para aplicar migrations

### 1) Subir infraestrutura (SQL Server + Redis)

Na raiz do projeto:

```bash
docker-compose up -d
```

Serviços padrão:

- **SQL Server**: `localhost:1433` (usuário `sa`)
- **Redis**: `localhost:6379`

> Observação: a senha do SQL Server está no `docker-compose.yml` e também em `KrtBank.Api/appsettings.json` (`ConnectionStrings:DefaultConnection`).

### 2) Criar banco e aplicar migrations

O projeto possui migrations em `KrtBank.Infrastructure/Context/Migrations`. Para aplicá-las:

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project KrtBank.Infrastructure --startup-project KrtBank.Api
```

### 3) Rodar a API

```bash
dotnet run --project KrtBank.Api
```

URLs padrão (Development):

- Swagger: `http://localhost:5027/swagger`
- Health check: `http://localhost:5027/health`

## Credenciais padrão (seed)

No startup, o `DbSeeder` cria um usuário admin caso não exista:

- **E-mail**: `admin@admin.com`
- **Senha**: `Krt@2026`

E também popula **8 contas de exemplo** *somente se* a tabela `Accounts` estiver vazia.

## Endpoints

Base URL (default): `http://localhost:5027`

### Auth

#### `POST /api/v1/auth/login`

Request:

```json
{
  "email": "admin@admin.com",
  "password": "Krt@2026"
}
```

Response (200):

```json
{
  "success": true,
  "message": "Login realizado com sucesso.",
  "data": {
    "token": "<jwt>"
  }
}
```

### Account (requer JWT)

Para chamar os endpoints abaixo, envie o header:

```
Authorization: Bearer <jwt>
```

#### `GET /api/v1/account/{cpf}`

Busca conta por CPF (11 dígitos, somente números). Usa cache Redis com TTL padrão de 24h.

Response (200):

```json
{
  "success": true,
  "message": "Sucesso",
  "data": {
    "id": "00000000-0000-0000-0000-000000000000",
    "accountHolderName": "João Silva",
    "cpf": "12345678901",
    "status": "Active"
  }
}
```

#### `POST /api/v1/account`

Cria conta (CPF deve ser único).

Request:

```json
{
  "name": "Fulano de Tal",
  "cpf": "11122233344"
}
```

Response (201):

```json
{
  "success": true,
  "message": "Conta aberta com sucesso!",
  "data": {
    "id": "00000000-0000-0000-0000-000000000000",
    "accountHolderName": "Fulano de Tal",
    "cpf": "11122233344",
    "status": "Active"
  }
}
```

#### `PATCH /api/v1/account/status`

Atualiza status da conta. Valores do enum:

- `0` = `Inactive`
- `1` = `Active`

Request:

```json
{
  "cpf": "11122233344",
  "status": 0
}
```

Response (200):

```json
{
  "success": true,
  "message": "Status da conta atualizado com sucesso.",
  "data": true
}
```

#### `DELETE /api/v1/account/{cpf}`

Remove conta por CPF.

Response (200):

```json
{
  "success": true,
  "message": "Conta deletada com sucesso.",
  "data": true
}
```

## Respostas e erros

### Resposta padrão (sucesso)

A API retorna `ApiResponse<T>`:

- `success`: `true`
- `message`: string
- `data`: payload

### Resposta padrão (erro)

Erros são tratados pelo middleware global e retornam:

- `success`: `false`
- `statusCode`: HTTP status
- `message`: mensagem consolidada (inclui validações do FluentValidation)
- `detailed`: stack trace apenas em `Development`

## Cache (Redis)

A consulta `GET /api/v1/account/{cpf}` usa cache distribuído:

- **Chave**: `account:{cpf}`
- **TTL padrão**: 24h
- **Escritas**:
  - Create: grava cache da conta criada
  - Update/Delete: remove cache (`RemoveAsync`) para evitar dado stale

## Eventos e mensageria (mock)

Após Create/Update/Delete, a aplicação publica eventos via MediatR:

- `AccountCreatedEvent`
- `AccountUpdatedEvent`
- `AccountDeletedEvent`

Handlers de integração simulada:

- **`CardServiceHandler`** (fila `cards-issuing-service`)
  - `ISSUE_NEW_CARD`, `SYNC_LIMITS`, `BLOCK_ALL_CARDS`
- **`FraudPreventionHandler`** (fila `fraud-prevention-service`)
  - `CREATE`, `UPDATE`, `DELETE`

O broker é um **mock** (`MockMessageBus`) que apenas loga a “publicação” no console/Serilog.

## Health check e logs

- **Health**: `GET /health` (checa SQL Server e Redis)
- **Logs**:
  - Console (structured logging)
  - Arquivo: `KrtBank.Api/Logs/krtbank-log-YYYYMMDD.txt` (JSON, retenção 7 dias)
- **Performance**: middleware mede latência por request e registra `HTTP {Method} {Path} respondeu {StatusCode} em {ElapsedMs}ms`.

## Testes

Na raiz da solução:

```bash
dotnet test
```
