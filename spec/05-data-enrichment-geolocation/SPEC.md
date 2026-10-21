# Especificação de Negócio: Fase 05 - Enriquecimento de Dados (Geolocalização & Metadados)

## Objetivo
Enriquecer os eventos brutos de clique com geolocalização física (País e Cidade) de forma 100% offline e metadados normalizados de cliente (Dispositivo, Sistema Operacional e Domínio de Origem), sem onerar a latência do redirecionamento HTTP.

## Requisitos Funcionais
1. **Geolocalização Offline (MaxMind GeoLite2)**:
   - Resolução de `CountryCode` (ex: "BR"), `CountryName` (ex: "Brazil") e `CityName` (ex: "Guarulhos") a partir do `IpAddress`.
   - Tratamento de IPs de loopback/redes privadas (`127.0.0.1`, `::1`, `10.x.x.x`, `192.168.x.x`) marcando como "Local Network".
   - Execução 100% offline via leitor MMDB local, sem qualquer dependência de APIs externas.
2. **Normalização de User-Agent**:
   - Extração do Sistema Operacional: `OperatingSystem` (ex: "Windows", "iOS", "Android", "Linux", "macOS", "Outro").
   - Classificação do Dispositivo: `DeviceType` ("Mobile", "Desktop", "Tablet", "Bot").
3. **Normalização de Origem (Referrer)**:
   - Extração do domínio limpo em `ReferrerDomain` (ex: "t.co", "linkedin.com", "google.com", "Direct").
4. **Local de Execução do Enriquecimento**:
   - O enriquecimento DEVE ocorrer em segundo plano no `AnalyticsBatchWorker` antes da inserção do lote no PostgreSQL, preservando o tempo de resposta do endpoint `GET /{code}`.

## Critérios de Aceitação
- [ ] O banco de dados passa a persistir os campos normalizados na tabela `ClickEvents`.
- [ ] Requisições com IPs de teste ou headers reais são enriquecidas com país, cidade, SO e tipo de dispositivo.
- [ ] Nenhum erro ou exceção quebra o lote caso o IP ou User-Agent sejam nulos ou desconhecidos (valores padrão: "Unknown").