# Teste Técnico BTG Desenvolvedor Backend
API de Gerenciamento de Contas (Onboarding) desenvolvida em .NET 8, aplicando princípios de DDD, Clean Architecture e CQRS. Implementa estratégias de cache distribuído para otimização de custos e arquitetura baseada em eventos para integração entre sistemas.

## Tecnologias e Ferramentas

* **Runtime:** .NET 8 SDK.
* **Banco de Dados:** SQL Server 2022 (via Docker).
* **Cache Distribuído:** Redis para otimização de consultas de alta frequência.
* **Mensageria & CQRS:** MediatR para desacoplamento da lógica de negócio.
* **ORM:** Entity Framework Core com abordagem Code-First.
* **Logs & Telemetria:** Serilog com Structured Logging.
* **Autenticação:** JWT (JSON Web Token) com Claims customizadas.

---

## Arquitetura do Sistema

O projeto está dividido em quatro camadas principais para garantir a separação de responsabilidades (SoC):

1.  **Domain:** Contém as entidades de negócio, enums e interfaces de repositório. As entidades utilizam encapsulamento rigoroso (`private set`) para manter a integridade dos dados.
2.  **Application:** Implementa os Handlers do MediatR, Commands e Queries, orquestrando o fluxo da aplicação.
3.  **Infrastructure:** Implementação de serviços externos, contexto do banco de dados (EF Core), configuração do Redis e serviços de segurança (Tokens/Criptografia).
4.  **WebApi:** Ponto de entrada da aplicação, contendo os Controllers, Middlewares de performance e configurações de DI.



---

##  Diferenciais Técnicos

### Observabilidade e Telemetria (Middleware)
Foi implementado um **Middleware de Performance** que utiliza o `Stopwatch` para capturar o tempo de execução de cada requisição HTTP. Os logs são registrados de forma estruturada no Serilog, permitindo monitorar a latência e o status da API em tempo real.

### Autenticação e Segurança (JWT)
A API utiliza **JWT Bearer Token** para proteger endpoints sensíveis.
* **Login Seguro:** Integração com `PasswordService` para validação de credenciais.
* **Autorização:** Uso do atributo `[Authorize]` em rotas críticas como busca e atualização de contas.

### Resiliência no Startup (Smart Seeding)
O sistema conta com um **DbSeeder** inteligente que popula automaticamente a base de dados com 8 registros de teste (Contas Ativas e Inativas) apenas se a tabela estiver vazia (`AnyAsync` check). Isso garante que o avaliador tenha uma massa de dados pronta para testes imediatos sem redundância.

### Estratégia de Cache
A busca de contas por CPF foi otimizada com **Redis**, reduzindo drasticamente o tempo de resposta e a carga no SQL Server em consultas repetitivas.

---

## ⚙️ Como Executar o Projeto

### 1. Subir Infraestrutura (Docker)
Na raiz do projeto, onde se encontra o arquivo `docker-compose.yml`, execute:
```bash
docker-compose up -d
