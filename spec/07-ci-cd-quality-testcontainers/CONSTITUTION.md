# Regras Invioláveis de Engenharia - Fase 07

1. **Proibição de Mocks para Infraestrutura nos Testes de Integração**: É terminantemente proibido simular o PostgreSQL ou o Redis em memória com Dictionary ou Mocking Frameworks nos testes de integração. Os testes DEVEM validar o comportamento real contra instâncias de contêineres reais via Testcontainers.
2. **Determinismo e Isolamento**: Nenhum teste deve interferir no estado do outro. Cada teste ou suite deve usar códigos/URLs únicos ou limpar seu contexto.
3. **Bloqueio no CI por Formatação**: Commits fora do padrão do `dotnet format` devem quebrar a pipeline imediatamente na esteira de CI.
4. **Governança TASK.md**: Atualizar individualmente cada tarefa com `[x]` à medida que for finalizada e validada.