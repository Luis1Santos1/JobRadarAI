# JobRadar AI

Plataforma inteligente para descoberta responsável, análise e gestão de oportunidades de trabalho para desenvolvedores.

## Objetivo

O JobRadar AI é um projeto pessoal de portfólio criado para organizar recrutadores, posts de vagas, oportunidades e candidaturas, usando IA local, arquitetura moderna e observabilidade.

## Stack

- Backend: .NET 10, ASP.NET Core
- Arquitetura: Clean Architecture
- Frontend: Angular
- Banco de dados: SQL Server
- ORM: EF Core
- Workers: Hangfire
- Mensageria: Kafka
- IA: modelo local
- Auth: JWT/RBAC
- Observabilidade: Serilog, Prometheus e Grafana
- Deploy local: Docker Compose

## Responsible Automation

O JobRadar AI não automatiza ações dentro do LinkedIn, como envio de convite, envio de mensagens, login automatizado, scraping de perfis, scraping de posts ou qualquer simulação de comportamento humano.

A plataforma foca em organização, importação assistida, análise com IA, deduplicação, scoring de oportunidades e recomendações de ações manuais.

## Fase atual

Fase 0 — Fundação.

### Entregas

- Solução .NET
- Clean Architecture
- Angular
- Docker Compose com API, Web e SQL Server
- EF Core
- Migration inicial
- Health checks
- Serilog
- Swagger em ambiente de desenvolvimento

## Como rodar

```bash
docker compose up --build
```

## URLs

- Frontend: http://localhost:4200
- API: http://localhost:8080
- Swagger: http://localhost:8080/swagger
- Health Live: http://localhost:8080/health/live
- Health Ready: http://localhost:8080/health/ready
- System Info: http://localhost:8080/api/v1/system/info

## Estrutura do projeto

```text
src/
  JobRadar.Api
  JobRadar.Application
  JobRadar.Domain
  JobRadar.Infrastructure
  JobRadar.Worker
  JobRadar.Contracts
  JobRadar.SharedKernel

frontend/
  jobradar-web

tests/
docker/
docs/
```

## Critérios de aceite da Fase 0

- [ ] Repositório GitHub criado
- [ ] README inicial criado
- [ ] Solução .NET criada
- [ ] Projetos organizados em Clean Architecture
- [ ] Angular criado em `/frontend/jobradar-web`
- [ ] Docker Compose criado
- [ ] SQL Server sobe no Docker
- [ ] API sobe no Docker
- [ ] Frontend sobe no Docker
- [ ] EF Core configurado
- [ ] Migration `InitialCreate` criada
- [ ] Migration aplicada no SQL Server
- [ ] `/health/live` retorna Healthy
- [ ] `/health/ready` retorna Healthy
- [ ] Swagger abre em ambiente Development
- [ ] Logs estruturados aparecem no console
- [ ] Endpoint `/api/v1/system/info` retorna dados da API

## Próxima fase

Fase 1 — Auth e Perfil Profissional.

### Próximas entregas previstas

- Implementar autenticação JWT
- Implementar refresh token
- Implementar RBAC inicial com Admin/User
- Criar tela de login no Angular
- Criar perfil profissional do desenvolvedor
- Criar cadastro inicial de tecnologias e preferências

## Observação sobre uso responsável

Este projeto foi planejado para demonstrar arquitetura, IA aplicada e organização de oportunidades profissionais. Ele não deve ser usado para violar termos de plataformas externas, automatizar interações indevidas ou coletar dados sem permissão.
