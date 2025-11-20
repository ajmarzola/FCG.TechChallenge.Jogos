# FCG.TechChallenge.Jogos

> Microsserviço de **Jogos** da plataforma **FIAP Cloud Games (FCG)** — evolução do MVP do repositório **Grupo49-TechChallenge**, agora separado em **microsserviços** e com **busca avançada via Elasticsearch**, **processos assíncronos** e **observabilidade**. Este serviço cuida do **catálogo**, **busca**, **biblioteca do usuário** e **compra** de jogos, integrando-se a **Usuários** (autenticação) e **Pagamentos** (intents/status).

- **Usuarios** (auth/identidade): https://github.com/ajmarzola/FCG.TechChallenge.Usuarios  
- **Pagamentos** (intents/status): https://github.com/ajmarzola/FCG.TechChallenge.Pagamentos  
- **Jogos** (este repositório): https://github.com/ajmarzola/FCG.TechChallenge.Jogos

🔎 **Projeto anterior (base conceitual):**  
https://github.com/ajmarzola/Grupo49-TechChallenge

🧭 **Miro – Visão de Arquitetura:**  
<https://miro.com/welcomeonboard/VXBnOHN6d0hWOWFHZmxhbzlMenp2cEV3N0FPQm9lUEZwUFVnWC9qWnUxc2ZGVW9FZnZ4SjNHRW5YYVBRTUJEWkFaTjZPNmZMcXFyWUNONEg3eVl4dEdOZWozd0J3RzZld08xM3E1cGl2dTR6QUlJSUVFSkpQcFVSRko1Z0hFSXphWWluRVAxeXRuUUgwWDl3Mk1qRGVRPT0hdjE=?share_link_id=964446466388>

---

## Sumário

- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Tecnologias](#tecnologias)
- [Como Rodar (Rápido)](#como-rodar-rápido)
- [Configuração por Ambiente](#configuração-por-ambiente)
- [Executando com .NET CLI](#executando-com-net-cli)
- [Executando com Docker](#executando-com-docker)
- [Elasticsearch: Índice e Ping](#elasticsearch-índice-e-ping)
- [Fluxo de Teste End-to-End](#fluxo-de-teste-end-to-end)
- [Coleções/API Docs](#coleçõesapi-docs)
- [Estrutura do Repositório](#estrutura-do-repositório)
- [CI/CD](#cicd)
- [Roadmap](#roadmap)
- [Licença](#licença)

---

## Visão Geral

O **FCG.TechChallenge.Jogos** provê APIs REST para **CRUD de jogos**, **busca** (com **Elasticsearch**), **compra** e **consulta de biblioteca**. Ele publica e consome **eventos** para manter o índice de busca atualizado e coordenar a jornada de compra com o serviço de **Pagamentos** por meio de **filas/tópicos**.

Os requisitos da fase incluem: separar em três microsserviços (**Usuários, Jogos, Pagamentos**), indexar dados no **Elasticsearch** com consultas/agragações avançadas, usar **funcões serverless** para tarefas assíncronas e melhorar **observabilidade** (logs/traces).

---

## Arquitetura

- **API Jogos** (ASP.NET Core) — catálogo, compra, biblioteca.
- **Read Model + Índice** — **Elasticsearch** para busca rápida; indexer assíncrono atualiza o índice a partir de eventos.
- **Write Model** — banco relacional (PostgreSQL/SQL Server) para persistência transacional.
- **Mensageria** — barramento/filas para propagar eventos e processar compra/pagamentos de forma **assíncrona**; DLQ para falhas.
- **Serverless** — **Azure Functions** para indexação e orquestrações (ex.: atualização do índice, handlers de eventos).

> O **API Gateway** (com **JWT**) orquestra o tráfego e a autenticação, roteando o front-end para as APIs de Usuários, Jogos e Pagamentos.

---

## Tecnologias

- **.NET 8** (API e processos)
- **EF Core** (PostgreSQL/SQL Server)
- **Elasticsearch** (busca/agragações)
- **Azure Service Bus** (eventos/filas/tópicos)
- **Azure Functions** (indexação/consumidores assíncronos)
- **Docker** (containers para dev e CI)

---

## Monitoramento
- Instalação do stack de monitoramento via Helm — ver [values-monitoring.yaml](https://github.com/ajmarzola/Grupo49-TechChallenge/blob/main/infra/monitoring/values-monitoring.yaml)

---

## Como Rodar (Rápido)

Duas opções:

1) **.NET CLI (sem Docker)** – ciclo de dev mais ágil.  
2) **Docker** – isolamento total e paridade com produção.

> Antes de iniciar, configure variáveis e *connection strings* conforme a seção abaixo.

### Pré-requisitos

- .NET SDK 8.x  
- Docker + Docker Compose (para a opção 2)  
- Banco (PostgreSQL **ou** SQL Server) acessível/local  
- **Elasticsearch** acessível (preferencialmente **Elastic Cloud**, ou um nó local para testes)  
- (Opcional) Azure Functions Core Tools (para indexers/handlers locais)

---

## Configuração por Ambiente

Use `appsettings.Development.json` **ou** variáveis de ambiente (recomendado).

| Chave (Environment) | Exemplo / Descrição |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` |
| `ConnectionStrings__Default` | `Host=localhost;Port=5432;Database=fcg_games;Username=dev;Password=dev` |
| `Elastic__CloudId` | `elastic-<nome>:<hash>` (**Elastic Cloud**) |
| `Elastic__ApiKey` | `base64id:base64secret` **ou** `Elastic__User`/`Elastic__Password` |
| `Elastic__Index__Games` | `fcg-games` |
| `ServiceBus__ConnectionString` | `Endpoint=sb://...;SharedAccessKeyName=...;SharedAccessKey=...` |
| `ServiceBus__Topics__Games` | `games-events` |
| `ServiceBus__Subscriptions__Indexer` | `games-indexer` |
| `Jwt__Authority` | URL do emissor (B2C/IdP) |
| `Jwt__Audience` | `fcg-api` |
| `Observability__EnableTracing` | `true` |

> Ajuste os nomes reais conforme o seu `appsettings`. Caso use **Elastic Cloud**, prefira `CloudId` + `ApiKey`. Se usar um **Elasticsearch local**, use `Elastic__Uri` (`http://localhost:9200`) + `Elastic__User/Password`.

---

## Executando com .NET CLI

> Estrutura típica da solução: **Application**, **Domain**, **Infrastructure**, **Presentation**, **Test**.

1. Restaurar & compilar
   ```bash
   dotnet restore
   dotnet build -c Debug
   ```

2. (Opcional) Aplicar **migrations** (Write/Read Model)
   ```bash
   dotnet ef database update \
     -s FCG.TechChallenge.Jogos.Api \
     -p FCG.TechChallenge.Jogos.Infrastructure
   ```

3. Executar a **API**
   ```bash
   dotnet run -c Debug --project src/FCG.TechChallenge.Jogos.Api
   ```
   - Por padrão, `http://localhost:5085` (ajuste conforme `launchSettings.json`).

4. (Opcional) Executar **Azure Functions** (indexers/handlers)
   ```bash
   func start
   ```

---

## Executando com Docker

> Este repo pode conter `docker-compose.yml` para levantar a API, banco e dependências (ajuste conforme necessidade).

1. Buildar imagens
   ```bash
   docker compose build
   ```

2. Subir serviços
   ```bash
   docker compose up -d
   ```

3. Ver logs
   ```bash
   docker compose logs -f jogos-api
   ```

> **Elasticsearch local** (opcional): você pode subir um nó *single* para desenvolvimento e apontar `Elastic__Uri` para `http://localhost:9200`. Para produção, recomenda-se **Elastic Cloud** e `CloudId+ApiKey`.

---

## Elasticsearch: Índice e Ping

### 1) Verificar conectividade (**ping**)
```bash
curl -u "<usuario>:<senha>" https://<seu-endpoint-elastic>/
# ou, em local:
curl http://localhost:9200/
```

### 2) Criar índice básico (dev/local)
```bash
curl -X PUT http://localhost:9200/fcg-games \
  -H "Content-Type: application/json" \
  -d '{
        "settings": { "number_of_shards": 1, "number_of_replicas": 0 },
        "mappings": {
          "properties": {
            "id":        { "type": "keyword" },
            "nome":      { "type": "text" },
            "descricao": { "type": "text" },
            "preco":     { "type": "double" },
            "tags":      { "type": "keyword" }
          }
        }
      }'
```

> Em **produção**, crie o índice via *bootstrap* do próprio serviço ou *pipeline* IaC. As buscas avançadas/agragações são requisitos desta fase.

---

## Fluxo de Teste End-to-End

> **Cenário típico**: Usuário autenticado lista/busca jogos, adiciona ao carrinho e inicia compra; **Jogos** publica intent de pagamento; **Pagamentos** processa e notifica; **Jogos** confirma aquisição e atualiza biblioteca.

1) **Subir microsserviços** (CLI ou Docker):  
   - **Usuarios** (gera **JWT** para chamadas autenticadas)  
   - **Jogos** (este repo)  
   - **Pagamentos** (consumirá intents publicadas por **Jogos**)

2) **Obter token** (Usuarios)  
   - Login e capture o `access_token` (JWT).

3) **CRUD & Busca de Jogos**
```bash
# Criar jogo
curl -X POST http://localhost:5085/api/jogos \
  -H "Authorization: Bearer <JWT>" \
  -H "Content-Type: application/json" \
  -d '{ "nome":"Celeste", "descricao":"Plataforma", "preco":49.90, "tags":["indie","plataforma"] }'

# Buscar (Elasticsearch)
curl "http://localhost:5085/api/jogos/busca?q=plataforma&tags=indie"
```

4) **Compra / Intent de Pagamento**
```bash
curl -X POST http://localhost:5085/api/jogos/compra \
  -H "Authorization: Bearer <JWT>" \
  -H "Content-Type: application/json" \
  -d '{ "jogoId":"<id>", "usuarioId":"<userId>", "metodo":"credit_card" }'
# → publica evento / intent; Pagamentos processa assíncrono
```

5) **Biblioteca do Usuário**
```bash
curl http://localhost:5085/api/jogos/biblioteca \
  -H "Authorization: Bearer <JWT>"
```

---

## Coleções/API Docs

- **Swagger/OpenAPI**: `http://localhost:<porta>/swagger`
- **Postman**: recomenda-se criar uma Collection com as rotas acima.
- **Autorização**: inclua o **JWT** do serviço de **Usuarios** nas requisições.

---

## Estrutura do Repositório

```
FCG.TechChallenge.Jogos/
├─ src/
│  ├─ FCG.TechChallenge.Jogos.Api/
│  ├─ FCG.TechChallenge.Jogos.Application/
│  ├─ FCG.TechChallenge.Jogos.Domain/
│  └─ FCG.TechChallenge.Jogos.Infrastructure/
├─ tests/
├─ docker-compose.yml
└─ FCG.TechChallenge.Jogos.sln
```

> Alguns nomes/pastas podem variar no seu repo — ajuste os comandos conforme a organização atual.

---

## CI/CD

- **GitHub Actions** para *build*, *test*, *container publish* e *deploy* (App Service / Container Apps / Functions).  
- **Environments** (Dev/Homolog/Prod) com **aprovação manual** para Prod.  
- **OIDC + azure/login** (se publicar no Azure).  
- **Secrets** por ambiente (ex.: `ELASTIC__APIKEY`, `CONNECTIONSTRINGS__DEFAULT`, `SERVICEBUS__CONNECTIONSTRING`).  
- *Infra as Code* opcional para Elasticsearch e Service Bus.

> Os entregáveis da fase pedem **README completo**, desenho/fluxo de arquitetura e **pipelines**; o deploy serverless é recomendado.

---

## Roadmap

- [ ] Projeções e *reindex* guiados por eventos (*event sourcing* + indexer)  
- [ ] Recomendações por histórico/agragações (popularidade, tags, preço)  
- [ ] Cache de consulta (elástico + memória)  
- [ ] Tracing distribuído (W3C) e métricas customizadas  
- [ ] **Rate limiting** e *circuit breakers* em integrações

---

## Licença

Projeto acadêmico, parte do **Tech Challenge FIAP**. Verifique os termos aplicáveis a cada repositório.

## 👥 Integrantes do Grupo
• Anderson Marzola — RM360850 — Discord: aj.marzola

• Rafael Nicoletti — RM361308 — Discord: rafaelnicoletti_

• Valber Martins — RM3608959 — Discord: valberdev
