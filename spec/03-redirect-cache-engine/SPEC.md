# Especificação de Negócio: Fase 03 - Motor de Redirecionamento de Baixa Latência

## Objetivo
Implementar o endpoint de alta vazão para resolução de URLs encurtadas, garantindo redirecionamento HTTP com latência inferior a 5ms via Redis, com proteção contra penetração de cache através de *Negative Caching*.

## Requisitos Funcionais
1. **Endpoint**: `GET /{code}`
2. **Entrada**: Parâmetro de rota `{code}` (alfanumérico, 7 a 10 caracteres).
3. **Fluxo de Resolução**:
   - **Passo 1 (Cache L1/Redis)**: Consulta a chave `link:{code}` no Redis.
   - **Passo 2 (Cache Hit Válido)**: Retorna imediatamente `302 Found` com cabeçalho `Location: {originalUrl}`.
   - **Passo 3 (Negative Cache Hit)**: Se o valor no Redis for o marcador de inexistência (`__NOT_FOUND__`), encerra imediatamente com `404 Not Found`.
   - **Passo 4 (Cache Miss)**: Consulta o PostgreSQL via EF Core (`AsNoTracking`).
   - **Passo 5 (Persistência no Cache)**:
     - Se encontrado no banco: Grava no Redis `link:{code} = {originalUrl}` com TTL de 24 horas + Jitter aleatório (de 0 a 1800 segundos para evitar avalanche de expiração).
     - Se NÃO encontrado: Grava no Redis `link:{code} = __NOT_FOUND__` com TTL curto de 60 segundos (*Negative Caching*).
4. **Respostas HTTP**:
   - `302 Found` com `Location` para links existentes.
   - `404 Not Found` (ProblemDetails) para links inexistentes.
   - `400 Bad Request` para códigos com tamanho ou caracteres inválidos.

## Critérios de Aceitação
- [ ] O endpoint deve retornar status 302 apontando para a URL de destino registrada.
- [ ] O segundo acesso ao mesmo `{code}` deve ser resolvido 100% em memória via Redis sem disparar queries no PostgreSQL.
- [ ] Códigos inexistentes devem retornar 404 e gravar a chave sentinela no Redis com expiração curta.