# BankMore - Sistema Bancário com Microsserviços

Sistema de gerenciamento bancário desenvolvido em .NET 8, implementando arquitetura de microsserviços com comunicação assíncrona via Kafka. O projeto demonstra a aplicação prática de padrões como CQRS, DDD e Clean Architecture em um contexto bancário simplificado.

## Visão Geral da Arquitetura

O sistema é composto por três serviços independentes que se comunicam de forma assíncrona:

**API Conta Corrente**
- Gerenciamento completo de contas (cadastro, autenticação, movimentações)
- Consulta de saldo e extrato
- Consumidor Kafka: processa tarifações realizadas

**API Transferência**
- Processamento de transferências entre contas
- Validação de saldo e estorno automático em caso de falha
- Produtor Kafka: publica eventos de transferência

**Worker Tarifas**
- Processamento assíncrono de tarifas
- Consome eventos de transferência
- Aplica tarifa de R$ 2,00 por operação
- Publica eventos de tarifação concluída

### Stack Tecnológica

- .NET 8 / C#
- SQLite (banco de dados)
- Dapper (micro-ORM)
- MediatR (implementação CQRS)
- Kafka + KafkaFlow (mensageria)
- JWT (autenticação)
- BCrypt (criptografia de senhas)
- xUnit + Moq + FluentAssertions (testes)
- Docker / Docker Compose

## Executando o Projeto

### Pré-requisitos

- Docker Desktop instalado e em execução

### Iniciar os serviços

```bash
docker-compose up --build
```

O comando irá subir todos os containers necessários: Zookeeper, Kafka e os três microsserviços. Aguarde os health checks confirmarem que todos os serviços estão prontos.

**URLs de acesso:**
- API Conta Corrente: http://localhost:5001/swagger
- API Transferência: http://localhost:5002/swagger

## Fluxo de Uso

### 1. Cadastrar uma conta

```http
POST /api/conta-corrente/cadastrar
Content-Type: application/json

{
  "cpf": "12345678909",
  "nome": "João Silva",
  "senha": "senha123"
}
```

Resposta:
```json
{
  "numeroConta": 1
}
```

### 2. Autenticar

```http
POST /api/conta-corrente/login
Content-Type: application/json

{
  "cpfOuNumeroConta": "12345678909",
  "senha": "senha123"
}
```

Resposta:
```json
{
  "token": "eyJhbGc...",
  "expiracao": "2026-01-30T10:00:00Z"
}
```

**Importante:** Use o token retornado no header `Authorization: Bearer {token}` nas próximas requisições. No Swagger, clique em "Authorize" e cole o token.

### 3. Adicionar saldo

```http
POST /api/conta-corrente/movimentacao
Authorization: Bearer {seu-token}
Content-Type: application/json

{
  "idempotenciaKey": "credito-inicial-001",
  "tipoMovimento": 0,
  "valor": 1000.00
}
```

> `tipoMovimento`: 0 = Crédito | 1 = Débito

### 4. Consultar saldo

```http
GET /api/conta-corrente/saldo
Authorization: Bearer {seu-token}
```

### 5. Realizar transferência

Primeiro crie uma segunda conta (repita os passos 1-3 com CPF diferente). Depois:

```http
POST /api/transferencia
Authorization: Bearer {seu-token}
Content-Type: application/json

{
  "idempotenciaKey": "transferencia-001",
  "numeroContaDestino": 2,
  "valor": 100.00
}
```

**O que acontece nos bastidores:**

1. Débito de R$ 100 na conta origem
2. Crédito de R$ 100 na conta destino
3. Publicação do evento no Kafka
4. Worker processa a tarifa (R$ 2)
5. Débito da tarifa na conta origem

**Resultado:** Conta origem fica com R$ 898 (1000 - 100 - 2)

## Decisões de Arquitetura

### Clean Architecture

O projeto segue a separação em camadas proposta por Robert C. Martin:

```
Domain (núcleo) 
  ↓
Application (casos de uso)
  ↓
Infrastructure (detalhes técnicos)
  ↓
API (entrega)
```

Essa estrutura garante que as regras de negócio sejam independentes de frameworks, UI ou banco de dados.

### CQRS com MediatR

Commands e Queries são separados, cada um com seu handler específico. Isso simplifica o código e facilita a aplicação de responsabilidades diferentes para leitura e escrita.

### Domain-Driven Design

**Entidades:** ContaCorrente e Transferencia encapsulam regras de negócio  
**Value Objects:** Cpf e Senha garantem validação em tempo de criação  
**Domain Services:** SaldoService centraliza lógica que não pertence a uma única entidade  
**Repositories:** Abstraem a persistência, permitindo trocar a implementação sem impactar o domínio

### Comunicação Assíncrona

Kafka foi escolhido para desacoplar os serviços. O worker de tarifas não precisa estar disponível no momento da transferência - ele processa o evento quando possível, tornando o sistema mais resiliente.

### Segurança

- Senhas armazenadas com BCrypt (workfactor 12)
- JWT com assinatura HS256 e validação rigorosa
- Validação de CPF seguindo algoritmo oficial dos dígitos verificadores
- Autorização obrigatória em endpoints de movimentação financeira

### Idempotência

Todas as operações críticas (movimentações e transferências) utilizam uma chave de idempotência. Requisições duplicadas com a mesma chave são ignoradas, evitando débitos/créditos em duplicidade.

## Estrutura do Código

```
BankMore/
├── src/
│   ├── Shared/
│   │   └── BankMore.Shared/              # Contratos e utilitários compartilhados
│   ├── ContaCorrente/
│   │   ├── Domain/                       # Entidades, VOs, interfaces
│   │   ├── Application/                  # Handlers, DTOs, validações
│   │   ├── Infrastructure/               # Repositórios, Kafka, DB
│   │   └── API/                          # Controllers, configuração
│   ├── Transferencia/
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── API/
│   └── Tarifas/
│       └── Worker/                       # Consumer Kafka, processamento
├── tests/
│   ├── BankMore.ContaCorrente.Tests/
│   └── BankMore.Transferencia.Tests/
└── docker-compose.yml
```

## Testes

O projeto possui cobertura de testes unitários para as principais regras de negócio:

```bash
dotnet test
```

**Cobertura atual:**
- Value Objects (validações de CPF e senha)
- Entidades (invariantes de domínio)
- Domain Services (lógica de negócio)
- Application Handlers (casos de uso)
- Validação de idempotência
- Fluxo de estorno

Total: 29 testes passando


## Deployment

O projeto está pronto para containers. Para parar os serviços:

```bash
docker-compose down
```

Para limpar volumes e recomeçar do zero:

```bash
docker-compose down -v
```

A aplicação possui health checks configurados e pode ser migrada para Kubernetes com ajustes mínimos nos manifests.

---

**Desenvolvido como projeto técnico para demonstração de habilidades em arquitetura de microsserviços, padrões de design e boas práticas de desenvolvimento .NET.**
