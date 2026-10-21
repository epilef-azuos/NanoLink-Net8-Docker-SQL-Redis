# Especificação de Negócio: Fase 02 - Criação de URLs Encurtadas

## Objetivo
Disponibilizar um endpoint HTTP POST de alto desempenho para registar URLs de destino e devolver uma URL encurtada com identificador alfanumérico único em Base62.

## Requisitos Funcionais
1. **Endpoint**: `POST /api/links`
2. **Entrada**: JSON com o atributo `originalUrl`.
3. **Validação**:
   - `originalUrl` obrigatório, com formato URI absoluto válido (esquemas `http` ou `https`).
   - Comprimento máximo de 2048 carateres.
4. **Geração de Código**:
   - Identificador alfanumérico Base62 `[a-zA-Z0-9]` com comprimento fixo de 7 carateres.
   - Geração segura sem colisões previsíveis.
5. **Saída**:
   - Código HTTP `201 Created`.
   - Headers: `Location: /api/links/{code}`.
   - Payload JSON: `code`, `shortUrl`, `originalUrl`, `createdAt`.

## Critérios de Aceitação
- [ ] Retornar HTTP 201 com payload padronizado para URLs válidas.
- [ ] Retornar HTTP 400 Bad Request detalhando erros para URLs malformadas ou vazias.
- [ ] O registo deve ser persistido na tabela `Links` do PostgreSQL via EF Core.