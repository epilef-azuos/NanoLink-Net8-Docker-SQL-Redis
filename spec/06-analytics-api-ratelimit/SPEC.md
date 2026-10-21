# Especificação de Negócio: Fase 06 - API de Métricas e Rate Limiting

## Objetivo
Disponibilizar uma API analítica agregada para visualização das métricas de acesso de cada link encurtado e proteger os endpoints contra abuso de tráfego utilizando o Rate Limiting nativo do .NET.

## Requisitos Funcionais
1. **Endpoint de Analytics**:
   - Rota: `GET /api/links/{code}/analytics`
   - Parâmetro: `{code}` do link encurtado.
   - Respostas:
     - `200 OK`: Retorna payload agregado com as estatísticas do link.
     - `404 Not Found`: Caso o link `{code}` não exista no sistema.
2. **Payload de Resposta do Analytics**:
   - Metadados do Link: `Code`, `OriginalUrl`, `CreatedAt`.
   - Métrica Geral: `TotalClicks`.
   - Agrupamentos por dimensão:
     - `ByCountry`: Lista de `{ CountryCode, CountryName, Count }`.
     - `ByDevice`: Lista de `{ DeviceType, Count }`.
     - `ByOperatingSystem`: Lista de `{ OperatingSystem, Count }`.
     - `ByReferrer`: Lista de `{ ReferrerDomain, Count }`.
     - `ClicksOverTime`: Lista diária dos últimos 7 dias `{ Date, Count }`.
3. **Rate Limiting (Proteção de Borda)**:
   - Aplicar política baseada em IP no endpoint de criação de links (`POST /api/links`).
   - Política: `FixedWindow` permitindo até 10 requisições por minuto por IP.
   - Resposta em caso de violação: HTTP `429 Too Many Requests` com cabeçalho `Retry-After`.

## Critérios de Aceitação
- [ ] O endpoint de analytics deve calcular agregações diretamente no banco sem carregar registros brutos para a memória.
- [ ] Links existentes sem nenhum clique registrado devem retornar `TotalClicks: 0` e listas vazias (sem erro 500).
- [ ] Requisições consecutivas no `POST /api/links` que excederem o limite configurado devem receber HTTP 429.