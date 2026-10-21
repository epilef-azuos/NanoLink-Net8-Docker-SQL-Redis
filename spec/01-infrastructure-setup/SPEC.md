# Especificação de Negócio: Fase 01 - Infraestrutura As Code

## Objetivo
Estabelecer um ambiente de desenvolvimento local determinístico, reproduzível e isolado, garantindo que a aplicação .NET 8, o banco de dados PostgreSQL e o cache Redis subam orquestrados em contêineres Docker.

## Requisitos
1. A API deve rodar dentro de um contêiner Docker gerado por um `Dockerfile` multi-stage (otimizando o tamanho da imagem final).
2. O ambiente local deve ser orquestrado por um `docker-compose.yml` que inclua:
   - Serviço da API (expondo a porta 8080 localmente).
   - Serviço do PostgreSQL 16 (expondo a porta 5432).
   - Serviço do Redis 7 (expondo a porta 6379).
3. Configuração de variáveis de ambiente no Compose injetadas na API (Connection Strings do Postgres e Redis).

## Critérios de Aceite
- [ ] O comando `docker-compose up --build` deve subir os três serviços sem erros.
- [ ] O Swagger ou um endpoint de *health check* da API deve estar acessível via `http://localhost:8080/`.
- [ ] A API deve conseguir se conectar com sucesso ao PostgreSQL e ao Redis declarados no Compose.