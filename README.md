# ⚡ Backend Fintech High-Performance com Resiliência a Falhas de Rede

[![.NET 8 CI Pipeline](https://github.com/GeovaneParedes/Backend-Fintech-High-Performance/actions/workflows/ci.yml/badge.svg)](https://github.com/GeovaneParedes/Backend-Fintech-High-Performance/actions)
![NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20%2F%20High--Performance-emerald)
![Polly Resilience](https://img.shields.io/badge/Resilience-Polly%20%2B%20Circuit%20Breaker-orange)
![Database](https://img.shields.io/badge/Database-Dapper%20%2B%20PostgreSQL-336791?logo=postgresql)

API Fintech Core resiliente a falhas de comunicação de rede, desenvolvida em C# com **.NET 8 Minimal APIs**, **Dapper**, **System.Text.Json Source Generators** e arquitetura **GC-Friendly** integrada a um simulador TEF/POS resiliente via **Polly v8**.

---

## 🎯 Requisitos de Arquitetura & Performance

- **Low-Allocation & GC-Friendly:** DTOs estruturados em `readonly record struct`, retornos assíncronos de alta frequência com `ValueTask<T>`.
- **Serialização Estática:** `System.Text.Json Source Generators` habilitados para eliminar reflexão em runtime (`TypeInfoResolverChain`).
- **Resiliência Transacional (Polly v8):**
  - **Retry Pattern** com *Exponential Backoff* e *Jitter* para evitar o efeito *Thundering Herd*.
  - **Circuit Breaker** automatizado para desarmar requisições em quedas contínuas de canal TEF.
  - **Chave de Idempotência (`Idempotency-Key`):** Garantia de cobrança única em reconexões.
  - **Fallback Async (Outbox / Background Worker):** Transações não finalizadas por perda total de conectividade recebem status `PENDING_RETRY` para reprocessamento em segundo plano via `BackgroundService`.
- **Acesso a Dados Ultra-Rápido:** SQL puro gerenciado via **Dapper** assíncrono.
- **Segurança:** Autenticação JWT resiliente com Refresh Tokens e Hashing seguro via `BCrypt`.

---

## 🏗️ Estrutura da Solução

```text
Backend-Fintech-High-Performance/
├── src/
│   ├── FintechCore.Api/            # Servidor B: Minimal API principal (.NET 8)
│   └── FakeTef.Api/                # Servidor A: Simulador TEF / POS (Latência & Chaos)
├── tests/
│   └── FintechCore.Tests/          # Testes Unitários e de Integração com xUnit & FluentAssertions
├── .github/
│   └── workflows/
│       └── ci.yml                  # GitHub Actions CI/CD Pipeline (Lint, Build & Testes)
├── docker-compose.yml              # Orquestração do Backend, Fake TEF e PostgreSQL
└── README.md
```

---

## 🚀 Como Executar Localmente

### Pré-requisitos
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker & Docker Compose](https://www.docker.com/)

### Passo a Passo
```bash
# 1. Clonar o repositório
git clone https://github.com/GeovaneParedes/Backend-Fintech-High-Performance.git
cd Backend-Fintech-High-Performance

# 2. Subir os serviços via Docker Compose
docker compose up -d --build

# 3. Testar os endpoints
# Fintech Core API: http://localhost:5000/swagger
# Fake TEF API: http://localhost:5001/swagger
```

---

## 🧪 Execução de Testes Automatizados & Linter
```bash
dotnet test --configuration Release
```

---

## 📜 Licença
Este projeto está sob a licença [MIT](LICENSE).
