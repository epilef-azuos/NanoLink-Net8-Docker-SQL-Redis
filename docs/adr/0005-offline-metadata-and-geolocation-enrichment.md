# 0005. Enriquecimento de geolocalização e metadados offline

- **Status:** Aceita
- **Data:** 2026-10-21
- **Decisores:** Time NanoLink

## Contexto

Cada clique é enriquecido com país, cidade, sistema operacional, tipo de dispositivo e domínio de origem. Esses dados alimentam as agregações do analytics.

A forma mais direta de obter país e cidade a partir de um IP é chamar uma API de geolocalização de terceiros, como ip-api ou ipinfo. Isso traz três problemas: cada clique passaria a depender de uma rede externa, a latência e a disponibilidade do enriquecimento ficariam fora do nosso controle, e os IPs dos usuários seriam enviados a um serviço externo. Além disso, a maioria dessas APIs tem limites de requisições incompatíveis com o volume de cliques.

Parsear User-Agent também pode depender de serviços externos, mas isso é desnecessário: a leitura é determinística e pode ser feita localmente.

## Opções consideradas

Para geolocalização:

1. **API HTTP de terceiros**, consultada por clique.
2. **Base local MaxMind GeoLite2** (formato MMDB), lida em memória.
3. **Tabela própria** de faixas de IP mantida manualmente.

Para User-Agent e Referer:

1. **Serviço externo** de parse de User-Agent.
2. **Biblioteca local** UAParser e parse de URL em .NET.

## Decisão

**Geolocalização:** usamos o leitor MaxMind (`MaxMind.GeoIP2` 6.x) sobre um arquivo `GeoLite2-City.mmdb`. O `MaxMindGeoLocationResolver` é um singleton que carrega o `DatabaseReader` uma vez e o reutiliza, porque o leitor é thread-safe.

**Caminhos de falha tratados com segurança:**

- Arquivo MMDB ausente: a API sobe normalmente, registra um aviso uma única vez no startup e devolve `Unknown` para país e cidade.
- IP nulo, inválido ou de loopback/rede privada (`127.0.0.0/8`, `::1`, `10/8`, `172.16/12`, `192.168/16`, `169.254/16`, `fc00::/7`, `fe80::/10`): devolve `Local Network`, sem consultar a base.
- IP não encontrado na base: devolve `Unknown`.

**User-Agent e Referer:** `AnalyticsEnricher` usa `UAParser` 3.x para extrair o sistema operacional e classificar o dispositivo como `Mobile`, `Tablet`, `Desktop` ou `Bot`. O domínio do Referer é extraído com `Uri.TryCreate`, sem o prefixo `www.`. Referer ausente vira `Direct`.

**Onde o enriquecimento roda:** exclusivamente no `AnalyticsBatchWorker`, em segundo plano, antes da gravação. O endpoint de redirect não chama nenhuma parte do enriquecimento.

## Consequências

### Positivas

- **Offline:** nenhum IP sai do servidor, e o enriquecimento não tem dependência de rede externa.
- **Previsível:** a latência de consulta é de memória, sem variação de rede.
- **Sem custo por requisição** de API de terceiros.
- **Resiliente:** a ausência da base degrada a geolocalização, mas não interrompe a API nem a gravação dos cliques.
- **Privacidade:** o IP fica no banco da própria aplicação, sem ser enviado a terceiros para consulta.

### Negativas

- **Dados e licença:** a base GeoLite2 exige cadastro e aceite de licença na MaxMind, e não é versionada no repositório. Cada ambiente precisa obter o arquivo.
- **Precisão limitada:** bases gratuitas têm precisão menor que as pagas, especialmente para cidades.
- **Atualização manual:** a base envelhece e precisa ser atualizada periodicamente.
- **UAParser depende de regras:** a classificação de dispositivo é heurística. Por exemplo, Android sem o token `Mobile` é classificado como `Tablet`, e ferramentas como `curl` caem em `Desktop`.
- **Não validado com base real:** nos testes, todos os IPs públicos resultaram em `Unknown`, porque o arquivo MMDB não estava presente. O caminho de resolução de país e cidade ainda precisa de teste com a base real.
