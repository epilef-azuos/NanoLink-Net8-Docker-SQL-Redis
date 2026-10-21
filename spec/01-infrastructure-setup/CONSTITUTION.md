# Regras Invioláveis de Engenharia - Fase 01

1. **Eficiência de Build**: O `Dockerfile` DEVE usar multi-stage build. A imagem final não pode conter o SDK do .NET, apenas o ASP.NET Core Runtime.
2. **Segurança do Contêiner**: A imagem final da API deve rodar sob um usuário não-root (prática recomendada de segurança).
3. **Isolamento Constante**: A string de conexão no `appsettings.Development.json` pode apontar para o `localhost` (para debug local fora do Docker), mas o `docker-compose.yml` DEVE sobrescrever essa variável para usar a rede interna do Docker (ex: `Host=postgres_db;...`).