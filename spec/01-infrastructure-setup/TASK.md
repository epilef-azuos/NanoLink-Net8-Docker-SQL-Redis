# Checklist de Execução Atômica - Fase 01

- [ ] **T1**: Criar a solution (`dotnet new sln`) e o projeto (`dotnet new webapi -n NanoLink.Api`).
- [ ] **T2**: Adicionar o projeto à solution.
- [ ] **T3**: Criar o `.dockerignore` e o `.gitignore`.
- [x] **T4**: Escrever o `Dockerfile` multi-stage dentro de `/src/NanoLink.Api`.
- [x] **T5**: Criar o arquivo `docker-compose.yml` na raiz do repositório, configurando Postgres, Redis e a API.
- [x] **T6**: Adicionar um endpoint simples de "Hello World" em `Program.cs` apenas para validação de subida.
- [ ] **T7**: Testar a subida completa com `docker-compose up --build` e acessar o endpoint na porta 8080.
- [x] **T8**: Commit semântico: `chore: setup docker e docker-compose com postgres e redis`.