# Checklist de Tarefas Atômicas - Fase 05

- [x] **T1**: Instalar pacotes NuGet `MaxMind.GeoIP2` e `UAParser`.
- [x] **T2**: Atualizar a entidade `ClickEvent.cs` com os campos enriquecidos e atualizar o schema no `AppDbContext`.
- [x] **T3**: Implementar o leitor de geolocalização offline `MaxMindGeoLocationResolver.cs` com tratamento seguro para IPs locais e privados.
- [x] **T4**: Implementar `AnalyticsEnricher.cs` para normalização de User-Agent (`DeviceType`, `OperatingSystem`) e extração do domínio de `Referer`.
- [x] **T5**: Integrar a etapa de enriquecimento no `AnalyticsBatchWorker.cs` antes do `AddRangeAsync`.
- [x] **T6**: Registrar os novos serviços no container de injeção de dependências em `Program.cs`.
- [x] **T7**: Atualizar `requests.http` com cabeçalhos `X-Forwarded-For` ou IPs simulados para teste.
- [x] **T8**: Marcar todos os itens pendentes deste `TASK.md` com `[x]`.
- [x] **T9**: Compilar o projeto com `dotnet build` e realizar o commit semântico: `feat: adiciona enriquecimento offline de geolocalizacao e metadados de analytics`.