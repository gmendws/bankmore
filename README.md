# BankMore - Sistema Bancário com Microsserviços

Sistema de gerenciamento bancário desenvolvido em .NET 8 com arquitetura de microsserviços, CQRS, DDD e comunicação assíncrona via Kafka.

## Arquitetura

### Tecnologias
- **.NET 8** - Framework principal
- **Clean Architecture** - Separação de responsabilidades
- **DDD** (Domain-Driven Design) - Modelagem de domínio
- **CQRS** (MediatR) - Separação de Commands e Queries
- **Dapper** - Micro-ORM para acesso a dados
- **SQLite** - Banco de dados
- **Kafka** - Message broker para comunicação assíncrona
- **JWT** - Autenticação e autorização
- **Docker** - Containerização
- **Swagger** - Documentação de APIs
- **xUnit + Moq + FluentAssertions** - Testes unitários

### Microsserviços
```
┌─────────────────────────────────────────────────────────────┐
│                     API CONTA CORRENTE                       │
│  - Cadastro de contas                                        │
│  - Autenticação (Login/JWT)                                  │
│  - Movimentações (Crédito/Débito)                           │
│  - Consulta de saldo                                         │
│  - Inativação de contas                                      │
│  - Consumer Kafka: Tarifações realizadas                    │
└─────────────────────────────────────────────────────────────┘
                              ▲
                              │ HTTP + JWT
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    API TRANSFERÊNCIA                          │
│  - Transferências entre contas                              │
│  - Validação de saldo                                        │
│  - Estorno automático em caso de falha                       │
│  - Producer Kafka: Transferências realizadas                │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ Kafka
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      WORKER TARIFAS                          │
│  - Consumer: Transferências realizadas                      │
│  - Processamento de tarifas (R$ 2,00)                       │
│  - Persistência no banco                                     │
│  - Producer: Tarifações realizadas                          │
└─────────────────────────────────────────────────────────────┘
```

---

## Como Executar

### Pré-requisitos
- Docker Desktop
- Docker Compose

### Subir todos os serviços
```bash
# Na raiz do projeto
docker-compose up --build
```

Aguarde todos os containers iniciarem (health checks):
- ✅ Zookeeper
- ✅ Kafka
- ✅ API Conta Corrente
- ✅ API Transferência
- ✅ Worker Tarifas

### Acessar as APIs

- **API Conta Corrente**: http://localhost:5001
- **API Transferência**: http://localhost:5002

Ambas abrem automaticamente no Swagger.

---

## 📖 Guia de Uso

### 1️⃣ Cadastrar Conta

**POST** `/api/conta-corrente/cadastrar`
```json
{
  "cpf": "12345678909",
  "nome": "João Silva",
  "senha": "senha123"
}
```

**Resposta:**
```json
{
  "numeroConta": 1
}
```

---

### 2️⃣ Fazer Login

**POST** `/api/conta-corrente/login`
```json
{
  "cpfOuNumeroConta": "12345678909",
  "senha": "senha123"
}
```

**Resposta:**
```json
{
  "token": "eyJhbGc...",
  "expiracao": "2026-01-30T10:00:00Z"
}
```

**⚠️ Copie o token!** Será necessário para as próximas requisições.

---

### 3️⃣ Autorizar no Swagger

1. Clique no botão **"Authorize"** 🔒
2. Cole o **token** (sem "Bearer")
3. Clique **"Authorize"**

---

### 4️⃣ Adicionar Saldo

**POST** `/api/conta-corrente/movimentacao`
```json
{
  "idempotenciaKey": "credito-inicial-001",
  "idContaCorrente": null,
  "numeroConta": null,
  "tipoMovimento": 0,
  "valor": 1000.00
}
```

`tipoMovimento`: `0` = Crédito, `1` = Débito

---

### 5️⃣ Consultar Saldo

**GET** `/api/conta-corrente/saldo`

**Resposta:**
```json
{
  "numeroConta": 1,
  "nomeTitular": "João Silva",
  "dataHoraConsulta": "2026-01-29T15:30:00",
  "saldo": 1000.00
}
```

---

### 6️⃣ Fazer Transferência

Primeiro, crie uma **segunda conta** (repita passos 1-4 com CPF diferente).

**POST** `/api/transferencia` (API Transferência)
```json
{
  "idempotenciaKey": "transferencia-001",
  "numeroContaDestino": 2,
  "valor": 100.00
}
```

**O que acontece:**
1. ✅ Débito de R$ 100 na conta 1
2. ✅ Crédito de R$ 100 na conta 2
3. ✅ Kafka: Mensagem "transferencia-realizada"
4. ✅ Worker Tarifas processa
5. ✅ Kafka: Mensagem "tarifacao-realizada"
6. ✅ Débito de R$ 2 na conta 1 (tarifa)

**Saldo final:**
- Conta 1: R$ 898,00 (1000 - 100 - 2)
- Conta 2: R$ 100,00

---

## 🧪 Testando Idempotência

Repita a **mesma transferência** com a **mesma chave**:
```json
{
  "idempotenciaKey": "transferencia-001",
  "numeroContaDestino": 2,
  "valor": 100.00
}
```

**Resultado:** HTTP 204, mas **não executa novamente**!

O saldo permanece o mesmo. ✅

---

## 🎯 Padrões e Conceitos Implementados

### Clean Architecture
```
API → Infrastructure → Application → Domain
         ↓              ↓              ↓
    Dapper, Kafka   Handlers      Entidades
```

### CQRS
- **Commands**: Alteram estado (Create, Update, Delete)
- **Queries**: Apenas leitura (Read)
- **MediatR**: Desacopla Controllers de Handlers

### DDD
- **Entidades**: ContaCorrente, Transferencia
- **Value Objects**: Cpf, Senha
- **Repositories**: Abstração de persistência
- **Services**: Lógica que não cabe em entidade (SaldoService)

### Comunicação Assíncrona
- **Kafka** para desacoplar serviços
- **Producer/Consumer** para tarifas

### Segurança
- **JWT** para autenticação
- **BCrypt** para hash de senhas
- **Validação de CPF** com algoritmo oficial

### Resiliência
- **Idempotência** em todas operações críticas
- **Estorno automático** em transferências
- **Health Checks** para orquestração

---

## 📂 Estrutura do Projeto
```
BankMore/
├── src/
│   ├── Shared/                          # Kernel compartilhado
│   │   └── BankMore.Shared/
│   ├── ContaCorrente/                   # API Conta Corrente
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── API/
│   ├── Transferencia/                   # API Transferência
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── API/
│   └── Tarifas/                         # Worker Tarifas
│       └── Worker/
├── tests/                               # Testes
├── docker-compose.yml                   # Orquestração
└── README.md
```

---

## 🧪 Executar Testes
```bash
# Executar todos os testes
dotnet test

# Executar com detalhes
dotnet test --verbosity normal
```

**Esperado:**
```
Total tests: 16
     Passed: 16
```

---

## 🛑 Parar os Serviços
```bash
docker-compose down
```

**Para limpar volumes:**
```bash
docker-compose down -v
```

---

## 🚢 Preparado para Kubernetes

Embora a entrega seja via Docker Compose, a aplicação está preparada para Kubernetes:

- ✅ Health Checks configurados (`/health`)
- ✅ Variáveis de ambiente externalizadas
- ✅ Stateless (JWT)
- ✅ Logs estruturados (stdout)
- ✅ Graceful shutdown

### Migrando para Kubernetes:

Os recursos do Docker Compose mapeiam diretamente para K8s:

| Docker Compose | Kubernetes |
|----------------|------------|
| `healthcheck` | `livenessProbe` / `readinessProbe` |
| `environment` | `ConfigMap` / `Secret` |
| `networks` | `Service` |
| `volumes` | `PersistentVolumeClaim` |
| `depends_on` | `initContainers` |

Para escalar horizontalmente:
```yaml
replicas: 2
```

---

## 👨‍💻 Detalhes Técnicos

### Requisitos Implementados
- ✅ APIs RESTful com .NET 8
- ✅ Clean Architecture + DDD + CQRS
- ✅ Princípios SOLID
- ✅ Dapper para acesso a dados
- ✅ Banco de dados SQLite
- ✅ Autenticação JWT
- ✅ Documentação Swagger
- ✅ Kafka para comunicação assíncrona
- ✅ Docker Compose
- ✅ Testes unitários (16 testes)
- ✅ Idempotência
- ✅ Validação de CPF (algoritmo oficial)
- ✅ Criptografia de senhas (BCrypt + Salt)
- ✅ Health Checks

### Destaques da Arquitetura
- **Dependency Inversion**: Domain define interfaces, Infrastructure implementa
- **Single Responsibility**: Cada classe tem uma responsabilidade
- **Open/Closed**: Fácil adicionar novos handlers sem modificar código existente
- **Value Objects**: Cpf e Senha encapsulam validação
- **Factory Methods**: Create() para novas instâncias, Reconstruct() para persistência
- **Repository Pattern**: Abstração de persistência
- **Result Pattern**: Evita exceções para fluxos esperados

---

## 📊 Cobertura de Testes

- ✅ Value Objects (Cpf, Senha)
- ✅ Entidades (ContaCorrente, Transferencia)
- ✅ Serviços de Domínio (SaldoService)
- ✅ Handlers (CreateAccount, CreateTransfer)
- ✅ Validação de regras de negócio
- ✅ Idempotência
- ✅ Estorno automático

---

## 🔒 Segurança

- Senhas armazenadas com BCrypt (workfactor 12) + Salt
- JWT com assinatura HS256
- Validação de token com tolerância zero (ClockSkew = 0)
- Validação de CPF com dígitos verificadores
- Autorização em todos os endpoints sensíveis

---

## 📞 Contato

Desenvolvido como projeto de teste técnico para vaga de Desenvolvedor .NET C#.

**Tecnologias:**
- .NET 8, C#
- Clean Architecture, DDD, CQRS
- Dapper, SQLite
- Kafka (KafkaFlow)
- Docker, Docker Compose
- JWT, BCrypt
- Swagger, Serilog
- xUnit, Moq, FluentAssertions
