# 0001. Vertical Slice Architecture com Feature Folders

- **Status:** Aceita
- **Data:** 2026-10-21
- **Decisores:** Time NanoLink

## Contexto

Um encurtador de URLs tem poucas funcionalidades, mas cada uma atravessa várias camadas: validação, persistência, cache, publicação de eventos e resposta HTTP. Uma organização em camadas horizontais (`Controllers/`, `Services/`, `Repositories/`) espalha o código de uma mesma funcionalidade por várias pastas. Para entender ou alterar "criar link", seria preciso percorrer quatro ou cinco diretórios.

Com o crescimento das fases, o risco é duplicar lógica entre camadas e acoplar funcionalidades que não têm nada em comum, como o redirecionamento de alta performance e a agregação de analytics.

## Opções consideradas

1. **Camadas horizontais tradicionais** (Controllers, Services, Repositories).
2. **Clean Architecture** com projetos separados (Domain, Application, Infrastructure, API).
3. **Vertical Slice Architecture**: uma pasta por funcionalidade, contendo tudo o que ela precisa.

## Decisão

Adotamos **Vertical Slice Architecture com Feature Folders**. Cada funcionalidade vive em `src/NanoLink.Api/Features/<Nome>/` e contém seu próprio request, response, validator, handler e endpoint:

```text
Features/
  ShortenUrl/      ShortenUrlRequest.cs, ShortenUrlValidator.cs, ShortenUrlHandler.cs, ShortenUrlEndpoint.cs
  RedirectUrl/     RedirectUrlHandler.cs, RedirectUrlEndpoint.cs
  GetAnalytics/    GetAnalyticsHandler.cs, GetAnalyticsEndpoint.cs, GetAnalyticsResponse.cs
```

Código compartilhado por mais de uma fatia fica fora de `Features/`, em áreas de infraestrutura (`Infrastructure/`, `Data/`, `Common/`). Não criamos pastas genéricas de camada dentro de `Features/`. Essa regra é uma restrição da CONSTITUTION da fase 02.

## Consequências

### Positivas

- **Coesão:** tudo o que afeta "criar link" está em um lugar. Uma mudança de regra raramente toca outra fatia.
- **Baixo acoplamento entre funcionalidades:** o redirecionamento não depende dos tipos de analytics, e vice-versa.
- **Navegação simples:** a estrutura de pastas descreve os casos de uso do produto.
- **Crescimento linear:** adicionar uma funcionalidade é criar uma pasta, sem reorganizar o restante.

### Negativas

- **Possível duplicação:** duas fatias podem repetir pequenos trechos de código. Aceitamos a duplicação localizada em vez de criar abstrações prematuras.
- **Disciplina necessária:** sem uma regra clara, código compartilhado pode crescer dentro de uma fatia e virar dependência oculta. A fronteira entre fatia e infraestrutura precisa ser vigiada em revisão.
- **Menos familiar para quem vem de MVC:** a estrutura exige explicação para equipes acostumadas a camadas horizontais.
