# Especificação de Negócio: Fase 07 - CI/CD e Testes com Testcontainers

## Objetivo
Garantir a integridade, conformidade de estilo e robustez funcional do NanoLink através de uma suíte de testes de integração reais (utilizando contêineres efêmeros de PostgreSQL e Redis via Testcontainers) e uma esteira automatizada de CI via GitHub Actions.

## Requisitos Funcionais
1. **Projeto de Testes**:
   - Criação do projeto `NanoLink.IntegrationTests` com xUnit.
   - Utilização de `WebApplicationFactory<Program>` para subir o host da API em memória.
   - Ciclo de vida gerenciado com `Testcontainers.PostgreSql` e `Testcontainers.Redis`.
2. **Cenários Mínimos de Teste de Integração**:
   - **Fluxo Completo de Criação e Redirecionamento**:
     - `POST /api/links` com URL válida -> retorna 201 e persiste no Postgres do container.
     - `GET /{code}` -> retorna 302 com o cabeçalho `Location` correto e armazena a chave no Redis do container.
     - `GET /{code}` subsequente -> responde via cache (latência reduzida).
   - **Fluxo Negativo**:
     - `POST /api/links` com payload vazio/inválido -> retorna 400 Bad Request.
     - `GET /codigo_inexistente` -> retorna 404 e aplica negative caching.
   - **Fluxo de Analytics**:
     - Disparar requisições para o link criado, aguardar o flush do batch worker e validar `GET /api/links/{code}/analytics`.
3. **Pipeline de Integração Contínua (GitHub Actions)**:
   - Disparo automático em `push` e `pull_request` nas branches `main` e `feature/*`.
   - Etapas obrigatórias:
     1. Checkout do código.
     2. Setup do SDK .NET 9.
     3. Restauração de dependências (`dotnet restore`).
     4. Verificação de formatação e lint (`dotnet format --verify-no-changes`).
     5. Compilação da Solution (`dotnet build --no-restore`).
     6. Execução dos testes de integração com Docker Daemon disponível (`dotnet test --no-build`).

## Critérios de Aceitação
- [ ] Todos os testes de integração devem executar de forma determinística sem depender de instâncias externas prévias de banco ou Redis instaladas na máquina hospedeira.
- [ ] O workflow do GitHub Actions deve passar com status verde em todas as etapas.
- [ ] O código deve respeitar a formatação oficial do `dotnet format`.