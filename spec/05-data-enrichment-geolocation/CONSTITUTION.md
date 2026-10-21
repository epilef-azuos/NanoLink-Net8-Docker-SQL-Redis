# Regras Invioláveis de Engenharia - Fase 05

1. **Zero Chamadas HTTP Externas**: É proibido o uso de `HttpClient` para buscar dados de geolocalização (ex.: ip-api, ipinfo). Todo lookup DEVE ser estritamente local usando o leitor MMDB em memória/disco.
2. **Desacoplamento do Caminho Crítico**: O método `Enrich(...)` DEVE ser executado exclusivamente dentro do `AnalyticsBatchWorker` (Background), mantendo a thread do `GET /{code}` restrita ao redirecionamento 302.
3. **Resiliência a Falhas de Lookup**: Se o arquivo de banco MMDB não for encontrado ou o IP for inválido, o serviço deve assumir valores padrão ("Unknown" ou "Local Network") com log em nível Warning, sem abortar a persistência do evento.
4. **Governança TASK.md**: É obrigatório atualizar o checklist do `TASK.md` marcando com `[x]` cada etapa concluída.