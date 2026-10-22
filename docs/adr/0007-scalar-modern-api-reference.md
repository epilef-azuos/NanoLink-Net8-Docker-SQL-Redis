# 0007. Scalar sobre o OpenAPI nativo do ASP.NET Core

- **Status:** Aceita
- **Data:** 2026-10-21
- **Decisores:** Time NanoLink

## Contexto

A API precisa de documentação interativa para facilitar testes manuais e a integração de quem consome o encurtador. A documentação deve descrever os endpoints reais, incluindo rotas, tags, códigos de resposta e payloads.

O ecossistema .NET tem uma escolha histórica popular: o Swashbuckle, que gera o documento OpenAPI e serve uma UI Swagger. Ele continua funcional, mas depende de um pacote de terceiros para a geração do documento, e a interface Swagger UI tem uma aparência datada.

No .NET 9, o ASP.NET Core passou a incluir suporte nativo a OpenAPI, com `AddOpenApi` e `MapOpenApi`, e a UI Scalar é uma alternativa moderna, distribuída como pacote.

Nota de contexto: o NanoLink nunca usou Swashbuckle. Esta ADR registra a escolha de não adotá-lo, e não uma migração a partir dele.

## Opções consideradas

1. **Swashbuckle** (Swagger UI): biblioteca de terceiros consolidada, com UI clássica.
2. **Microsoft.AspNetCore.OpenApi + Scalar**: geração nativa do framework e UI moderna.
3. **Nenhuma documentação**: apenas o README com a tabela de endpoints.

## Decisão

Adotamos o **suporte nativo do ASP.NET Core 9** para a geração do documento OpenAPI (`Microsoft.AspNetCore.OpenApi` 9.0.x, com `AddOpenApi` e `MapOpenApi`), e a **UI Scalar** (`Scalar.AspNetCore` 2.x) para a interface interativa.

A documentação é exposta apenas em `Development`:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("NanoLink API")
               .WithTheme(ScalarTheme.Moon)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}
```

Os endpoints são organizados com `WithTags` (`Links` e `Analytics`) e `WithSummary`, para que a UI agrupe as operações.

A UI fica em `/scalar/v1` e o documento em `/openapi/v1.json`.

## Consequências

### Positivas

- **Menos dependências de terceiros:** a geração do documento é do próprio framework, o que reduz o risco de incompatibilidade entre versões de biblioteca e do runtime.
- **Documento sempre fiel ao código:** o documento é gerado a partir dos metadados dos endpoints em tempo de execução, sem arquivo a manter à parte.
- **UI moderna e legível:** o Scalar tem uma interface mais limpa que a Swagger UI clássica, com cliente HTTP integrado.
- **Testável:** a suíte de integração valida que o documento e a UI respondem (`DocumentationIntegrationTests`).
- **Exposição controlada:** restringir a `Development` evita divulgar a superfície completa da API em produção.

### Negativas

- **Ausente em produção:** como a documentação só existe em `Development`, o container do compose (que roda em `Production`) não a expõe. Quem quiser documentação publicada precisa de uma decisão de exposição.
- **Ecossistema mais novo:** o Scalar tem menos anos de uso que o Swagger UI, e a API de configuração (`ScalarTarget`, `ScalarClient`) pode mudar entre versões major.
- **Dependência de versão do framework:** o suporte nativo acompanha o .NET 9. Migrar para outra versão de runtime exigirá verificar a compatibilidade do `Microsoft.AspNetCore.OpenApi`.
- **Menos extensibilidade pronta:** recursos de terceiros do Swashbuckle, como filtros e geradores de cliente, precisam ser reescritos com as APIs nativas quando necessários.
