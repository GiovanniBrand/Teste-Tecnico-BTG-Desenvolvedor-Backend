## BTG Technical Test — Backend Developer

Account Management API (Onboarding) developed in **.NET 8**, applying **DDD**, **Clean Architecture**, and **CQRS** (MediatR) principles. Includes **Redis** as distributed cache and an **event-driven** model (publish/subscribe) to simulate integration with external services (e.g., card issuing and fraud prevention).

## Summary

- [Challenge Requirements (from specification)](#challenge-requirements-from-specification)
- [Technologies and Tools](#technologies-and-tools)
- [Architecture](#architecture)
- [How to Run](#how-to-run)
- [Default Credentials (seed)](#default-credentials-seed)
- [Endpoints](#endpoints)
- [Responses and Errors](#responses-and-errors)
- [Cache (Redis)](#cache-redis)
- [Events and Messaging (mock)](#events-and-messaging-mock)
- [Health Check and Logs](#health-check-and-logs)
- [Tests](#tests)
- [Architectural Decision Records](#architectural-decision-records)

## Challenge Requirements (from specification)

Based on the technical test statement:

- **Objective**: Create an API to manage onboarding accounts (CRUD).
- **Account Data**: `Id`, `Account Holder Name`, `CPF`, `Status` (Active/Inactive).
- **Technical Requirements**:
  - Notify other domains when an account is **created/updated/deleted** (e.g., fraud prevention and cards).
  - Reduce the cost of repeated queries "on the same day" (cache).
- **Expected Technologies**: .NET 8, database of choice, Clean Code, SOLID, MVC, DDD, unit tests, code publishing to git.

How this project meets them:

- **CRUD**: Implemented via endpoints in `KrtBank.Api/Controllers/AccountController.cs` (Create, Get by CPF, Update status, Delete).
- **Integration Events**: `AccountCreatedEvent` / `AccountUpdatedEvent` / `AccountDeletedEvent` published after write operations; handlers simulate integration with **Cards** and **Fraud Prevention**.
- **"Same Day" Cache**: CPF query uses Redis with **24h TTL** (equivalent to "on that same day"); on **Update/Delete**, the cache is invalidated to avoid stale data.
- **MVC**: API exposed via Controllers (ASP.NET Core MVC) + middlewares.
- **Unit Tests**: `KrtBank.Tests` project covers entities and handlers.

## Technologies and Tools

- **Runtime**: .NET 8
- **Web**: ASP.NET Core
- **Database**: SQL Server 2022 (Docker)
- **Cache**: Redis (Docker)
- **CQRS/Mediator**: MediatR + Validation Pipeline (FluentValidation)
- **ORM**: Entity Framework Core (Code-First + Migrations)
- **Auth**: JWT Bearer
- **Logs**: Serilog (Console + file)
- **Docs**: Swagger (OpenAPI)

## Architecture

The project follows layer separation:

- **`KrtBank.Domain`**: Entities, enums, exceptions, and interfaces (contracts).
- **`KrtBank.Application`**: Commands/Queries + Handlers (MediatR), validations, and domain events (INotification).
- **`KrtBank.Infrastructure`**: EF Core (DbContext/Migrations), repositories, services (Redis/JWT/Password), and mock messaging.
- **`KrtBank.Api`**: Controllers, middlewares, configurations (JWT/Swagger/CORS/HealthChecks), and composition via DI.

## How to Run

### Prerequisites

- **Docker Desktop** (or equivalent) to run SQL Server and Redis
- **.NET 8 SDK** installed and in PATH (`dotnet`)
- (Optional) **EF CLI** (`dotnet-ef`) to apply migrations

### 1) Run the complete application (API + Database + Redis)

1. Open the terminal.
2. Navigate to the project root folder (where `docker-compose.yml` is located):
   ```bash
   cd c:\Projetos\Teste_BTG
   ```
   *(Adjust the path according to where you cloned the project)*

3. Run the command to build and start the containers:
   ```bash
   docker-compose up -d --build
   ```

Default services:

- **API running on docker**: `localhost:8080` / `localhost:8081`
- **SQL Server**: `localhost:1433` (user `sa`)
- **Redis**: `localhost:6379`

> Note: SQL Server password is in `docker-compose.yml` and also in `KrtBank.Api/appsettings.json` (`ConnectionStrings:DefaultConnection`).

### 2) Create database and apply migrations

The project has migrations in `KrtBank.Infrastructure/Context/Migrations`. To apply them:

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project KrtBank.Infrastructure --startup-project KrtBank.Api
```

### 3) Run the API

```bash
dotnet run --project KrtBank.Api
```

Default URLs (Development):

- Swagger: `http://localhost:5027/swagger`
- Health check: `http://localhost:5027/health`

## Default Credentials (seed)

At startup, `DbSeeder` creates an admin user if one does not exist:

- **Email**: `admin@admin.com`
- **Password**: `Krt@2026`

It also populates **8 sample accounts** *only if* the `Accounts` table is empty.

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
  "message": "Login successful.",
  "data": {
    "token": "<jwt>"
  }
}
```

### Account (requires JWT)

To call the endpoints below, send the header:

```
Authorization: Bearer <jwt>
```

#### `GET /api/v1/account/{cpf}`

Search account by CPF (11 digits, numbers only). Uses Redis cache with default 24h TTL.

Response (200):

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "id": "00000000-0000-0000-0000-000000000000",
    "accountHolderName": "John Doe",
    "cpf": "12345678901",
    "status": "Active"
  }
}
```

#### `POST /api/v1/account`

Create account (CPF must be unique).

Request:

```json
{
  "name": "Jane Doe",
  "cpf": "11122233344"
}
```

Response (201):

```json
{
  "success": true,
  "message": "Account opened successfully!",
  "data": {
    "id": "00000000-0000-0000-0000-000000000000",
    "accountHolderName": "Jane Doe",
    "cpf": "11122233344",
    "status": "Active"
  }
}
```

#### `PATCH /api/v1/account/status`

Update account status. Enum values:

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
  "message": "Account status updated successfully.",
  "data": true
}
```

#### `DELETE /api/v1/account/{cpf}`

Remove account by CPF.

Response (200):

```json
{
  "success": true,
  "message": "Account deleted successfully.",
  "data": true
}
```

## Responses and Errors

### Default Response (Success)

The API returns `ApiResponse<T>`:

- `success`: `true`
- `message`: string
- `data`: payload

### Default Response (Error)

Errors are handled by the global middleware and return:

- `success`: `false`
- `statusCode`: HTTP status
- `message`: consolidated message (includes FluentValidation validations)
- `detailed`: stack trace only in `Development`

## Cache (Redis)

The query `GET /api/v1/account/{cpf}` uses distributed cache:

- **Key**: `account:{cpf}`
- **Default TTL**: 24h
- **Writes**:
  - Create: caches the created account
  - Update/Delete: removes cache (`RemoveAsync`) to avoid stale data

## Events and Messaging (mock)

After Create/Update/Delete, the application publishes events via MediatR:

- `AccountCreatedEvent`
- `AccountUpdatedEvent`
- `AccountDeletedEvent`

Simulated integration handlers:

- **`CardServiceHandler`** (`cards-issuing-service` queue)
  - `ISSUE_NEW_CARD`, `SYNC_LIMITS`, `BLOCK_ALL_CARDS`
- **`FraudPreventionHandler`** (`fraud-prevention-service` queue)
  - `CREATE`, `UPDATE`, `DELETE`

The broker is a **mock** (`MockMessageBus`) that only logs the "publication" to Console/Serilog.

## Health Check and Logs

- **Health**: `GET /health` (checks SQL Server and Redis)
- **Logs**:
  - Console (structured logging)
  - File: `KrtBank.Api/Logs/krtbank-log-YYYYMMDD.txt` (JSON, 7-day retention)
- **Performance**: middleware measures latency per request and logs `HTTP {Method} {Path} answered {StatusCode} in {ElapsedMs}ms`.

## Tests

In the solution root:

```bash
dotnet test
```

## Architectural Decision Records

This section describes the fundamental technical choices of this project, justifying the "why" of each approach to ensure system maintainability and scalability.

- **CQRS with MediatR**:
  - **Decision**: Physical and logical separation of read (Queries) and write (Commands) operations.
  - **Why**: Avoids disordered growth of service classes. With MediatR, each use case has its own Handler, adhering to the Single Responsibility Principle and facilitating the implementation of logs and validations via IPipelineBehavior.

- **Result Pattern vs. Exceptions**:
  - **Decision**: Use of a `Result<T>` return object instead of throwing exceptions for business flow.
  - **Why**: Exceptions should be reserved for exceptional situations (e.g., connection failure). Using exceptions for flow control (e.g., "User not found") hurts performance and makes code less predictable. The Result Pattern makes error states explicit in the method signature.

- **Security: JWT & OAuth2**:
  - **Decision**: Stateless authentication with JWT tokens and authorization based on Roles.
  - **Why**: Allows horizontal scalability of the API as it does not depend on in-memory state and follows modern market identity standards, facilitating integration with external providers. Being a database connection, it is essential to have guaranteed authentication for API access.

- **Observability**:
  - **Decision**: Implementation of Health Checks and Serilog.
  - **Why**: In cloud environments, structured logs are needed to perform complex searches and telemetry to identify performance bottlenecks in real-time.

- **Sensitive Data in `appsettings.json` (Didactic Exception)**:
  - **Decision**: Leaving JWT key and ConnectionString open in `appsettings.json`.
  - **Why**: In production environments, these data are configured in this same location. In development, it is common for ConnectionString and JWT key to be in User Secrets to avoid leaking this data. However, as this is a test application and data was generated, the option to leave it in `appsettings.json` is just for visualizing the functionality and configuration of the application.

- **Distributed Cache with Redis**:
  - **Decision**: Implementation of `IDistributedCache` using Redis for high-frequency data storage (e.g., catalogs, sessions, heavy query results).
  - **Why**:
    - **Performance**: Reduces application latency by avoiding unnecessary database trips.
    - **Scalability**: In an environment with multiple API instances, local memory cache fails as instances do not share data. Redis centralizes this state.
    - **Note**: The cache-aside pattern was used where the application attempts to read from the cache; if not found, it fetches from the database and populates the cache for the next request.

- **Migrations and Seed Data on Startup (Didactic Exception)**:
  - **Decision**: Automatic execution of `context.Database.Migrate()` and initial data insertion directly in `Program.cs` when starting the application.
  - **Why**:
    - **Practicality**: Just like data in `appsettings.json`, this choice aims for practicality in testing where the developer does not need to manually insert test data as they are inserted automatically.
    - **Production Warning**: In production and staging environments, migrations should not be run on startup; this requires the application user to have Owner permissions on the database, which violates security principles. The correct way is to run migrations via scripts in the CD (Continuous Deployment) Pipeline.

- **Communication Services Mock (Didactic Exception)**:
  - **Decision**: Use of console logs or local files to simulate sending messaging to other services.
  - **Why**: This allows testing the complete business flow just by observing the terminal output, keeping the focus on code logic and not integration with other services, in the example integration with credit card or fraud protection service.

- **Resilience and Transient Failures**:
  - **Decision**: Enabling `EnableRetryOnFailure` in Entity Framework Core and retry configuration in the Redis driver.
  - **Why**: In distributed and containerized environments, momentary network failures are common. The retry mechanism prevents the application from failing immediately due to transient errors, increasing system availability and robustness, crucial for a financial environment.

- **Deploy with Multi-stage Docker**:
  - **Decision**: Implementation of a `Dockerfile` in multiple stages (Build and Runtime) and orchestration via Docker Compose.
  - **Why**:
    - **Security and Size**: The final production image contains only the .NET runtime, without SDKs or source code, reducing the attack surface and container size.
    - **Ease of Deploy**: `docker-compose` now brings up the complete application (API + Database + Cache) with a single command, ensuring an execution environment identical to production/staging.

---

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
- [Pontos de Decisão Arquitetural](#pontos-de-decisão-arquitetural)

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

### 1) Subir a aplicação completa (API + Banco + Redis)

1. Abra o terminal.
2. Navegue até a pasta raiz do projeto (onde está o arquivo `docker-compose.yml`):
   ```bash
   cd c:\Projetos\Teste_BTG
   ```
   *(Ajuste o caminho conforme onde você clonou o projeto)*

3. Execute o comando para compilar e subir os containers:
   ```bash
   docker-compose up -d --build
   ```

Serviços padrão:

- **API rodando no docker**: `localhost:8080` / `localhost:8081`
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

## Pontos de Decisão Arquitetural

Esta seção descreve as escolhas técnicas fundamentais deste projeto, justificando o "porquê" de cada abordagem para garantir a manutenibilidade e escalabilidade do sistema.

- **CQRS com MediatR**:
  - **Decisão**: Separação física e lógica de operações de leitura (Queries) e escrita (Commands).
  - **Por que:** Evita o crescimento desordenado de classes de serviço. Com MediatR, cada caso de uso tem seu próprio Handler, respeitando o Princípio de Responsabilidade Única e facilitando a implementação de logs e validações via IPipelineBehavior.

- **Result Pattern vs. Exceptions**:
  - **Decisão**: Utilização de um objeto de retorno Result<T> em vez de lançar exceções para fluxo de negócio.
  - **Por que** Exceções devem ser reservadas para situações excepcionais (ex: falha de conexão). O uso de exceções para controle de fluxo (ex: "Usuário não encontrado") prejudica a performance e torna o código menos previsível. O Result Pattern torna os estados de erro explícitos na assinatura do método.

- **Segurança: JWT & OAuth2**:
  - **Decisão**: Autenticação stateless com tokens JWT e autorização baseada em Roles.
  - **Por que**: Permite a escalabilidade horizontal da API onde não depende de estado em memória e segue os padrões modernos de identidade de mercado, facilitando a integração com provedores externos. E por se tratar de conexão ao banco, é providencial que tenha uma autenticação garantida para a liberação das API.
  
- **Observabilidade**:
  - **Decisão**: Implementação de Health Checks e Serilog.
  - **Por que** Em ambientes de cloud, é preciso logs estruturados para realizar buscas complexas e telemetria para identificar gargalos de performance em tempo real.

- **Dados Sensiveis no `appsettings.json` (Exceção Didática)**:
  - **Decisão**: Deixar a chave do JWT e ConnectionString abertas no appsettings.json.
  - **Por que**: Em ambientes de produção, estes dados ficam configurados neste mesmo local. Em desenvolvimento é comum ConnectionString e chave do JWT ficarem no User Secrets para não vazar esses dados. Porém como se trata de uma aplicação para teste e os dados foram gerados, a opção por deixar no `appsettings.json` é apenas para visualização da montagem e configuração do aplicativo.

- **Cache Distribuído com Redis**:
  - **Decisão**: Implementação de IDistributedCache utilizando Redis para armazenamento de dados de alta frequência (ex: catálogos, sessões, resultados de queries pesadas).
  - **Por que**:
    - **Performance**: Reduz a latência da aplicação ao evitar idas desnecessárias ao banco de dados.
    - **Escalabilidade**: Em um ambiente com múltiplas instâncias da API, o cache em memória local falha pois as instâncias não compartilham dados. O Redis centraliza esse estado.
    - **Ponto de Atenção**: Foi utilizado o padrão de cache onde a aplicação tenta ler do cache; se não encontrar, busca no banco e popula o cache para a próxima requisição.

- **Migrations e Seed Data no Startup (Exceção Didática)**:
  - **Decisão**: Execução automática de context.Database.Migrate() e inserção de dados iniciais diretamente no Program.cs ao iniciar a aplicação.
  - **Por que**:
    - **Praticidade**: Assim como os dados no `appsettings.json`, esta escolha visa praticidade no teste onde o desenvolvedor não precisa inserir dados teste manualmente pois os mesmos são inseridos automaticamente.
    - **Ressalva de Produção**: Em ambientes de produção e homologação, não se deve executar migrações no startup, isso exige que o usuário da aplicação tenha permissões de Owner no banco, o que viola princípios de segurança. O correto é rodar as migrations via scripts no Pipeline de CD (Continuous Deployment).

- **Mock de Serviços de Comunicação (Exceção Didática)**:
  - **Decisão**: Uso de logs no console ou arquivos locais para simular o envio mensageria para outros serviços.
  - **Por que**: Isso permite testar o fluxo de negócio completo apenas observando a saída do terminal, mantendo o foco na lógica do código e não na integração com outros serviços, no exemplo integração com cartão de credito ou serviço de proteção de fraude.

- **Resiliência e Falhas Transitórias**:
  - **Decisão**: Habilitação do `EnableRetryOnFailure` no Entity Framework Core e configuração de retry no driver do Redis.
  - **Por que**: Em ambientes distribuídos e containerizados, falhas de rede momentâneas são comuns. O mecanismo de retry evita que a aplicação falhe imediatamente por erros passageiros, aumentando a disponibilidade e robustez do sistema, crucial para um ambiente financeiro.

- **Deploy com Docker Multi-stage**:
  - **Decisão**: Implementação de um `Dockerfile` em múltiplos estágios (Build e Runtime) e orquestração via Docker Compose.
  - **Por que**: 
    - **Segurança e Tamanho**: A imagem final de produção contém apenas o runtime do .NET, sem SDKs ou código fonte, reduzindo a superfície de ataque e o tamanho do container.
    - **Facilidade de Deploy**: O `docker-compose` agora sobe a aplicação completa (API + Banco + Cache) com um único comando, garantindo um ambiente de execução idêntico ao de produção/homologação.