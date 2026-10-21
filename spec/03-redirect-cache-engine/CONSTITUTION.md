# Regras Invioláveis de Engenharia - Fase 03

1. **Uso Exclusivo de HTTP 302 Found**: É estritamente proibido retornar `301 Moved Permanently`. O status 301 faz com que os navegadores guardem o redirecionamento em cache local, inviabilizando a coleta de analytics nas próximas fases.
2. **Proibição de Bloqueio de Thread**: O método de busca no Redis e no PostgreSQL DEVE ser 100% assíncrono (`async`/`await`).
3. **Negative Caching Mandatório**: Todo acesso a código inexistente DEVE persistir a chave sentinela no Redis para impedir ataques de esgotamento de conexões ao PostgreSQL (*Cache Penetration*).
4. **Ausência de Deserialização no Caminho Crítico**: Não utilizar serializadores JSON para ler ou gravar o link original no Redis; trabalhar diretamente com `RedisValue`.